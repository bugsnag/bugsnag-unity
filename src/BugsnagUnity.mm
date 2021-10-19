#import "Bugsnag.h"
#import "BugsnagConfiguration+Private.h"
#import "BugsnagLogger.h"
#import "BugsnagUser.h"
#import "BugsnagNotifier.h"
#import "BugsnagSessionTracker.h"
#import "Bugsnag+Private.h"
#import "BugsnagClient+Private.h"
#import "BSG_KSSystemInfo.h"
#import "BSG_KSMach.h"
#import "BSG_KSCrash.h"
#import "BSG_RFC3339DateTool.h"
#import "BugsnagBreadcrumb+Private.h"
#import "BugsnagSession+Private.h"

extern "C" {
    
    struct bugsnag_user {
        const char *user_id;
    };

    void *bugsnag_createConfiguration(char *apiKey);

    void bugsnag_setReleaseStage(const void *configuration, char *releaseStage);

    void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount);

    void bugsnag_setAppVersion(const void *configuration, char *appVersion);

    void bugsnag_setPersistUser(const void *configuration, bool persistUser);

    void bugsnag_setSendLaunchCrashesSynchronously(const void *configuration, bool sendLaunchCrashesSynchronously);

    void bugsnag_setAutoNotifyConfig(const void *configuration, bool autoNotify);

    void bugsnag_setAutoNotify(bool autoNotify);

    void bugsnag_setBundleVersion(const void *configuration, char *bundleVersion);

    void bugsnag_setAppType(const void *configuration, char *appType);

    void bugsnag_setContext(const void *configuration, char *context);
      
    void bugsnag_setContextConfig(const void *configuration, char *context);

    void bugsnag_setMaxBreadcrumbs(const void *configuration, int maxBreadcrumbs);

    void bugsnag_setEnabledBreadcrumbTypes(const void *configuration, const char *types[], int count);

    void bugsnag_setMaxPersistedEvents(const void *configuration, int maxPersistedEvents);

    void bugsnag_setThreadSendPolicy(const void *configuration, char *threadSendPolicy);

    void bugsnag_setEndpoints(const void *configuration, char *notifyURL, char *sessionsURL);

    void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount);

    void bugsnag_removeMetadata(const void *configuration, const char *tab);

    void bugsnag_retrieveMetaData(const void *metadata, void (*callback)(const void *instance, const char *tab, const char *keys[], int keys_size, const char *values[], int values_size));
 
    void bugsnag_retrieveLastRunInfo(const void *lastRuninfo, void (*callback)(const void *instance, bool crashed, bool crashedDuringLaunch, int consecutiveLaunchCrashes));

    void bugsnag_startBugsnagWithConfiguration(const void *configuration, char *notifierVersion);

    void bugsnag_addBreadcrumb(char *name, char *type, char *metadata[], int metadataCount);

    void bugsnag_retrieveBreadcrumbs(const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], int keys_size, const char *values[], int values_size));

    void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value));

    void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value));

    void bugsnag_populateUser(bugsnag_user *user);

    void bugsnag_setUser(char *userId, char *userName, char *userEmail);

    void bugsnag_setEnabledErrorTypes(const void *configuration, const char *types[], int count);

    void bugsnag_setDiscardClasses(const void *configuration, const char *classNames[], int count);

    void bugsnag_setRedactedKeys(const void *configuration, const char *redactedKeys[], int count);

    void bugsnag_setAutoTrackSessions(const void *configuration, bool autoTrackSessions);

    void bugsnag_setAppHangThresholdMillis(const void *configuration, NSUInteger appHangThresholdMillis);

    void bugsnag_markLaunchCompleted();

    void bugsnag_setLaunchDurationMillis(const void *configuration, NSUInteger launchDurationMillis);

    void bugsnag_startSession();

    void bugsnag_pauseSession();

    bool bugsnag_resumeSession();

    void bugsnag_retrieveCurrentSession(const void *session, void (*callback)(const void *instance, const char *sessionId, const char *startedAt, int handled, int unhandled));

    void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount);

    void bugsnag_registerForOnSendCallbacks(const void *configuration, bool (*callback)(const char *test));

    void bugsnag_registerForSessionCallbacks(const void *configuration, bool (*callback)(void *session));

    const char * bugsnag_getIdFromSession(const void *session);

    void bugsnag_setSessionId(const void *session, const char *newId);

}

