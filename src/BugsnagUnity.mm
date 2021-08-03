#import "Bugsnag.h"
#import "BugsnagConfiguration.h"
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

extern "C" {
  struct bugsnag_user {
    const char *user_id;
  };

  void *bugsnag_createConfiguration(char *apiKey);

  void bugsnag_setReleaseStage(const void *configuration, char *releaseStage);

  void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount);

  void bugsnag_setAppVersion(const void *configuration, char *appVersion);

  void bugsnag_setAutoNotifyConfig(const void *configuration, bool autoNotify);

  void bugsnag_setAutoNotify(bool autoNotify);

  void bugsnag_setContext(const void *configuration, char *context);
  void bugsnag_setContextConfig(const void *configuration, char *context);

  void bugsnag_setMaxBreadcrumbs(const void *configuration, int maxBreadcrumbs);
  void bugsnag_setEnabledBreadcrumbTypes(const void *configuration, const char *types[], int count);

  void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL);

  void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount);
  void bugsnag_removeMetadata(const void *configuration, const char *tab);
  void bugsnag_retrieveMetaData(const void *metadata, void (*callback)(const void *instance, const char *tab, const char *keys[], int keys_size, const char *values[], int values_size));

  void bugsnag_startBugsnagWithConfiguration(const void *configuration, char *notifierVersion);

  void bugsnag_addBreadcrumb(char *name, char *type, char *metadata[], int metadataCount);
  void bugsnag_retrieveBreadcrumbs(const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], int keys_size, const char *values[], int values_size));

  void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value));
  void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value));

  void bugsnag_populateUser(bugsnag_user *user);
  void bugsnag_setUser(char *userId, char *userName, char *userEmail);
  void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount);

  void bugsnag_setEnabledErrorTypes(const void *configuration, const char *types[], int count);

  void bugsnag_setAppHangThresholdMillis(const void *configuration, NSUInteger appHangThresholdMillis);


}

void *bugsnag_createConfiguration(char *apiKey) {
  NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];
  BugsnagConfiguration *config = [[BugsnagConfiguration alloc] initWithApiKey:ns_apiKey];
  config.apiKey = ns_apiKey;
  config.autoTrackSessions = NO;
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

void bugsnag_setAutoNotifyConfig(const void *configuration, bool autoNotify) {
  ((__bridge BugsnagConfiguration *)configuration).autoDetectErrors = autoNotify;
}

void bugsnag_setAutoNotify(bool autoNotify) {
  Bugsnag.client.autoNotify = autoNotify;
}

void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL) {
  if (notifyURL == NULL)
    return;
  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  ((__bridge BugsnagConfiguration *)configuration).endpoints.notify = ns_notifyURL;
  // Workaround for endpoint stale-cache issue: Force-trigger re-processing by reassinging endpoint
  ((__bridge BugsnagConfiguration *)configuration).endpoints = ((__bridge BugsnagConfiguration *)configuration).endpoints;
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
  [Bugsnag startWithConfiguration: (__bridge BugsnagConfiguration *)configuration];
  // Memory introspection is unused in a C/C++ context
  [BSG_KSCrash sharedInstance].introspectMemory = NO;
  if (notifierVersion != NULL) {
    NSString *ns_version = [NSString stringWithUTF8String:notifierVersion];
    BugsnagNotifier *notifier = Bugsnag.client.notifier;
    notifier.version = ns_version;
    notifier.name = @"Bugsnag Unity (Cocoa)";
    notifier.url = @"https://github.com/bugsnag/bugsnag-unity";
  }
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

  callback(appData, "bundleVersion", [sysInfo[@BSG_KSSystemField_BundleVersion] UTF8String]);
  callback(appData, "id", [sysInfo[@BSG_KSSystemField_BundleID] UTF8String]);
  callback(appData, "type", [sysInfo[@BSG_KSSystemField_SystemName] UTF8String]);
  NSString *version = [Bugsnag configuration].appVersion ?: sysInfo[@BSG_KSSystemField_BundleShortVersion];
  callback(appData, "version", [version UTF8String]);
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

void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount) {
    BugsnagSessionTracker *tracker = Bugsnag.client.sessionTracker;
    [tracker registerExistingSession:sessionId == NULL ? nil : [NSString stringWithUTF8String:sessionId]
                           startedAt:[NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)startedAt]
                                user:Bugsnag.user
                        handledCount:(NSUInteger)handledCount
                      unhandledCount:(NSUInteger)unhandledCount];
}
