//
//  native_utils.m
//  NativeCrashy
//
//  Created by Steve Kirkland-Walton on 20/06/2022.
//  Copyright © 2022 Bugsnag, Inc. All rights reserved.
//

#import <Foundation/Foundation.h>

void PreventCrashPopups() {
    [[NSUserDefaults standardUserDefaults] registerDefaults:@{
        // Disable state restoration to prevent the following dialog being shown after crashes
        // "The last time you opened macOSTestApp, it unexpectedly quit while reopening windows.
        //  Do you want to try to reopen its windows again?"
        // https://developer.apple.com/library/archive/releasenotes/AppKit/RN-AppKitOlderNotes/index.html#10_7StateRestoration
        @"ApplePersistenceIgnoreState": @YES,
        // Stop NSApplication swallowing NSExceptions thrown on the main thread.
        @"NSApplicationCrashOnExceptions": @YES,
    }];
}