const char * bugsnag_getIdFromSession(const void *session)
{
    // Duplicate the string as it seems to be freed after being converted into
    // a C# string (?)
    // Fall back to empty string if nil, as strdup(NULL) will crash, and C#
    // strings should never be null either. Should never happen but we live in
    // strange times ¯\_(ツ)_/¯
    return strdup([[(__bridge BugsnagSession *)session id] UTF8String] ?: "");
}

void bugsnag_setSessionId(const void *session, const char *newId){

    ((__bridge BugsnagSession *)session).id = [NSString stringWithUTF8String:newId];

}


void bugsnag_registerForSessionCallbacks(const void *configuration, bool (*callback)(void *session)){
    [((__bridge BugsnagConfiguration *)configuration) addOnSessionBlock:^BOOL (BugsnagSession *session) {    
        return callback((void*)CFBridgingRetain(session));
    }];
}

void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount) {
    BugsnagSessionTracker *tracker = Bugsnag.client.sessionTracker;
    [tracker registerExistingSession:sessionId == NULL ? nil : [NSString stringWithUTF8String:sessionId]
                           startedAt:[NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)startedAt]
                                user:Bugsnag.user
                        handledCount:(NSUInteger)handledCount
                      unhandledCount:(NSUInteger)unhandledCount];
}

void bugsnag_retrieveCurrentSession(const void *session, void (*callback)(const void *instance, const char *sessionId, const char *startedAt, int handled, int unhandled)) {
 
    if([Bugsnag client].sessionTracker.runningSession == NULL)
    {
      callback(session, NULL, NULL, 0, 0);
      return;
    }
    NSDictionary * sessionDict = [[Bugsnag client].sessionTracker.runningSession toDictionary];
    const char *sessionId = [[sessionDict objectForKey:@"id"] UTF8String];
    const char *timeString = [[sessionDict objectForKey:@"startedAt"] UTF8String];
    int handled = [sessionDict[@"handledCount"] integerValue];
    int unhandled = [sessionDict[@"unhandledCount"] integerValue];
    callback(session, sessionId, timeString, handled, unhandled);

}

void bugsnag_markLaunchCompleted() {
  [Bugsnag markLaunchCompleted];
}

void *bugsnag_createConfiguration(char *apiKey) {
  NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];
  BugsnagConfiguration *config = [[BugsnagConfiguration alloc] initWithApiKey:ns_apiKey];
  config.apiKey = ns_apiKey;
  return (void*)CFBridgingRetain(config);
}

void bugsnag_setReleaseStage(const void *configuration, char *releaseStage) {
  NSString *ns_releaseStage = releaseStage == NULL ? nil : [NSString stringWithUTF8String: releaseStage];
  ((__bridge BugsnagConfiguration *)configuration).releaseStage = ns_releaseStage;
}

void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount){
  NSMutableSet *ns_releaseStages = [NSMutableSet new];
  for (int i = 0; i < releaseStagesCount; i++) {
    const char *releaseStage = releaseStages[i];
    if (releaseStage != nil) {
      NSString *ns_releaseStage = [NSString stringWithUTF8String: releaseStage];
      [ns_releaseStages addObject: ns_releaseStage];
    }
  }
  ((__bridge BugsnagConfiguration *)configuration).enabledReleaseStages = ns_releaseStages;
}

void bugsnag_setAppVersion(const void *configuration, char *appVersion) {
  NSString *ns_appVersion = appVersion == NULL ? nil : [NSString stringWithUTF8String: appVersion];
  ((__bridge BugsnagConfiguration *)configuration).appVersion = ns_appVersion;
}

void bugsnag_setAppHangThresholdMillis(const void *configuration, NSUInteger appHangThresholdMillis) {
  ((__bridge BugsnagConfiguration *)configuration).appHangThresholdMillis = appHangThresholdMillis;
}

void bugsnag_setLaunchDurationMillis(const void *configuration, NSUInteger launchDurationMillis) {
  ((__bridge BugsnagConfiguration *)configuration).launchDurationMillis = launchDurationMillis;
}

