#import "Bugsnag.h"
#import "BugsnagConfiguration.h"
#import "BSG_KSSystemInfo.h"
#import "BSG_KSMach.h"

extern "C" {
  void *bugsnag_createConfiguration(char *apiKey);

  const char *bugsnag_getApiKey(const void *configuration);

  void bugsnag_setReleaseStage(const void *configuration, char *releaseStage);
  const char *bugsnag_getReleaseStage(const void *configuration);

  void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount);
  void bugsnag_getNotifyReleaseStages(const void *configuration, const void *managedConfiguration, void (*callback)(const void *instance, const char *releaseStages[], NSUInteger size));

  void bugsnag_setAppVersion(const void *configuration, char *appVersion);
  const char *bugsnag_getAppVersion(const void *configuration);

  void bugsnag_setContext(const void *configuration, char *context);
  const char *bugsnag_getContext(const void *configuration);

  void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL);
  const char *bugsnag_getNotifyUrl(const void *configuration);

  void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount);

  void bugsnag_startBugsnagWithConfiguration(const void *configuration);

  void *bugsnag_createBreadcrumbs(const void *configuration);
  void bugsnag_addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount);
  void bugsnag_retrieveBreadcrumbs(const void *breadcrumbs, const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], NSUInteger keys_size, const char *values[], NSUInteger values_size));

  void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value));
  void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value));
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
  NSString *ns_releaseStage = [NSString stringWithUTF8String: releaseStage];
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

void bugsnag_getNotifyReleaseStages(const void *configuration, const void *managedConfiguration, void (*callback)(const void *instance, const char *releaseStages[], NSUInteger size)) {
  NSArray *releaseStages = ((__bridge BugsnagConfiguration *)configuration).notifyReleaseStages;
  NSUInteger count = [releaseStages count];
  const char **c_releaseStages = (const char **) malloc(sizeof(char *) * (count + 1));

  for (NSUInteger i = 0; i < count; i++) {
    c_releaseStages[i] = [[releaseStages objectAtIndex: i] UTF8String];
  }

  callback(managedConfiguration, c_releaseStages, count);
}

void bugsnag_setAppVersion(const void *configuration, char *appVersion) {
  NSString *ns_appVersion = [NSString stringWithUTF8String: appVersion];
  ((__bridge BugsnagConfiguration *)configuration).appVersion = ns_appVersion;
}

const char *bugsnag_getAppVersion(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).appVersion UTF8String];
}

void bugsnag_setContext(const void *configuration, char *context) {
  NSString *ns_Context = [NSString stringWithUTF8String: context];
  ((__bridge BugsnagConfiguration *)configuration).context = ns_Context;
}

const char *bugsnag_getContext(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).context UTF8String];
}

void bugsnag_setNotifyUrl(const void *configuration, char *notifyURL) {
  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  NSURL *endpoint = [NSURL URLWithString: ns_notifyURL];
  ((__bridge BugsnagConfiguration *)configuration).notifyURL = endpoint;
}

const char *bugsnag_getNotifyUrl(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).notifyURL.absoluteString UTF8String];
}

void bugsnag_setMetadata(const void *configuration, const char *tab, const char *metadata[], int metadataCount) {
  BugsnagConfiguration *ns_configuration = (__bridge BugsnagConfiguration *)configuration;
  NSString *tabName = [NSString stringWithUTF8String: tab];

  for (size_t i = 0; i < metadataCount; i += 2) {
    [ns_configuration.metaData addAttribute: [NSString stringWithUTF8String: metadata[i]]
                                  withValue: [NSString stringWithUTF8String: metadata[i+1]]
                              toTabWithName: tabName];
  }
}

void bugsnag_startBugsnagWithConfiguration(const void *configuration) {
  [Bugsnag startBugsnagWithConfiguration: (__bridge BugsnagConfiguration *)configuration];
}

void *bugsnag_createBreadcrumbs(const void *configuration) {
  return (__bridge void*)((__bridge BugsnagConfiguration *)configuration).breadcrumbs;
}

void bugsnag_addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount) {
  BugsnagBreadcrumbs *ns_breadcrumbs = ((__bridge BugsnagBreadcrumbs *) breadcrumbs);
  NSString *ns_name = [NSString stringWithUTF8String: name];
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

        for (size_t i = 0; i < metadataCount; i += 2) {
          NSString *key = [NSString stringWithUTF8String: metadata[i]];
          NSString *value = [NSString stringWithUTF8String: metadata[i+1]];
          [ns_metadata setValue: value forKey: key];
        }

        crumb.metadata = ns_metadata;
      }
  }];
}

void bugsnag_retrieveBreadcrumbs(const void *breadcrumbs, const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *keys[], NSUInteger keys_size, const char *values[], NSUInteger values_size)) {
  NSArray *crumbs = [((__bridge BugsnagBreadcrumbs *) breadcrumbs) arrayValue];
  [crumbs enumerateObjectsUsingBlock:^(id crumb, NSUInteger index, BOOL *stop){
    const char *name = [[crumb valueForKey: @"name"] UTF8String];
    const char *timestamp = [[crumb valueForKey: @"timestamp"] UTF8String];
    const char *type = [[crumb valueForKey: @"type"] UTF8String];

    NSDictionary *metadata = [crumb valueForKey: @"metaData"];

    NSArray *keys = [metadata allKeys];
    NSArray *values = [metadata allValues];

    NSUInteger count = [keys count];
    const char **c_keys = (const char **) malloc(sizeof(char *) * (count + 1));
    const char **c_values = (const char **) malloc(sizeof(char *) * (count + 1));

    for (NSUInteger i = 0; i < count; i++) {
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
  callback(appData, "version", [sysInfo[@BSG_KSSystemField_BundleShortVersion] UTF8String]);
}

void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value)) {
  NSDictionary *sysInfo = [BSG_KSSystemInfo systemInfo];

  NSFileManager *fileManager = [NSFileManager defaultManager];
  NSArray *searchPaths = NSSearchPathForDirectoriesInDomains(
                                                             NSDocumentDirectory, NSUserDomainMask, true);
  NSString *path = [searchPaths lastObject];

  NSError *error;
  NSDictionary *fileSystemAttrs =
  [fileManager attributesOfFileSystemForPath:path error:&error];

  if (!error) {
      NSNumber *freeBytes = [fileSystemAttrs objectForKey:NSFileSystemFreeSize];
      callback(deviceData, "freeDisk", [[freeBytes stringValue] UTF8String]);
  }

  uint64_t freeMemory = bsg_ksmachfreeMemory();
  char buff[30];
  sprintf(buff, "%lld", freeMemory);
  callback(deviceData, "freeMemory", buff);

  callback(deviceData, "jailbroken", [[sysInfo[@BSG_KSSystemField_Jailbroken] stringValue] UTF8String]);
  callback(deviceData, "locale", [[[NSLocale currentLocale] localeIdentifier] UTF8String]);
  // callback("manufacturer", sysInfo[@"Apple"]);//does this exist?
  callback(deviceData, "manufacturer", "Apple");//does this exist?
  callback(deviceData, "model", [sysInfo[@BSG_KSSystemField_Machine] UTF8String]);
  callback(deviceData, "modelNumber", [sysInfo[@BSG_KSSystemField_Model] UTF8String]);
  callback(deviceData, "osName", [sysInfo[@BSG_KSSystemField_SystemName] UTF8String]);
  callback(deviceData, "osVersion", [sysInfo[@BSG_KSSystemField_SystemVersion] UTF8String]);
}
