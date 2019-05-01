#import "Bugsnag.h"
#import "BugsnagConfiguration.h"
#import "BugsnagLogger.h"
#import "BugsnagUser.h"
#import "BugsnagNotifier.h"
#import "BugsnagSessionTracker.h"
#import "BSG_KSSystemInfo.h"
#import "BSG_KSMach.h"

@interface Bugsnag ()
+ (id)notifier;
@end

extern "C" {
  struct bugsnag_user {
    const char *user_id;
  };

  void *bugsnag_createConfiguration(char *apiKey);

  const char *bugsnag_getApiKey(const void *configuration);

  void bugsnag_setReleaseStage(const void *configuration, char *releaseStage);
  const char *bugsnag_getReleaseStage(const void *configuration);

  void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount);
  void bugsnag_getNotifyReleaseStages(const void *configuration, const void *managedConfiguration, void (*callback)(const void *instance, const char *releaseStages[], int size));

  void bugsnag_setAppVersion(const void *configuration, char *appVersion);
  const char *bugsnag_getAppVersion(const void *configuration);

  void bugsnag_setContext(const void *configuration, char *context);
  const char *bugsnag_getContext(const void *configuration);

  void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL);
  const char *bugsnag_getNotifyUrl(const void *configuration);

  void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount);

  void bugsnag_startBugsnagWithConfiguration(const void *configuration, char *notifierVersion);

  void *bugsnag_createBreadcrumbs(const void *configuration);
  void bugsnag_addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount);
  void bugsnag_retrieveBreadcrumbs(const void *breadcrumbs, const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], int keys_size, const char *values[], int values_size));

  void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value));
  void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value));

  void bugsnag_populateUser(bugsnag_user *user);
  void bugsnag_setUser(char *userId, char *userName, char *userEmail);
  void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount);
}

void *bugsnag_createConfiguration(char *apiKey) {
  NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];
  BugsnagConfiguration *config = [BugsnagConfiguration new];
  config.apiKey = ns_apiKey;
  return (void*)CFBridgingRetain(config);
}

const char *bugsnag_getApiKey(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).apiKey UTF8String];
}

void bugsnag_setReleaseStage(const void *configuration, char *releaseStage) {
  NSString *ns_releaseStage = releaseStage == NULL ? nil : [NSString stringWithUTF8String: releaseStage];
  ((__bridge BugsnagConfiguration *)configuration).releaseStage = ns_releaseStage;
}

const char *bugsnag_getReleaseStage(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).releaseStage UTF8String];
}

void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount){
  NSMutableArray *ns_releaseStages = [NSMutableArray new];
  for (size_t i = 0; i < releaseStagesCount; i++) {
    [ns_releaseStages addObject: [NSString stringWithUTF8String: releaseStages[i]]];
  }
  ((__bridge BugsnagConfiguration *)configuration).notifyReleaseStages = ns_releaseStages;
}

void bugsnag_getNotifyReleaseStages(const void *configuration, const void *managedConfiguration, void (*callback)(const void *instance, const char *releaseStages[], int size)) {
  NSArray *releaseStages = ((__bridge BugsnagConfiguration *)configuration).notifyReleaseStages;
  int count = 0;

  if ([releaseStages count] <= INT_MAX) {
    count = (int)[releaseStages count];
  }

  const char **c_releaseStages = (const char **) malloc(sizeof(char *) * (count + 1));

  for (int i = 0; i < count; i++) {
    c_releaseStages[i] = [[releaseStages objectAtIndex: i] UTF8String];
  }

  callback(managedConfiguration, c_releaseStages, count);
}

void bugsnag_setAppVersion(const void *configuration, char *appVersion) {
  NSString *ns_appVersion = appVersion == NULL ? nil : [NSString stringWithUTF8String: appVersion];
  ((__bridge BugsnagConfiguration *)configuration).appVersion = ns_appVersion;
}

const char *bugsnag_getAppVersion(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).appVersion UTF8String];
}

void bugsnag_setContext(const void *configuration, char *context) {
  NSString *ns_Context = context == NULL ? nil : [NSString stringWithUTF8String: context];
  ((__bridge BugsnagConfiguration *)configuration).context = ns_Context;
}

const char *bugsnag_getContext(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).context UTF8String];
}

void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL) {
  if (notifyURL == NULL)
    return;
  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  [((__bridge BugsnagConfiguration *)configuration) setEndpointsForNotify: ns_notifyURL sessions: nil];
}

const char *bugsnag_getNotifyUrl(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).notifyURL.absoluteString UTF8String];
}

void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount) {
  BugsnagConfiguration *ns_configuration = (__bridge BugsnagConfiguration *)configuration;
  if (tab == NULL)
    return;

  NSString *tabName = [NSString stringWithUTF8String: tab];

  for (size_t i = 0; i < metadataCount; i += 2) {
    [ns_configuration.metaData addAttribute: [NSString stringWithUTF8String: metadata[i]]
                                  withValue: [NSString stringWithUTF8String: metadata[i+1]]
                              toTabWithName: tabName];
  }
}