void bugsnag_setBundleVersion(const void *configuration, char *bundleVersion) {
  NSString *ns_bundleVersion = bundleVersion == NULL ? nil : [NSString stringWithUTF8String: bundleVersion];
  ((__bridge BugsnagConfiguration *)configuration).bundleVersion = ns_bundleVersion;
}

void bugsnag_setAppType(const void *configuration, char *appType) {
  NSString *ns_appType = appType == NULL ? nil : [NSString stringWithUTF8String: appType];
  ((__bridge BugsnagConfiguration *)configuration).appType = ns_appType;
}

void bugsnag_setContext(const void *configuration, char *context) {
  NSString *ns_Context = context == NULL ? nil : [NSString stringWithUTF8String: context];
  [Bugsnag.client setContext:ns_Context];
}

void bugsnag_setContextConfig(const void *configuration, char *context) {
  NSString *ns_Context = context == NULL ? nil : [NSString stringWithUTF8String: context];
  ((__bridge BugsnagConfiguration *)configuration).context = ns_Context;
}

void bugsnag_setMaxBreadcrumbs(const void *configuration, int maxBreadcrumbs) {
  ((__bridge BugsnagConfiguration *)configuration).maxBreadcrumbs = maxBreadcrumbs;
}

void bugsnag_setMaxPersistedEvents(const void *configuration, int maxPersistedEvents) {
  ((__bridge BugsnagConfiguration *)configuration).maxPersistedEvents = maxPersistedEvents;
}

