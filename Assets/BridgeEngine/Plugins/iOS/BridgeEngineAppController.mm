/*
 This file is part of the Structure SDK.
 Copyright © 2018 Occipital, Inc. All rights reserved.
 http://structure.io
 */

#import "BridgeEngineUnity.h"
#import "BridgeEngineAppController.h"
#import "BEScanViewController.h"

#import <BridgeEngine/BridgeEngine.h>
#import <BridgeEngine/BEProfiling.h>
#import <BridgeEngine/BEDebugging.h>
#import <BridgeEngine/BEAppSettings.h>
#import <BridgeEngine/BEDebugSettingsViewController.h>

#import "BridgeEngineOverlayView.h"

#if BE_PROFILING
namespace BEMonitor
{
    BE::PerformanceMonitor displayLink {"DisplayLink"};
}
#endif

// See the NSLog messages wirelessly.
//
// Run netcat on the terminal:
//  nc -lk 4999
//
// And uncomment the following line:
// #define REMOTE_LOG_HOST "172.16.10.106"

@interface BridgeEngineAppController ()
<
    BEDebugSettingsDelegate,
    BEScanViewControllerDelegate,
    BridgeEngineUnityDelegate
>

@property(nonatomic) BOOL scanLoaded;
@property(nonatomic, strong) BridgeEngineUnity *bridgeEngineUnity;
@property(nonatomic, strong) BEScanViewController *scanVC;
@property BridgeEngineOverlayView *overlay;

#ifdef DEBUG
@property (nonatomic, strong) UINavigationController *navController;
#endif // DEBUG

@end

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

@implementation BridgeEngineAppController

//------------------------------------------------------------------------------
#pragma mark - Unity Runtime

+ (void)initialize
{
#ifdef REMOTE_LOG_HOST
    NSError* error = nil;
    BEStartLoggingToWirelessConsole(@REMOTE_LOG_HOST, 4999, &error);
    if (error)
        NSLog(@"Oh no! Can't start wireless log: %@", [error localizedDescription]);
#endif
}

/**
 * Intercept Unity's createUI flow, override the _window and make it or own for scanning.
 *  Once completed, we can switch to proper Unity runtime createUI.
 */ 
- (void) createUI {
    self.scanLoaded = NO;

    BOOL hasAppliedSettingsOnFirstRun = [BEAppSettings booleanValueFromAppSetting:SETTING_FIRST_RUN defaultValueIfSettingIsNotInBundle:NO];
    if(hasAppliedSettingsOnFirstRun == NO) {
        [BEAppSettings setBooleanValue:YES forAppSetting:SETTING_FIRST_RUN];
        [self resetSettingsToDefaults];
    }

#ifdef DEBUG
    // Show the settings UI, with a prepared set of debug settings.
    NSBundle *beBundle = [NSBundle bundleForClass:BEDebugSettingsViewController.class];
    UIStoryboard *beDebugSettingsStoryboard = [UIStoryboard storyboardWithName:@"BEDebugSettings" bundle:beBundle];
    self.navController = [beDebugSettingsStoryboard instantiateInitialViewController];
    BEDebugSettingsViewController *debugSettingsVC = (BEDebugSettingsViewController *)_navController.viewControllers.firstObject;
    [self prepareDebugSettingsVC:debugSettingsVC];
    [_window setRootViewController:_navController];
#else // DEBUG
    // Release mode builds always goes to BE scanning phase
    self.scanVC = [[BEScanViewController alloc] init];
    _scanVC.delegate = self;
    [_window setRootViewController:_scanVC];
#endif // DEBUG  
    [_window makeKeyAndVisible];
}

/**
 * Intercept Unity runtime, so we launch our BEScanViewController and start scanning.
 */
- (void)startUnity:(UIApplication*)application
{
    if( _scanLoaded ) {
        // Aggressively try to disconnect from BridgeEngine.
        [_scanVC disconnectFromBE];
        [_scanVC removeFromParentViewController];
        self.scanVC = nil;

        // Fire-up Bridge Engine in headless mode.
        // Make sure to create this _after_ the scanVC is dead.
        self.bridgeEngineUnity = [[BridgeEngineUnity alloc] initWithUnityView:self.unityView unityVC:self.rootViewController];
        self.bridgeEngineUnity.delegate = self;

        // Kickoff BE's initialization
        [self.bridgeEngineUnity onDisplayLink];

        // Wait until BE is ready before officially starting Unity.
    }
}

/**
 * Called After [super startUnity]
 * Override the UnityAppController+Rendering handler for DisplayLink callbacks
 * This is our render thread.
 */
- (void)repaintDisplayLink
{
    BE_SCOPE_PROFILER(_, BEMonitor::displayLink, 60);
    BE_KDEBUG_SCOPED_SIGN(kd, BE::KDebugCode::DisplayLink, 0, 1);

    [self.bridgeEngineUnity onDisplayLink];
    [super repaintDisplayLink];
}

