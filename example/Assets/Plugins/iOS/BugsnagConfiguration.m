//
//  BugsnagConfiguration.m
//
//  Created by Conrad Irwin on 2014-10-01.
//
//  Copyright (c) 2014 Bugsnag, Inc. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall remain in place
// in this source code.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

#import "KSCrashReportWriter.h"

#import "BugsnagBreadcrumb.h"
#import "BugsnagConfiguration.h"
#import "BugsnagMetaData.h"

@interface BugsnagConfiguration ()
@property(nonatomic, readwrite, strong) NSMutableArray *beforeNotifyHooks;
@end

@implementation BugsnagConfiguration

- (id)init {
  if (self = [super init]) {
    _metaData = [[BugsnagMetaData alloc] init];
    _config = [[BugsnagMetaData alloc] init];
    _apiKey = @"";
    _autoNotify = true;
    _notifyURL = [NSURL URLWithString:@"https://notify.bugsnag.com/"];
    _beforeNotifyHooks = [NSMutableArray new];
    _notifyReleaseStages = nil;
    _breadcrumbs = [BugsnagBreadcrumbs new];
#if DEBUG
    _releaseStage = @"development";
#else
    _releaseStage = @"production";
#endif
  }
  return self;
}
- (void)setUser:(NSString *)userId
       withName:(NSString *)userName
       andEmail:(NSString *)userEmail {
  [self.metaData addAttribute:@"id" withValue:userId toTabWithName:@"user"];
  [self.metaData addAttribute:@"name" withValue:userName toTabWithName:@"user"];
  [self.metaData addAttribute:@"email"
                    withValue:userEmail
                toTabWithName:@"user"];
}

- (void)addBeforeNotifyHook:(BugsnagBeforeNotifyHook)hook {
  [(NSMutableArray *)self.beforeNotifyHooks addObject:[hook copy]];
}

- (void)setReleaseStage:(NSString *)newReleaseStage {
  _releaseStage = newReleaseStage;
  [self.config addAttribute:@"releaseStage"
                  withValue:newReleaseStage
              toTabWithName:@"config"];
}

- (void)setNotifyReleaseStages:(NSArray *)newNotifyReleaseStages;
{
  NSArray *notifyReleaseStagesCopy = [newNotifyReleaseStages copy];
  _notifyReleaseStages = notifyReleaseStagesCopy;
  [self.config addAttribute:@"notifyReleaseStages"
                  withValue:notifyReleaseStagesCopy
              toTabWithName:@"config"];
}

- (void)setContext:(NSString *)newContext {
  _context = newContext;
  [self.config addAttribute:@"context"
                  withValue:newContext
              toTabWithName:@"config"];
}

- (void)setAppVersion:(NSString *)newVersion {
  _appVersion = newVersion;
  [self.config addAttribute:@"appVersion"
                  withValue:newVersion
              toTabWithName:@"config"];
}
@end
