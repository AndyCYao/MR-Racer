/*
 This file is part of the Structure SDK.
 Copyright © 2018 Occipital, Inc. All rights reserved.
 http://structure.io
 */

#import <UIKit/UIKit.h>
#import <GLKit/GLKit.h>
#import <ReplayKit/ReplayKit.h>

#import "UnityAppController.h"
#import "UnityAppController+ViewHandling.h"
#import "UnityAppController+Rendering.h"
#import "Unity/DisplayManager.h"
#import "UnityView.h"

#import "iPhone_Sensors.h"
#import <GLKit/GLKit.h>
#import "BridgeEngineUnity.h"
#import "BridgeEngineAppController.h"

#include "IUnityInterface.h"
#include "IUnityGraphics.h"

#import <BridgeEngine/BridgeEngine.h>
#import <BridgeEngine/BEDebugging.h>
#import <BridgeEngine/BEProfiling.h>
#import <BridgeEngine/BEAppSettings.h>

#import <BridgeEngine/BEMesh.h>

#import "BEUnityControllerInterop.h"

//------------------------------------------------------------------------------
#pragma mark - Static Vars

//Callbacks for Rendering in Unity
static BETrackerEventCallback trackerBridgeEngineEventCallback;

/// BridgeEngine singleton
static BridgeEngineUnity* currentEngine = NULL;

// Latest tracker update data in a struct shared with Unity C#
static BETrackerUpdateInterop trackerUpdateInterop = {};

//--------
// Forward declare a GVROverlayView class for identification.
@class GVROverlayView<NSObject>;

//------------------------------------------------------------------------------

#pragma mark - BridgeEngineUnity ()

@interface BridgeEngineUnity ()
<
BEMixedRealityModeDelegate,
RPPreviewViewControllerDelegate,
BEControllerDelegate
>

@property (nonatomic, weak) UnityView *unityView;
@property (nonatomic, weak) UIViewController *unityVC;

@property (nonatomic, strong) BEUnityInteropController *interopController;

@end

//------------------------------------------------------------------------------

#pragma mark - BridgeEngineUnity

@implementation BridgeEngineUnity
{
    BEMixedRealityMode* _mixedReality;
    BEMixedRealityPrediction* _predictionAtDisplayLinkStart;

    // Support Metal API rendering mode.
    id <MTLDevice> _device;
    CVMetalTextureCacheRef _textureCache;
    id<MTLTexture> _metalTexture;
}

/**
 * Initialize BridgeEngine in headless tracking only mode.
 * Updates on tracking are run entirely via calls to onDisplayLink. 
 */
