#import "Bugsnag.h"
#import "KSCrash.h"
#import "NSDictionary+Merge.h"

extern "C" {
    void SetContext(char *context);
    void SetReleaseStage(char *releaseStage);
    void SetAutoNotify(int autoNotify);
    void Notify(char *errorClass, char *errorMessage, char *severity, char *context, char *stackTrace);
    void Register(char *apiKey);
    void AddToTab(char *tabName, char *attributeName, char *attributeValue);
    void ClearTab(char *tabName);
    NSMutableArray *parseStackTrace(NSString *stackTrace, NSRegularExpression *stacktraceRegex);

    void SetContext(char *context) {
        NSString *ns_context = [NSString stringWithUTF8String: context];
        [Bugsnag configuration].context = ns_context;
    }

    void SetReleaseStage(char *releaseStage) {
        NSString *ns_releaseStage = [NSString stringWithUTF8String: releaseStage];
        [Bugsnag configuration].releaseStage = ns_releaseStage;
    }

    void SetNotifyReleaseStages(char *notifyReleaseStages) {
        NSString *ns_notifyReleaseStages = [NSString stringWithUTF8String: notifyReleaseStages];
        if ([ns_notifyReleaseStages isEqualToString: @""]) {
            [Bugsnag configuration].notifyReleaseStages = @[];
        } else {
            [Bugsnag configuration].notifyReleaseStages = [ns_notifyReleaseStages componentsSeparatedByString: @","];
        }
    }

    void SetNotifyUrl(char *notifyUrl) {
        NSString *ns_notifyUrl = [NSString stringWithUTF8String:notifyUrl];
        [Bugsnag configuration].notifyURL = [NSURL URLWithString: ns_notifyUrl];
    }

    void SetAutoNotify(int autoNotify) {
        [Bugsnag configuration].autoNotify = autoNotify;
    }

    void AddToTab(char *tabName, char *attributeName, char *attributeValue) {
        NSString *ns_tabName = [NSString stringWithUTF8String:tabName];
        NSString *ns_attributeName = [NSString stringWithUTF8String:attributeName];
        NSString *ns_attributeValue = [NSString stringWithUTF8String:attributeValue];
        [Bugsnag addAttribute:ns_attributeName withValue:ns_attributeValue toTabWithName:ns_tabName];
    }

    void ClearTab(char *tabName) {
        NSString *ns_tabName = [NSString stringWithUTF8String:tabName];
        [Bugsnag clearTabWithName:ns_tabName];
    }

    void Notify(char *errorClass, char *errorMessage, char *severity, char *context, char *stackTrace) {
        NSString *ns_errorClass = [NSString stringWithUTF8String:errorClass];
        NSString *ns_errorMessage = [NSString stringWithUTF8String:errorMessage];
        NSString *ns_severity = [NSString stringWithUTF8String:severity];
        NSString *ns_context = [NSString stringWithUTF8String:context];

        NSString *ns_stackTrace = [NSString stringWithUTF8String:stackTrace];

        NSRegularExpression *unityExpression = [NSRegularExpression regularExpressionWithPattern:@"(\\S+)\\s*\\(.*?\\)\\s*(?:(?:\\[.*\\]\\s*in\\s|\\(at\\s*\\s*)(.*):(\\d+))?"
                                                                                         options:NSRegularExpressionCaseInsensitive
                                                                                           error:nil];

        NSMutableArray *stacktrace = parseStackTrace(ns_stackTrace, unityExpression);

        NSDictionary *notifier = @{
                                   @"name": @"Bugsnag Unity (Cocoa)",
                                   @"version": @"2.0.0",
                                   @"url":@"https://bugsnag.com/"
                                   };

        if ([ns_context isEqualToString: @""]) {
            ns_context = [Bugsnag configuration].context;
        }

        NSDictionary *metaData = @{@"_bugsnag_unity_exception":@{@"stacktrace": stacktrace,
                                                                 @"notifier": notifier},
                                    @"context":ns_context};

        metaData = [metaData mergedInto: [[Bugsnag configuration].metaData toDictionary]];

        dispatch_async(dispatch_get_global_queue(0, 0), ^ {
            [Bugsnag notify:[NSException exceptionWithName:ns_errorClass reason: ns_errorMessage userInfo: NULL] withData: metaData atSeverity: ns_severity];
        });
    }

    void Register(char *apiKey) {
        NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];

        [KSCrash sharedInstance].suspendThreadsForUserReported = NO;

        [Bugsnag startBugsnagWithApiKey:ns_apiKey];
    }

    NSMutableArray *parseStackTrace(NSString *stackTrace, NSRegularExpression *stacktraceRegex) {
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
                    [lineDetails setObject:[NSNumber numberWithInt:[[stackTrace substringWithRange:[result rangeAtIndex:3]] integerValue]] forKey:@"lineNumber"];
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
}
