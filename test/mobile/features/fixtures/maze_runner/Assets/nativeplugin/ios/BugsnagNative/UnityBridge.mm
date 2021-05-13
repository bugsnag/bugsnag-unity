//
//  UnityBridge.m
//  BugsnagNative
//
//  Created by Alexander Moinet on 14/09/2018.
//  Copyright © 2018 Bugsnag. All rights reserved.
//

#import "UnityBridge.h"

void framework_crash_me() {
    [BugsnagNative crashMe];
}