- (instancetype) initWithUnityView:(UnityView *)unityView unityVC:(UIViewController*)unityVC
{
    self = [super init];
    if (self)
    {
        // Three-finger tap recognizer (for Start/Stop ReplayKit recording)
        UITapGestureRecognizer *threeFingerTapRecognizer = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(handleScreenRecordTap:)];
        threeFingerTapRecognizer.numberOfTouchesRequired = 3;
        threeFingerTapRecognizer.cancelsTouchesInView = YES;
        [unityView addGestureRecognizer:threeFingerTapRecognizer];

        _unityView = unityView;
        _unityVC = unityVC;
        
        currentEngine = self;
        
        BESetVerbosityLevel(0);

        BOOL useWVL = [BEAppSettings booleanValueFromAppSetting:SETTING_USE_WVL defaultValueIfSettingIsNotInBundle:YES];
        BOOL useColorOnlyMode = [BEAppSettings booleanValueFromAppSetting:SETTING_COLOR_CAMERA_ONLY defaultValueIfSettingIsNotInBundle:NO];
        BOOL autoExposeCamera = [BEAppSettings booleanValueFromAppSetting:SETTING_AUTO_EXPOSE_DURING_RELOC defaultValueIfSettingIsNotInBundle:NO];

        NSDictionary *options = @{
                                  kBECaptureReplayMode:  @(BridgeEngineAppController.replayMode),
                                  kBEUsingWideVisionLens:   @(useWVL),
                                  kBEStereoRenderingEnabled: @(be_isStereoMode()),
                                  kBEUsingColorCameraOnly:  @(useColorOnlyMode),
                                  kBEAutoExposeWhileRelocalizing:  @(autoExposeCamera)                                  
                                  };
        
        // REQUIRED for Unity Metal API:
        // BridgeEngine/Unity requires an OpenGL ES context created, even if it's not used.
        EAGLSharegroup *sharegroup = nil;
        
        if(UnitySelectedRenderingAPI() == apiOpenGLES2)
        {
            EAGLContext *unityContext = UnityGetDataContextEAGL();
            be_assert(unityContext != nil, "We need a context here to give the sharegroup to BE.");
            sharegroup = [unityContext sharegroup];
        }

        be_assert(UnitySelectedRenderingAPI() != apiOpenGLES3, "OpenGL ES3 is not supported on Bridge Engine.");

        if(UnitySelectedRenderingAPI() == apiMetal) {
            [self setupMetal];
        }

        _mixedReality = [[BEMixedRealityMode alloc]
                         initWithView:nil
                         engineOptions:options
                         markupNames:nil
                         eaglSharegroup:sharegroup
                         ];

        _mixedReality.delegate = self;
        
        // Activate the controller interop, wire all delegates to callbacks.
        self.interopController = [[BEUnityInteropController alloc] init];
        
        [_mixedReality startWithSavedScene];
    }
    return self;
}

- (void)dealloc
{
    if(UnitySelectedRenderingAPI() == apiMetal) {
        [self teardownMetal];
    }

    if ([_mixedReality delegate] == self)
        _mixedReality.delegate = self;
    
    if ([[BEController sharedController] delegate] == self)
        [BEController sharedController].delegate = nil;
}

//------------------------------------------------------------------------------

#if BE_PROFILING
namespace BEMonitor
{
    BE::PerformanceMonitor renderingTime {"[Rendering]"};
}
#endif

/**
 * Get the coarseMesh.
 */
- (BEMesh*) coarseMesh {
    return _mixedReality.coarseMesh;
}

