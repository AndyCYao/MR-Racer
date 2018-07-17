//
//  BridgeEngineOverlayView.m
//  Unity-iPhone
//
//  Created by Andrew Zimmer on 10/3/17.
//

#import "BridgeEngineOverlayView.h"

#import <BridgeEngine/BEAppSettings.h>
#import "BridgeEngineAppController.h"

//------------------------------------------------------------------
bool isStereo() {
    return [BEAppSettings booleanValueFromAppSetting:SETTING_STEREO defaultValueIfSettingIsNotInBundle:YES];
}
//------------------------------------------------------------------

@interface BridgeEngineOverlayView()
@property (nonatomic, strong) UIWindow *overlayWindow;
@end

@implementation BridgeEngineOverlayView

+ (instancetype)overlayView {
    BridgeEngineOverlayView *overlayView = [[BridgeEngineOverlayView alloc] initWithFrame:[UIScreen mainScreen].bounds];
    
    NSString *loadingImageName = isStereo() ? @"StartingUnity-stereo.png" : @"StartingUnity-mono.png";
    UIImage *loadingImage = [UIImage imageNamed:loadingImageName];
    UIImageView *imgView = [[UIImageView alloc] initWithImage:loadingImage];
    imgView.frame = [UIScreen mainScreen].bounds;
    imgView.contentMode = UIViewContentModeScaleAspectFit;
    imgView.backgroundColor = [UIColor clearColor];
    [overlayView addSubview:imgView];
    
    overlayView.backgroundColor = [UIColor blackColor];
    overlayView.alpha = 0;
    overlayView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;

    return overlayView;
}

- (void)showWithCompleted:(void (^)(BOOL finished))completedBlock {
    self.overlayWindow = [[UIWindow alloc] initWithFrame:[UIScreen mainScreen].bounds];
    
    // Cover the Unity bootstrapping somewhere between normal and the alert window level.
    self.overlayWindow.windowLevel = (UIWindowLevelNormal + UIWindowLevelAlert) * 0.5;
    self.overlayWindow.rootViewController = [[UIViewController alloc] init];
    [self.overlayWindow.rootViewController.view addSubview:self];
    self.overlayWindow.rootViewController.view.backgroundColor = [UIColor blackColor];
    self.overlayWindow.alpha = 0;
    self.overlayWindow.hidden = NO;
    
    [UIView animateWithDuration:0.3 animations:^{
        self.alpha = 1;
        self.overlayWindow.alpha = 1;
    } completion:completedBlock];
}

- (void)hide {
    [UIView animateWithDuration:0.3 animations:^{
        self.alpha = 0;
        self.overlayWindow.alpha = 0;
    } completion:^(BOOL finished) {
        [self removeFromSuperview];
        self.overlayWindow.hidden = YES;
        self.overlayWindow = nil;
    }];
}

@end
