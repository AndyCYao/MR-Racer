//
//  BridgeEngineOverlayView.h
//  Unity-iPhone
//
//  Created by Andrew Zimmer on 10/3/17.
//

#import <UIKit/UIKit.h>

@interface BridgeEngineOverlayView : UIView

+ (instancetype)overlayView;
- (void)showWithCompleted:(void (^)(BOOL finished))completedBlock;
- (void)hide;

@end