- (void)updateVR:(BOOL)useVR andCamera:(BOOL)useCameraTexture
{
    if (useVR || useCameraTexture)
    {
        BEMixedRealityRenderData *renderData = [_mixedReality renderDataForPrediction:_predictionAtDisplayLinkStart requiresTexture:useCameraTexture];
        
        if (useVR)
        {
            {
                GLKMatrix4 unityMatrix = BE2UnityMatrix(renderData.leftEyePose);
                
                trackerUpdateInterop.stereoSetup.leftPosePosition = GLKMatrix4ToPositionVector3(unityMatrix);
                trackerUpdateInterop.stereoSetup.leftPoseRotation = GLKMatrix4ToVector4Quaternion(unityMatrix);
                
                GLKMatrix4 unityProjMatrix = BE2UnityProjMatrix(renderData.leftEyeGLProjection);
                trackerUpdateInterop.stereoSetup.leftProjection = GLKMatrix4ToBEMatrix4(unityProjMatrix);
            }
            
            {
                GLKMatrix4 unityMatrix = BE2UnityMatrix(renderData.rightEyePose);
                
                trackerUpdateInterop.stereoSetup.rightPosePosition = GLKMatrix4ToPositionVector3(unityMatrix);
                trackerUpdateInterop.stereoSetup.rightPoseRotation = GLKMatrix4ToVector4Quaternion(unityMatrix);
                
                GLKMatrix4 unityProjMatrix = BE2UnityProjMatrix(renderData.rightEyeGLProjection);
                trackerUpdateInterop.stereoSetup.rightProjection = GLKMatrix4ToBEMatrix4(unityProjMatrix);
            }
        }
        
        if (useCameraTexture)
        {
            if( UnitySelectedRenderingAPI() == apiMetal ) {
                _metalTexture = [self createMetalTextureFromRenderData:renderData];
                trackerUpdateInterop.cameraTextureInfo.width = (UInt32)_metalTexture.width;
                trackerUpdateInterop.cameraTextureInfo.height = (UInt32)_metalTexture.height;
                trackerUpdateInterop.cameraTextureInfo.texturePtr = (__bridge_retained void*)_metalTexture;
            } else {
                trackerUpdateInterop.cameraTextureInfo.texturePtr = (void*)renderData.mixedRealityRgbaTexture;
                trackerUpdateInterop.cameraTextureInfo.width = renderData.mixedRealityRgbaTextureWidth;
                trackerUpdateInterop.cameraTextureInfo.height = renderData.mixedRealityRgbaTextureHeight;
            }

            bool isInvertible;
            trackerUpdateInterop.cameraTextureInfo.texturePerspectiveProj = GLKMatrix4ToBEMatrix4(BE2UnityProjMatrix(_predictionAtDisplayLinkStart.colorFrameGLProjection));
            trackerUpdateInterop.cameraTextureInfo.textureViewpoint = GLKMatrix4ToBEMatrix4(BE2UnityMatrix(GLKMatrix4Invert(_predictionAtDisplayLinkStart.associatedColorFramePose, &isInvertible)));
            
            /*
             This is a temporary fix that allows us to compensate for the conversion between CV <> Unity that is done
             in this class and on the Unity side.  The xy scale axis are inverted in Unity, and thus must be inverted here too.
             
             The correct fix is likely a right -> left hand conversion on the BEScene data here in the `be_loadMeshes` function
             which is currently only flipping the y-axis.
             */
            if(     ::isnan(_predictionAtDisplayLinkStart.associatedColorFramePose.m[0]) == false
               &&   ::isnan(_predictionAtDisplayLinkStart.associatedColorFramePose.m[12]) == false)
            {
                GLKMatrix4 unityMatrix = BE2UnityMatrix(_predictionAtDisplayLinkStart.associatedColorFramePose);
                
                // Position
                trackerUpdateInterop.cameraTextureInfo.position = GLKMatrix4ToPositionVector3(unityMatrix);
                
                // Scale
                trackerUpdateInterop.cameraTextureInfo.scale = GLKMatrix4ToScaleVector3(unityMatrix);
                trackerUpdateInterop.cameraTextureInfo.scale.x = -trackerUpdateInterop.cameraTextureInfo.scale.x;
                trackerUpdateInterop.cameraTextureInfo.scale.y = -trackerUpdateInterop.cameraTextureInfo.scale.y;
                
                // Rotation
                beVector4 newRot = GLKMatrix4ToVector4Quaternion(unityMatrix);
                trackerUpdateInterop.cameraTextureInfo.rotationQuaternion = newRot;
            }
        }
        else {
            trackerUpdateInterop.cameraTextureInfo.texturePtr = 0;
        }
    }
}

- (void)setupMetal
{
    _device = MTLCreateSystemDefaultDevice();
    if( _textureCache == nil ) {
        CVMetalTextureCacheCreate(kCFAllocatorDefault, NULL, _device, NULL, &_textureCache);
    }
}

- (void)teardownMetal
{
    if (_textureCache) {
        CFRelease(_textureCache);
        _textureCache = nil;
    }
}