void bugsnag_startBugsnagWithConfiguration(const void *configuration, char *notifierVersion) {
  [Bugsnag startBugsnagWithConfiguration: (__bridge BugsnagConfiguration *)configuration];
  if (notifierVersion != NULL) {
    NSString *ns_version = [NSString stringWithUTF8String:notifierVersion];
    id notifier = [Bugsnag notifier];
    [notifier setValue:@{
        @"version": ns_version,
        @"name": @"Bugsnag Unity (Cocoa)",
        @"url": @"https://github.com/bugsnag/bugsnag-unity"
    } forKey:@"details"];
  }
}

void *bugsnag_createBreadcrumbs(const void *configuration) {
  return (__bridge void*)((__bridge BugsnagConfiguration *)configuration).breadcrumbs;
}

void bugsnag_addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount) {
  BugsnagBreadcrumbs *ns_breadcrumbs = ((__bridge BugsnagBreadcrumbs *) breadcrumbs);
  NSString *ns_name = [NSString stringWithUTF8String: name == NULL ? "<empty>" : name];
  [ns_breadcrumbs addBreadcrumbWithBlock:^(BugsnagBreadcrumb *crumb) {
      crumb.name = ns_name;

      if (strcmp(type, "log") == 0) {
        crumb.type = BSGBreadcrumbTypeLog;
      } else if (strcmp(type, "user") == 0) {
        crumb.type = BSGBreadcrumbTypeUser;
      } else if (strcmp(type, "error") == 0) {
        crumb.type = BSGBreadcrumbTypeError;
      } else if (strcmp(type, "state") == 0) {
        crumb.type = BSGBreadcrumbTypeState;
      } else if (strcmp(type, "manual") == 0) {
        crumb.type = BSGBreadcrumbTypeManual;
      } else if (strcmp(type, "process") == 0) {
        crumb.type = BSGBreadcrumbTypeProcess;
      } else if (strcmp(type, "request") == 0) {
        crumb.type = BSGBreadcrumbTypeRequest;
      } else if (strcmp(type, "navigation") == 0) {
        crumb.type = BSGBreadcrumbTypeNavigation;
      }

      if (metadataCount > 0) {
        NSMutableDictionary *ns_metadata = [NSMutableDictionary new];

        for (size_t i = 0; i < metadataCount - 1; i += 2) {
          char *key = metadata[i];
          char *value = metadata[i+1];
          if (key == NULL || value == NULL)
              continue;
          [ns_metadata setValue:[NSString stringWithUTF8String:value]
                         forKey:[NSString stringWithUTF8String:key]];
        }

        crumb.metadata = ns_metadata;
      }
  }];
}

void bugsnag_retrieveBreadcrumbs(const void *breadcrumbs, const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], int keys_size, const char *values[], int values_size)) {
  NSArray *crumbs = [((__bridge BugsnagBreadcrumbs *) breadcrumbs) arrayValue];
  [crumbs enumerateObjectsUsingBlock:^(id crumb, NSUInteger index, BOOL *stop){
    const char *name = [[crumb valueForKey: @"name"] UTF8String];
    const char *timestamp = [[crumb valueForKey: @"timestamp"] UTF8String];
    const char *type = [[crumb valueForKey: @"type"] UTF8String];

    NSDictionary *metadata = [crumb valueForKey: @"metaData"];

    NSArray *keys = [metadata allKeys];
    NSArray *values = [metadata allValues];

    int count = 0;

    if ([keys count] <= INT_MAX) {
      count = (int)[keys count];
    }

    const char **c_keys = (const char **) malloc(sizeof(char *) * (count + 1));
    const char **c_values = (const char **) malloc(sizeof(char *) * (count + 1));

    for (int i = 0; i < count; i++) {
      c_keys[i] = [[keys objectAtIndex: i] UTF8String];
      c_values[i] = [[values objectAtIndex: i] UTF8String];
    }

    breadcrumb(managedBreadcrumbs, name, timestamp, type, c_keys, count, c_values, count);
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
    [[Bugsnag configuration] setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
                            withName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]
                            andEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]];
}

@interface Bugsnag ()
+ (BugsnagNotifier *)notifier;
@end

void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount) {
    BugsnagSessionTracker *tracker = [[Bugsnag notifier] sessionTracker];
    [tracker registerExistingSession:sessionId == NULL ? nil : [NSString stringWithUTF8String:sessionId]
                           startedAt:[NSDate dateWithTimeIntervalSince1970:startedAt]
                                user:[[Bugsnag configuration] currentUser]
                        handledCount:handledCount
                      unhandledCount:unhandledCount];
}