/// Convencience for computing the replayMode setting, across scanning and unity.
+ (BECaptureReplayMode) replayMode {
    BECaptureReplayMode replayMode = BECaptureReplayModeDisabled;
    if ([BEAppSettings booleanValueFromAppSetting:SETTING_OCC_REPLAY defaultValueIfSettingIsNotInBundle:NO])
    {
        if ([BEAppSettings booleanValueFromAppSetting:SETTING_OCC_REPLAY_REALTIME defaultValueIfSettingIsNotInBundle:NO]) {
            // Enables realtime replay, including frame dropping, etc. Use this when doing performance experiments.
            replayMode = BECaptureReplayModeRealTime;
        } else {
            // Enables playback of every single recorded frame, Use this when doing breakpoint debugging.
            replayMode = BECaptureReplayModeDeterministic;
        }
    } else {
        replayMode = BECaptureReplayModeDisabled;
    }
    return replayMode;
}

//------------------------------------------------------------------------------

#pragma mark - BEDebugSettingsVC Delegate Handler

/**
 * Add all the debug setting options to the VC.
 */
- (void) prepareDebugSettingsVC:(BEDebugSettingsViewController*)vc {
    BOOL isiPhone = (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone);
    
    [vc addKey:SETTING_STEREO label:@"Stereo VR Mode" defaultBool:isiPhone];
    [vc addKey:SETTING_USE_WVL label:@"Use Wide Vision Lens" defaultBool:isiPhone];
    [vc addKey:SETTING_COLOR_CAMERA_ONLY label:@"Color Camera Only" defaultBool:NO];
    [vc addKey:SETTING_AUTO_EXPOSE_DURING_RELOC label:@"Auto Expose Color While Relocalizing" defaultBool:NO];
    [vc addKey:SETTING_OCC_REPLAY label:@"Replay Last Recorded OCC" defaultBool:NO];
    [vc addKey:SETTING_OCC_REPLAY_REALTIME label:@"Replay in Realtime Mode" defaultBool:NO];
    
    vc.delegate = self;
}

/**
 * Reset all the settings for release.
 */
- (void) resetSettingsToDefaults {
    BOOL isiPhone = (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone);
    [BEAppSettings setBooleanValue:NO forAppSetting:SETTING_OCC_REPLAY];
    [BEAppSettings setBooleanValue:NO forAppSetting:SETTING_OCC_REPLAY_REALTIME];
    [BEAppSettings setBooleanValue:isiPhone forAppSetting:SETTING_USE_WVL];
    [BEAppSettings setBooleanValue:isiPhone forAppSetting:SETTING_STEREO];
    [BEAppSettings setBooleanValue:NO forAppSetting:SETTING_COLOR_CAMERA_ONLY];
    [BEAppSettings setBooleanValue:NO forAppSetting:SETTING_AUTO_EXPOSE_DURING_RELOC];
}

/**
 * User tapped on "Begin" to start the BE experience.
 */
- (void) debugSettingsBegin {
#ifdef DEBUG
    BOOL useColorOnlyMode = [BEAppSettings booleanValueFromAppSetting:SETTING_COLOR_CAMERA_ONLY defaultValueIfSettingIsNotInBundle:NO];
    BOOL useReplayOCC = [BEAppSettings booleanValueFromAppSetting:SETTING_OCC_REPLAY defaultValueIfSettingIsNotInBundle:NO];

    _navController.navigationBarHidden = YES;

    if( useReplayOCC || useColorOnlyMode ) {
        [self scanViewDidFinish];
    } else {
        self.scanVC = [[BEScanViewController alloc] init];
        _scanVC.delegate = self;
        [_navController pushViewController:_scanVC animated:YES];
    }
#endif // DEBUG
}

#pragma mark - BridgeEngineUnity Delegate
- (void) beUnityReady {
    [super startUnity:UIApplication.sharedApplication];
}

- (void)beUnityOverlayFinished {
    [self.overlay hide];
    self.overlay = nil;
}

#pragma mark - Finish Scanning and Run Unity

- (void) scanViewDidFinish {
    self.scanLoaded = YES;
    self.overlay = [BridgeEngineOverlayView overlayView];
    [self.overlay showWithCompleted:^(BOOL finished) {
        // Blank out the current scanner _window rootController
        [_window setRootViewController:[[UIViewController alloc] init]];
        
        // Disable the core BridgeEngine messages.  It can get noisy.
        BESetVerbosityLevel(1); // kept it to 1 for debugging.
        
        // Continue creating the UI, and restart setting it as root.
        [super createUI];
        [_window setRootViewController:_rootController];

        if (@available(iOS 11.0, *)) {
            // Constantly re-try hiding the home indicator every 2sec.
            [NSTimer scheduledTimerWithTimeInterval:2 repeats:YES block:^(NSTimer * _Nonnull timer) {
                [_rootController setNeedsUpdateOfHomeIndicatorAutoHidden];
            }];
        }
        
        UIApplication *app = [UIApplication sharedApplication];
        [self performSelector:@selector(startUnity:) withObject:app afterDelay:0];
    }];
}


- (void)finishActivityAndReturn:(BOOL)backTo2D {
    
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(BridgeEngineAppController)