- (id<MTLTexture>) createMetalTextureFromRenderData:(BEMixedRealityRenderData *)renderData {
    CVPixelBufferRef pixelBuffer = renderData.mixedRealityRgbaTextureBuffer;

    if( CVPixelBufferGetPixelFormatType(pixelBuffer) != kCVPixelFormatType_32BGRA ) {
        NSLog(@"BridgeEngineUnity: Failed to create metal texture, incorrect pixel format.");
        return nil;
    }

    // texture
    const size_t width = CVPixelBufferGetWidth(pixelBuffer);
    const size_t height = CVPixelBufferGetHeight(pixelBuffer);
    
    MTLPixelFormat pixelFormat;
    UnityDisplaySurfaceBase* surf = GetMainDisplaySurface();

    if( surf->srgb ) {
        // Linear
        pixelFormat = MTLPixelFormatRGBA8Unorm_sRGB;
    } else
    {   // Gamma
        pixelFormat = MTLPixelFormatRGBA8Unorm;
    }
    

    CVMetalTextureRef textureRef = NULL;
    CVReturn status = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                                                                _textureCache,
                                                                pixelBuffer, NULL,
                                                                pixelFormat,
                                                                width, height, 0, &textureRef);
    if(status == kCVReturnSuccess)
    {
        id<MTLTexture>texture = CVMetalTextureGetTexture(textureRef);
        CFRelease(textureRef);
        return texture;
    } else {
        return nil;
    }
}


- (void)updateTracking
{
    // Update with latest pose accuracy
    auto lastHints = _mixedReality.lastTrackerHints;

    // Watch for Orientation only tracking, override to pose "Not Available"
    if( lastHints.isOrientationOnly ) {
        trackerUpdateInterop.trackerPoseAccuracy = BETrackerPoseAccuracyNotAvailable; 
    } else {
        trackerUpdateInterop.trackerPoseAccuracy = _mixedReality.lastTrackerPoseAccuracy;
    }

    trackerUpdateInterop.trackerHints.isOrientationOnly = lastHints.isOrientationOnly;
    trackerUpdateInterop.trackerHints.modelVisibilityPercentage = lastHints.modelVisibilityPercentage;
    trackerUpdateInterop.trackerHints.mappedAreaNotVisible = lastHints.mappedAreaNotVisible;
}

/**
 * Hook the display link interface,
 * crunch the latest camera pose estimate for next frame,
 * and report this to Unity.
 */
- (void)onDisplayLink
{
    GLKMatrix4 predictedColorCameraPose = nanGLKMatrix4;
    GLKMatrix4 predictedColorCameraProj = nanGLKMatrix4;
    
    const bool showHeavyTrackingDebug = false;
    if(showHeavyTrackingDebug) {
    	be_dbg("onDisplayLink");
    }
    
    {
        NSTimeInterval displayLinkStart = CACurrentMediaTime();
        _predictionAtDisplayLinkStart = [_mixedReality predictColorCameraPoseForDisplayLinkStart:displayLinkStart];
        
        if (showHeavyTrackingDebug)
        {
            be_dbg("_mixedReality.lastTrackerPoseAccuracy: %d", (int)_mixedReality.lastTrackerPoseAccuracy);
            
            const auto hints = _mixedReality.lastTrackerHints;
            be_dbg("[visibility=%f] [isOrientationOnly=%d] [mappedAreaNotVisible=%d]",
                   hints.modelVisibilityPercentage,
                   hints.isOrientationOnly,
                   hints.mappedAreaNotVisible);
            
            const auto sensorsStatus = _mixedReality.sensorsStatus;
            be_dbg("[allSensorsReady=%d] [needToAuthorizeCamera=%d] [needToConnectDepthSensor=%d] [needToRunCalibrator=%d] [needToChargeDepthSensor=%d]",
                   sensorsStatus.allSensorsReady,
                   sensorsStatus.needToAuthorizeIOSCamera,
                   sensorsStatus.needToConnectDepthSensor,
                   sensorsStatus.needToRunCalibrator,
                   sensorsStatus.needToChargeDepthSensor);
            
            be_dbg("Could predict = %d", _predictionAtDisplayLinkStart.couldPredict);
        }
        
        if (_predictionAtDisplayLinkStart.couldPredict)
        {
            predictedColorCameraPose = _predictionAtDisplayLinkStart.predictedColorCameraPose;
            predictedColorCameraProj = _predictionAtDisplayLinkStart.predictedColorCameraProjection;
        }
    }
    
    // Get Pose
    if(     ::isnan(predictedColorCameraPose.m[0]) == false
       &&   ::isnan(predictedColorCameraPose.m[12]) == false)
    {
        GLKMatrix4 unityMatrix = BE2UnityMatrix(predictedColorCameraPose);
        
        trackerUpdateInterop.position = GLKMatrix4ToPositionVector3(unityMatrix);
        trackerUpdateInterop.scale = GLKMatrix4ToScaleVector3(unityMatrix);
        
        beVector4 newRot = GLKMatrix4ToVector4Quaternion(unityMatrix);
        
        trackerUpdateInterop.rotationQuaternion = newRot;
        trackerUpdateInterop.timestamp = msecFromMachAbsoluteTime();
        
        GLKMatrix4 unityProjMatrix = BE2UnityProjMatrix(predictedColorCameraProj);
        trackerUpdateInterop.cameraProj = GLKMatrix4ToBEMatrix4(unityProjMatrix);
        
        if (showHeavyTrackingDebug)
        {
            be_NSDbg(@"Tracking Result is %@", mat2String(unityMatrix));
            be_NSDbg(@"Tracking pos is %f %f %f", unityMatrix.m[12], unityMatrix.m[13], unityMatrix.m[14]);
            be_NSDbg(@"Tracking scale is %f %f %f", trackerUpdateInterop.scale.x, trackerUpdateInterop.scale.y, trackerUpdateInterop.scale.z);
        }

        // Update the controller transform.
        [BEController sharedController].cameraTransform = predictedColorCameraPose;

        BOOL useVR = YES;

        // Enable Camera Texture on OpenGL or Metal APIs
        BOOL useCameraTexture = UnitySelectedRenderingAPI() == apiOpenGLES2
            || UnitySelectedRenderingAPI() == apiMetal;

        [self updateVR:useVR andCamera:useCameraTexture];

    } else {
        if (showHeavyTrackingDebug)
        {
            be_NSDbg(@"Bad Tracking, NaN result");
        }
    }

    [self updateTracking];
    
    if (trackerBridgeEngineEventCallback)
    {
        trackerBridgeEngineEventCallback(trackerUpdateInterop);
    }
}

