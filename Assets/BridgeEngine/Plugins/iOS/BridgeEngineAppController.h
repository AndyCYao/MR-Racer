/*
 This file is part of the Structure SDK.
 Copyright © 2018 Occipital, Inc. All rights reserved.
 http://structure.io
 */

#pragma once

#import <BridgeEngine/BEEngine.h>

#import "UnityAppController.h"
#import "UnityAppController+Rendering.h"
#import "UnityAppController+ViewHandling.h"
#define SETTING_FIRST_RUN                       @"HasAppliedSettingOnFirstRun"

#define SETTING_OCC_REPLAY                      @"occReplay"
#define SETTING_OCC_REPLAY_REALTIME             @"occReplayRealtime"
#define SETTING_USE_WVL                         @"useWVL"
#define SETTING_STEREO                          @"stereo"
#define SETTING_COLOR_CAMERA_ONLY               @"colorCameraOnly"
#define SETTING_AUTO_EXPOSE_DURING_RELOC        @"autoExposeWhileRelocalizing"

@interface BridgeEngineAppController : UnityAppController

+ (BECaptureReplayMode) replayMode;

/// Check to make sure we are executing on device
#if TARGET_IPHONE_SIMULATOR
#error Bridge Engine Framework requires an iOS device to build. It cannot be run on the simulator.
#endif


@end
