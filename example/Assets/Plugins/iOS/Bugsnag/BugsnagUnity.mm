#import "Bugsnag.h"
#import "BugsnagCrashReport.h"

@interface Bugsnag ()
+ (id)notifier;
@end

extern "C" {
    void BSGSetContext(char *context);
    void BSGSetReleaseStage(char *releaseStage);
    void BSGSetAutoNotify(int autoNotify);
    void BSGNotify(char *errorClass, char *errorMessage, char *severity, char *context, char *stackTrace, char *logType, char *severityReason);
    void BSGRegister(char *apiKey, bool trackSessions);
    void BSGAddToTab(char *tabName, char *attributeName, char *attributeValue);
    void BSGClearTab(char *tabName);
    void BSGLeaveBreadcrumb(char *breadcrumb);
    void BSGSetBreadcrumbCapacity(int capacity);
    void BSGSetAppVersion(char *version);
    void BSGSetUser(char *userId, char *userName, char *userEmail);
    void BSGExampleNativeCrash();
    void BSGExampleCrashInBackground();

    NSMutableArray *BSGparseStackTrace(NSString *stackTrace, NSRegularExpression *stacktraceRegex);

    BSGSeverity BSGParseBugsnagSeverity(NSString *severity) {
        if ([severity isEqualToString:@"info"])
            return BSGSeverityInfo;
        else if ([severity isEqualToString:@"warning"])
            return BSGSeverityWarning;
        return BSGSeverityError;
    }

    void BSGSetContext(char *context) {
        NSString *ns_context = [NSString stringWithUTF8String: context];
        [Bugsnag configuration].context = ns_context;
    }

    void BSGSetReleaseStage(char *releaseStage) {
        NSString *ns_releaseStage = [NSString stringWithUTF8String: releaseStage];
        [Bugsnag configuration].releaseStage = ns_releaseStage;
    }

    void BSGSetNotifyReleaseStages(char *notifyReleaseStages) {
        NSString *ns_notifyReleaseStages = [NSString stringWithUTF8String: notifyReleaseStages];
        if ([ns_notifyReleaseStages isEqualToString: @""]) {
            [Bugsnag configuration].notifyReleaseStages = @[];
        } else {
            [Bugsnag configuration].notifyReleaseStages = [ns_notifyReleaseStages componentsSeparatedByString: @","];
        }
    }

    void BSGSetSessionUrl(char *url) {
        NSString *ns_url = [NSString stringWithUTF8String:url];
        [Bugsnag configuration].sessionURL = [NSURL URLWithString: ns_url];
    }

    void BSGSetNotifyUrl(char *notifyUrl) {
        NSString *ns_notifyUrl = [NSString stringWithUTF8String:notifyUrl];
        [Bugsnag configuration].notifyURL = [NSURL URLWithString: ns_notifyUrl];
    }

    void BSGSetAutoNotify(int autoNotify) {
        [Bugsnag configuration].autoNotify = autoNotify;
    }

    void BSGAddToTab(char *tabName, char *attributeName, char *attributeValue) {
        NSString *ns_tabName = [NSString stringWithUTF8String:tabName];
        NSString *ns_attributeName = [NSString stringWithUTF8String:attributeName];
        NSString *ns_attributeValue = [NSString stringWithUTF8String:attributeValue];
        [Bugsnag addAttribute:ns_attributeName withValue:ns_attributeValue toTabWithName:ns_tabName];
    }

    void BSGClearTab(char *tabName) {
        NSString *ns_tabName = [NSString stringWithUTF8String:tabName];
        [Bugsnag clearTabWithName:ns_tabName];
    }

    void BSGNotify(char *errorClass, char *errorMessage, char *severity, char *context, char *stackTrace, char *logType, char *severityReason) {
        NSString *ns_errorClass = [NSString stringWithUTF8String:errorClass];
        NSString *ns_errorMessage = [NSString stringWithUTF8String:errorMessage];
        NSString *ns_severity = [NSString stringWithUTF8String:severity];
        NSString *ns_context = [NSString stringWithUTF8String:context];
        NSString *ns_stackTrace = [NSString stringWithUTF8String:stackTrace];
        NSString *ns_logType = [NSString stringWithUTF8String:logType];
        NSString *ns_severityReason = [NSString stringWithUTF8String:severityReason];

        NSRegularExpression *unityExpression = [NSRegularExpression regularExpressionWithPattern:@"(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?"
                                                                                         options:NSRegularExpressionCaseInsensitive
                                                                                           error:nil];

        NSMutableArray *stacktrace = BSGparseStackTrace(ns_stackTrace, unityExpression);
        NSException * exception = [NSException exceptionWithName:ns_errorClass
                                                          reason:ns_errorMessage
                                                        userInfo:NULL];

        // Indicate thats its a unity exception (with the received log level)
        NSDictionary *unityData = nil;
        if ([ns_logType isEqualToString: @""]) {
            unityData = @{@"unityException": @true};
        } else {
            unityData = @{@"unityException": @true, @"unityLogLevel": ns_logType};
        }

        dispatch_async(dispatch_get_global_queue(0, 0), ^{
            id notifier = [Bugsnag notifier];

            NSMutableDictionary *dict = [NSMutableDictionary new];
            dict[@"severity"] = ns_severity;
            dict[@"severityReason"] = ns_severityReason;
            if (ns_logType) {
                dict[@"logLevel"] = ns_logType;
            }

            [notifier internalClientNotify:exception
                                  withData:dict
                                     block:^(BugsnagCrashReport *report) {
                 if (ns_context.length > 0) {
                     report.context = ns_context;
                 }
                 [report attachCustomStacktrace:stacktrace withType:@"unity"];
                 NSMutableDictionary *metadata = [report.metaData mutableCopy];
                 metadata[@"Unity"] = unityData;
                 report.metaData = metadata;
            }];
        });
    }

    void BSGRegister(char *apiKey, bool trackSessions) {
        NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];

        // Disable thread suspension so there is no noticable lag in sending Bugsnags
        [Bugsnag setSuspendThreadsForUserReported:false];

        // Set reporting of Bugsnags when debugger is attached
        [Bugsnag setReportWhenDebuggerIsAttached:true];

        // Disable thread tracing on non-fatal exceptions
        [Bugsnag setThreadTracingEnabled:false];

        // Disable writing binary images
        [Bugsnag setWriteBinaryImagesForUserReported:false];

        BugsnagConfiguration *config = [BugsnagConfiguration new];
        config.apiKey = ns_apiKey;
        config.shouldAutoCaptureSessions = trackSessions;
        [Bugsnag startBugsnagWithConfiguration:config];

        id notifier = [Bugsnag notifier];
        [notifier setValue:@{
            @"version": @"3.6.5",
            @"name": @"Bugsnag Unity (Cocoa)",
            @"url": @"https://github.com/bugsnag/bugsnag-unity"
        } forKey:@"details"];
    }

    void BSGStartSession() {
        [Bugsnag startSession];
    }

    void BSGLeaveBreadcrumb(char *breadcrumb) {
        [Bugsnag leaveBreadcrumbWithMessage: [NSString stringWithUTF8String:breadcrumb]];
    }

    void BSGSetBreadcrumbCapacity(int capacity) {
        [Bugsnag setBreadcrumbCapacity: (NSUInteger)capacity];
    }

    void BSGSetAppVersion(char *version) {
        [Bugsnag configuration].appVersion = [NSString stringWithUTF8String:version];
    }

    void BSGSetUser(char *userId, char *userName, char *userEmail) {
        NSString *ns_userId = [NSString stringWithUTF8String: userId];
        NSString *ns_userName = [NSString stringWithUTF8String: userName];
        NSString *ns_userEmail = [NSString stringWithUTF8String: userEmail];
        [[Bugsnag configuration] setUser:ns_userId withName:ns_userName andEmail:ns_userEmail];
    }

    NSMutableArray *BSGparseStackTrace(NSString *stackTrace, NSRegularExpression *stacktraceRegex) {
        NSMutableArray *returnArray = [[NSMutableArray alloc] init];

        [stacktraceRegex enumerateMatchesInString:stackTrace options:0 range:NSMakeRange(0, [stackTrace length]) usingBlock:^(NSTextCheckingResult *result, NSMatchingFlags flags, BOOL *stop) {
            NSMutableDictionary *lineDetails = [[NSMutableDictionary alloc] initWithCapacity:3];
            if(result) {
                if(result.numberOfRanges >= 1 && [result rangeAtIndex:1].location != NSNotFound) {
                    [lineDetails setObject:[stackTrace substringWithRange:[result rangeAtIndex:1]] forKey:@"method"];
                } else {
                    [lineDetails setObject:@"unknown method" forKey:@"method"];
                }

                if(result.numberOfRanges >= 2 && [result rangeAtIndex:2].location != NSNotFound) {
                    NSString *fileName = [stackTrace substringWithRange:[result rangeAtIndex:2]];
                    if(![fileName isEqualToString:@"<filename unknown>"]) {
                        [lineDetails setObject:fileName forKey:@"file"];
                    } else {
                        [lineDetails setObject:@"unknown file" forKey:@"file"];
                    }
                } else {
                    [lineDetails setObject:@"unknown file" forKey:@"file"];
                }

                if(result.numberOfRanges >= 3 && [result rangeAtIndex:3].location != NSNotFound) {
                    int lineNumber = (int)[[stackTrace substringWithRange:[result rangeAtIndex:3]] integerValue];
                    [lineDetails setObject:[NSNumber numberWithInt:lineNumber] forKey:@"lineNumber"];
                } else {
                    [lineDetails setObject:[NSNumber numberWithInt:0] forKey:@"lineNumber"];
                }
            } else {
                [lineDetails setObject:@"unknown method" forKey:@"method"];
                [lineDetails setObject:[NSNumber numberWithInt:0] forKey:@"lineNumber"];
                [lineDetails setObject:@"unknown file" forKey:@"file"];
            }
            [returnArray addObject:lineDetails];
        }];
        return returnArray;
    }


    void ExampleNativeCrash() {
        id obj = [NSArray new][1];
        NSLog(@"Should never happen");
    }

    void ExampleCrashInBackground() {
        dispatch_async(dispatch_get_global_queue(0, 0), ^{
            ExampleNativeCrash();
        });
    }
}