#pragma mark - BEMixedRealityModeDelegate

- (void)mixedRealitySetUpSceneKitWorlds:(BEMappedAreaStatus)mappedAreaStatus
{
}

- (void)mixedRealityMarkupDidChange:(NSString*)markupChangedName
{
}

- (void)mixedRealityMarkupEditingEnded
{
}

- (void)mixedRealityUpdateAtTime:(NSTimeInterval)time
{
}

- (void)mixedRealitySensorsStatusChanged:(BESensorsStatus)sensorsStatus
{
}

- (void)mixedRealityDidLoadScene:(BEMappedAreaStatus)mappedAreaStatus
{
    NSLog(@"Bridge Engine is ready to begin");
    [self.delegate beUnityReady];
}


#pragma mark - Screen Recording
// Here we use a 3-finger tap to start/end a ReplayKit screen recording.
- (void)handleScreenRecordTap:(UITapGestureRecognizer *)sender
{
    RPScreenRecorder *screenRecorder = [RPScreenRecorder sharedRecorder];
    if( screenRecorder.isRecording == NO ) {
        [self startScreenRecorder];
    } else {
        [self stopScreenRecorder];
    }
}

- (void) startScreenRecorder {
    RPScreenRecorder *screenRecorder = [RPScreenRecorder sharedRecorder];
    if( screenRecorder.isRecording == NO && screenRecorder.isAvailable)
    {
        NSLog(@"Starting Screen Recording");
        [screenRecorder startRecordingWithHandler:^(NSError * _Nullable error) {
            if( error != nil ) {
                NSLog(@"Error Starting Screen Recording = %@", error);
            }
        }];
    }
}