void bugsnag_setEnabledBreadcrumbTypes(const void *configuration, const char *types[], int count){
    if(types == NULL)
    {
        ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes = BSGEnabledBreadcrumbTypeAll;
        return;
    }

    ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes = BSGEnabledBreadcrumbTypeNone;

    for (int i = 0; i < count; i++) {
        const char *enabledType = types[i];
        if (enabledType != nil) {

			NSString *typeString = [[NSString alloc] initWithUTF8String:enabledType];

            if([typeString isEqualToString:@"Navigation"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeNavigation;
            }
            if([typeString isEqualToString:@"Request"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeRequest;
            }
            if([typeString isEqualToString:@"Process"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeProcess;
            }
            if([typeString isEqualToString:@"Log"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeLog;
            }
            if([typeString isEqualToString:@"User"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeUser;
            }
            if([typeString isEqualToString:@"State"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeState;
            }
            if([typeString isEqualToString:@"Error"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledBreadcrumbTypes |= BSGEnabledBreadcrumbTypeError;
            }
        }
      }
}

void bugsnag_setThreadSendPolicy(const void *configuration, char *threadSendPolicy)
{
    NSString *ns_threadSendPolicy = [[NSString alloc] initWithUTF8String:threadSendPolicy];
    if([ns_threadSendPolicy isEqualToString:@"ALWAYS"])
    {
        ((__bridge BugsnagConfiguration *)configuration).sendThreads = BSGThreadSendPolicyAlways;
    }
    if([ns_threadSendPolicy isEqualToString:@"UNHANDLED_ONLY"])
    {
        ((__bridge BugsnagConfiguration *)configuration).sendThreads = BSGThreadSendPolicyUnhandledOnly;
    }
    if([ns_threadSendPolicy isEqualToString:@"NEVER"])
    {
        ((__bridge BugsnagConfiguration *)configuration).sendThreads = BSGThreadSendPolicyNever;
    }
}


void bugsnag_setEnabledErrorTypes(const void *configuration, const char *types[], int count){

    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.appHangs = NO;
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.cppExceptions = NO;
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.signals = NO;
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.cppExceptions = NO;
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.machExceptions = NO;
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.ooms = NO;

    for (int i = 0; i < count; i++) {
        const char *enabledType = types[i];
        if (enabledType != nil) {

			NSString *typeString = [[NSString alloc] initWithUTF8String:enabledType];

            if([typeString isEqualToString:@"AppHangs"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.appHangs = YES;
            }
            if([typeString isEqualToString:@"UnhandledExceptions"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.cppExceptions = YES;
            }
            if([typeString isEqualToString:@"Signals"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.signals = YES;
            }
            if([typeString isEqualToString:@"CppExceptions"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.cppExceptions = YES;
            }
            if([typeString isEqualToString:@"MachExceptions"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.machExceptions = YES;
            }
            if([typeString isEqualToString:@"OOMs"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.ooms = YES;
            }
        }
      }
}

void bugsnag_setDiscardClasses(const void *configuration, const char *classNames[], int count){
  NSMutableSet *ns_classNames = [NSMutableSet new];
  for (int i = 0; i < count; i++) {
    const char *className = classNames[i];
    if (className != nil) {
      NSString *ns_className = [NSString stringWithUTF8String: className];
      [ns_classNames addObject: ns_className];
    }
  }
  ((__bridge BugsnagConfiguration *)configuration).discardClasses = ns_classNames;
}

void bugsnag_setRedactedKeys(const void *configuration, const char *redactedKeys[], int count){
  NSMutableSet *ns_redactedKeys = [NSMutableSet new];
  for (int i = 0; i < count; i++) {
    const char *key = redactedKeys[i];
    if (key != nil) {
      NSString *ns_key = [NSString stringWithUTF8String: key];
      [ns_redactedKeys addObject: ns_key];
    }
  }
  ((__bridge BugsnagConfiguration *)configuration).redactedKeys = ns_redactedKeys;
}

void bugsnag_setAutoNotifyConfig(const void *configuration, bool autoNotify) {
  ((__bridge BugsnagConfiguration *)configuration).autoDetectErrors = autoNotify;
}

void bugsnag_setAutoTrackSessions(const void *configuration, bool autoTrackSessions) {
  ((__bridge BugsnagConfiguration *)configuration).autoTrackSessions  = autoTrackSessions;
}

void bugsnag_setPersistUser(const void *configuration, bool persistUser) {
  ((__bridge BugsnagConfiguration *)configuration).persistUser = persistUser;
}

void bugsnag_setSendLaunchCrashesSynchronously(const void *configuration, bool sendLaunchCrashesSynchronously) {
  ((__bridge BugsnagConfiguration *)configuration).sendLaunchCrashesSynchronously = sendLaunchCrashesSynchronously;
}

void bugsnag_setAutoNotify(bool autoNotify) {
  Bugsnag.client.autoNotify = autoNotify;
}

void bugsnag_setEndpoints(const void *configuration, char *notifyURL, char *sessionsURL) {
  if (notifyURL == NULL || sessionsURL == NULL)
    return;

  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  NSString *ns_sessionsURL = [NSString stringWithUTF8String: sessionsURL];

  ((__bridge BugsnagConfiguration *)configuration).endpoints = [[BugsnagEndpointConfiguration alloc] initWithNotify:ns_notifyURL sessions:ns_sessionsURL];
}

void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount) {
  if (tab == NULL)
    return;

  NSString *tabName = [NSString stringWithUTF8String: tab];
  NSMutableDictionary *ns_metadata = [NSMutableDictionary new];

  for (int i = 0; i < metadataCount; i += 2) {
    NSString *key = metadata[i] != NULL
        ? [NSString stringWithUTF8String:metadata[i]]
        : nil;
    if (key == nil) {
      continue;
    }
    NSString *value = metadata[i+1] != NULL
        ? [NSString stringWithUTF8String:metadata[i+1]]
        : nil;
    ns_metadata[key] = value;

  }
  [Bugsnag.client addMetadata:ns_metadata toSection:tabName];
}

void bugsnag_retrieveMetaData(const void *metadata, void (*callback)(const void *instance, const char *tab,const char *keys[], int keys_size, const char *values[], int values_size)) {

    for (NSString* sectionKey in [Bugsnag.client metadata].dictionary.allKeys) {
                 NSDictionary* sectionDictionary = [[Bugsnag.client metadata].dictionary valueForKey:sectionKey];
                 NSArray *keys = [sectionDictionary allKeys];
                 NSArray *values = [sectionDictionary allValues];
                 int count = 0;
                 if ([keys count] <= INT_MAX) {
                   count = (int)[keys count];
                 }
                 const char **c_keys = (const char **) malloc(sizeof(char *) * ((size_t)count + 1));
                 const char **c_values = (const char **) malloc(sizeof(char *) * ((size_t)count + 1));
                 for (NSUInteger i = 0; i < (NSUInteger)count; i++) {
                   c_keys[i] = [[keys objectAtIndex: i] UTF8String];
                   c_values[i] = [[[values objectAtIndex: i]description] UTF8String];
                 }
                callback(metadata, [sectionKey UTF8String],c_keys,count,c_values,count);
                free(c_keys);
                free(c_values);
           }

}

void bugsnag_removeMetadata(const void *configuration, const char *tab) {
  if (tab == NULL)
    return;

  NSString *tabName = [NSString stringWithUTF8String:tab];
  [Bugsnag.client clearMetadataFromSection:tabName];
}

void bugsnag_startBugsnagWithConfiguration(const void *configuration, char *notifierVersion) {
  if (notifierVersion) {
    ((__bridge BugsnagConfiguration *)configuration).notifier =
      [[BugsnagNotifier alloc] initWithName:@"Unity Bugsnag Notifier"
                                    version:@(notifierVersion)
                                        url:@"https://github.com/bugsnag/bugsnag-unity"
                               dependencies:@[[[BugsnagNotifier alloc] init]]];
  }
  [Bugsnag startWithConfiguration: (__bridge BugsnagConfiguration *)configuration];
  // Memory introspection is unused in a C/C++ context
  [BSG_KSCrash sharedInstance].introspectMemory = NO;
}

void bugsnag_addBreadcrumb(char *message, char *type, char *metadata[], int metadataCount) {
  NSString *ns_message = [NSString stringWithUTF8String: message == NULL ? "<empty>" : message];
  [Bugsnag.client addBreadcrumbWithBlock:^(BugsnagBreadcrumb *crumb) {

      crumb.message = ns_message;
      crumb.type = BSGBreadcrumbTypeFromString([NSString stringWithUTF8String:type]);

      if (metadataCount > 0) {
        NSMutableDictionary *ns_metadata = [NSMutableDictionary new];

        for (int i = 0; i < metadataCount - 1; i += 2) {
          char *key = metadata[i];
          char *value = metadata[i+1];
          if (key == NULL || value == NULL)
              continue;
          NSString *ns_key = [NSString stringWithUTF8String:key];
          NSString *ns_value = [NSString stringWithUTF8String:value];
          ns_metadata[ns_key] = ns_value;
        }

        crumb.metadata = ns_metadata;
      }
  }];
}

void bugsnag_retrieveBreadcrumbs(const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], int keys_size, const char *values[], int values_size)) {
  [Bugsnag.breadcrumbs enumerateObjectsUsingBlock:^(BugsnagBreadcrumb *crumb, __unused NSUInteger index, __unused BOOL *stop){
    const char *message = [crumb.message UTF8String];
    const char *timestamp = [[BSG_RFC3339DateTool stringFromDate:crumb.timestamp] UTF8String];
    const char *type = [BSGBreadcrumbTypeValue(crumb.type) UTF8String];

    NSDictionary *metadata = crumb.metadata;

    NSArray *keys = [metadata allKeys];
    NSArray *values = [metadata allValues];

    int count = 0;

    if ([keys count] <= INT_MAX) {
      count = (int)[keys count];
    }

    const char **c_keys = (const char **) malloc(sizeof(char *) * ((size_t)count + 1));
    const char **c_values = (const char **) malloc(sizeof(char *) * ((size_t)count + 1));

    for (NSUInteger i = 0; i < (NSUInteger)count; i++) {
      c_keys[i] = [[keys objectAtIndex: i] UTF8String];
      c_values[i] = [[values objectAtIndex: i] UTF8String];
    }

    breadcrumb(managedBreadcrumbs, message, timestamp, type, c_keys, count, c_values, count);
    free(c_keys);
    free(c_values);
  }];
}

void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value)) {
  NSDictionary *sysInfo = [BSG_KSSystemInfo systemInfo];

  NSString *bundleVersion = [Bugsnag configuration].bundleVersion ?: sysInfo[@BSG_KSSystemField_BundleVersion];
  callback(appData, "bundleVersion", [bundleVersion UTF8String]);

  callback(appData, "id", [sysInfo[@BSG_KSSystemField_BundleID] UTF8String]);

  NSString *appType = [Bugsnag configuration].appType ?: sysInfo[@BSG_KSSystemField_SystemName];
  callback(appData, "type", [appType UTF8String]);

  NSString *version = [Bugsnag configuration].appVersion ?: sysInfo[@BSG_KSSystemField_BundleShortVersion];
  callback(appData, "version", [version UTF8String]);

  BugsnagAppWithState *app = [[Bugsnag client] generateAppWithState:sysInfo];
  NSString *isLaunching = app.isLaunching ? @"true" : @"false";
  callback(appData, "isLaunching", [isLaunching UTF8String]);

}

void bugsnag_retrieveLastRunInfo(const void *lastRuninfo, void (*callback)(const void *instance, bool crashed, bool crashedDuringLaunch, int consecutiveLaunchCrashes)) {

  int consecutiveLaunchCrashes = Bugsnag.lastRunInfo.consecutiveLaunchCrashes;
  bool crashed = Bugsnag.lastRunInfo.crashed;
  bool crashedDuringLaunch = Bugsnag.lastRunInfo.crashedDuringLaunch;

  callback(lastRuninfo, crashed, crashedDuringLaunch, consecutiveLaunchCrashes);

}

  void bugsnag_registerForOnSendCallbacks(const void *configuration, bool (*callback)(const char *test)){
    [((__bridge BugsnagConfiguration *)configuration) addOnSendErrorBlock:^BOOL (BugsnagEvent *event) {       
        return callback([@"hello" UTF8String]);
    }];
  }



void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value)) {
  NSDictionary *sysInfo = [BSG_KSSystemInfo systemInfo];

  NSFileManager *fileManager = [NSFileManager defaultManager];

  NSError *error;
  NSDictionary *fileSystemAttrs = [fileManager attributesOfFileSystemForPath:NSHomeDirectory() error:&error];

  if (error) {
      bsg_log_warn(@"Failed to read free disk space: %@", error);
  }

  NSNumber *freeBytes = [fileSystemAttrs objectForKey:NSFileSystemFreeSize];
  callback(deviceData, "freeDisk", [[freeBytes stringValue] UTF8String]);

  uint64_t freeMemory = bsg_ksmachfreeMemory();
  char buff[30];
  sprintf(buff, "%lld", freeMemory);
  callback(deviceData, "freeMemory", buff);

  callback(deviceData, "id", [sysInfo[@BSG_KSSystemField_DeviceAppHash] UTF8String]);
  callback(deviceData, "jailbroken", [[sysInfo[@BSG_KSSystemField_Jailbroken] stringValue] UTF8String]);
  callback(deviceData, "locale", [[[NSLocale currentLocale] localeIdentifier] UTF8String]);
  callback(deviceData, "manufacturer", "Apple");
  callback(deviceData, "model", [sysInfo[@BSG_KSSystemField_Machine] UTF8String]);
  callback(deviceData, "modelNumber", [sysInfo[@BSG_KSSystemField_Model] UTF8String]);
  callback(deviceData, "osName", [sysInfo[@BSG_KSSystemField_SystemName] UTF8String]);
  callback(deviceData, "osVersion", [sysInfo[@BSG_KSSystemField_SystemVersion] UTF8String]);
  callback(deviceData, "osBuild", [sysInfo[@BSG_KSSystemField_OSVersion] UTF8String]);
}

void bugsnag_populateUser(bugsnag_user *user) {
  NSDictionary *sysInfo = [BSG_KSSystemInfo systemInfo];
  user->user_id = [sysInfo[@BSG_KSSystemField_DeviceAppHash] UTF8String];
}

void bugsnag_setUser(char *userId, char *userName, char *userEmail) {
    [Bugsnag setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
            withEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]
             andName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]];
}

@interface Bugsnag ()
+ (BugsnagNotifier *)notifier;
@end

void bugsnag_startSession() {
    [Bugsnag startSession];
}

void bugsnag_pauseSession() {
    [Bugsnag pauseSession];
}

bool bugsnag_resumeSession() {
    return [Bugsnag resumeSession];
}