- (void) stopScreenRecorder {
    RPScreenRecorder *screenRecorder = [RPScreenRecorder sharedRecorder];
    if( screenRecorder.isRecording == NO ) {
        return;
    }
    
    NSLog(@"Ending Screen Recording");
    [screenRecorder stopRecordingWithHandler:^(RPPreviewViewController * _Nullable previewViewController, NSError * _Nullable error) {
        if(error != nil)
        {
            NSLog(@"Error Ending Screen Recording = %@", error);
        }
        else
        {
            previewViewController.previewControllerDelegate = self;
            previewViewController.popoverPresentationController.sourceView = _unityView;
            [_unityVC presentViewController:previewViewController animated:YES completion:nil];
        }
    }];
}

- (void)previewControllerDidFinish:(RPPreviewViewController *)previewController
{
    // When the user is finished with the ReplayKit preview controller, we dismiss it.
    [previewController dismissViewControllerAnimated:YES completion:nil];
}

@end

#pragma mark - Interop Functions
extern "C"
{
    /// Check if BE is running in stereo mode 
    bool be_isStereoMode() {
        return [BEAppSettings booleanValueFromAppSetting:SETTING_STEREO defaultValueIfSettingIsNotInBundle:YES];
    }
    
    void be_hideOverlay() {
        [currentEngine.delegate beUnityOverlayFinished];
    }

    // get a pointer to a delegate in Mono from Unity to send tracker updates
    void be_registerTrackerEventCallback(void (*cb)(BETrackerUpdateInterop))
    {
        trackerBridgeEngineEventCallback = cb;
    }

    // Trigger loading all the meshes from the scan.
    void be_loadMeshes( BEMeshEventCallback meshCallback ) {
        NSLog(@"Loading Meshes");
        BEMesh *sceneMesh = currentEngine.coarseMesh;
        
        int meshCount = [sceneMesh numberOfMeshes];
        for (int meshIndex = 0; meshIndex < meshCount; ++meshIndex)
        {
            int verticesCount = [sceneMesh numberOfMeshVertices:meshIndex];
            int indicesCount = 3 * [sceneMesh numberOfMeshFaces:meshIndex];
            
            GLKVector3 *positions = [sceneMesh meshVertices:meshIndex];
            
            GLKVector3 *normals = NULL;
            GLKVector3 *colors = NULL;
            GLKVector2 *uvs = NULL;
            
            if ([sceneMesh hasPerVertexNormals])
            {
                normals = [sceneMesh meshPerVertexNormals:meshIndex];
            }
            
            if ([sceneMesh hasPerVertexColors])
            {
                colors = [sceneMesh meshPerVertexColors:meshIndex];
            }
            
            if ([sceneMesh hasPerVertexUVTextureCoords])
            {
                uvs = [sceneMesh meshPerVertexUVTextureCoords:meshIndex];
            }
            
            uint16_t *indices = [sceneMesh meshFaces:meshIndex];
            
            // right - handed coordinate system (BE) to left - handed (Unity)
            {
                BE_SCOPE_PROFILER (_, BE2Unity::scannedMeshConvertionOfCoordSystem, 60);
                
                GLKVector3 *ip = positions;
                for (int i = 0; i < verticesCount; ++i, ++ip)           // change sign for Y for vectors
                {
                    ip->v[1] = -ip->v[1];
                }
                
                GLKVector3 *inp = normals;
                for (int i = 0; i < verticesCount; ++i, ++inp)          // change sign for Y for vectors
                {
                    inp->v[1] = -inp->v[1];
                }
                
                uint16_t *ii = indices;
                for (int i = 0; i < indicesCount / 3; ++i, ii += 3)    // swap culling (clockwise/counterclockwise)
                {
                    uint16_t *second = (ii + 1);
                    uint16_t *third  = (ii + 2);
                    
                    std::swap(*second, *third);
                }
            }
            
            meshCallback(meshIndex,
                       meshCount,
                       verticesCount,
                       reinterpret_cast<intptr_t>(positions),
                       reinterpret_cast<intptr_t>(normals),
                       reinterpret_cast<intptr_t>(colors),
                       reinterpret_cast<intptr_t>(uvs),
                       indicesCount,
                       reinterpret_cast<intptr_t>(indices));
        }
    }
}
