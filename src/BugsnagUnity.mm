#import <Bugsnag/Bugsnag.h>
#import <Bugsnag/BugsnagConfiguration.h>

extern "C" {
  void *createConfiguration(char *apiKey);

  const char *getApiKey(const void *configuration);

  void setReleaseStage(const void *configuration, char *releaseStage);
  const char *getReleaseStage(const void *configuration);

  void setAppVersion(const void *configuration, char *appVersion);
  const char *getAppVersion(const void *configuration);

  void setContext(const void *configuration, char *context);
  const char *getContext(const void *configuration);

  void setNotifyUrl(const void *configuration, char *notifyURL);
  const char *getNotifyUrl(const void *configuration);

  void startBugsnagWithConfiguration(const void *configuration);

  void *createBreadcrumbs(const void *configuration);
  void addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount);
  void retrieveBreadcrumbs(const void *breadcrumbs, void (*breadcrumb)(const char *name, const char *timestamp, const char *type, const char *keys[], NSUInteger keys_size, const char *values[], NSUInteger values_size));
}

void *createConfiguration(char *apiKey) {
  NSString *ns_apiKey = [NSString stringWithUTF8String: apiKey];
  BugsnagConfiguration *config = [BugsnagConfiguration new];
  config.apiKey = ns_apiKey;
  return (void*)CFBridgingRetain(config);
}

const char *getApiKey(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).apiKey UTF8String];
}

void setReleaseStage(const void *configuration, char *releaseStage) {
  NSString *ns_releaseStage = [NSString stringWithUTF8String: releaseStage];
  ((__bridge BugsnagConfiguration *)configuration).releaseStage = ns_releaseStage;
}

const char *getReleaseStage(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).releaseStage UTF8String];
}

void setAppVersion(const void *configuration, char *appVersion) {
  NSString *ns_appVersion = [NSString stringWithUTF8String: appVersion];
  ((__bridge BugsnagConfiguration *)configuration).appVersion = ns_appVersion;
}

const char *getAppVersion(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).appVersion UTF8String];
}

void setContext(const void *configuration, char *context) {
  NSString *ns_Context = [NSString stringWithUTF8String: context];
  ((__bridge BugsnagConfiguration *)configuration).context = ns_Context;
}

const char *getContext(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).context UTF8String];
}

void setNotifyUrl(const void *configuration, char *notifyURL) {
  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  NSURL *endpoint = [NSURL URLWithString: ns_notifyURL];
  ((__bridge BugsnagConfiguration *)configuration).notifyURL = endpoint;
}

const char *getNotifyUrl(const void *configuration) {
  return [((__bridge BugsnagConfiguration *)configuration).notifyURL.absoluteString UTF8String];
}

void startBugsnagWithConfiguration(const void *configuration) {
  [Bugsnag startBugsnagWithConfiguration: (__bridge BugsnagConfiguration *)configuration];
}

void *createBreadcrumbs(const void *configuration) {
  return (__bridge void*)((__bridge BugsnagConfiguration *)configuration).breadcrumbs;
}

void addBreadcrumb(const void *breadcrumbs, char *name, char *type, char *metadata[], int metadataCount) {
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

      NSMutableDictionary *ns_metadata = [NSMutableDictionary new];

      for (size_t i = 0; i < metadataCount; i += 2) {
        NSString *key = [NSString stringWithUTF8String: metadata[i]];
        NSString *value = [NSString stringWithUTF8String: metadata[i+1]];
        [ns_metadata setValue: value forKey: key];
      }

      crumb.metadata = ns_metadata;
  }];
}

void retrieveBreadcrumbs(const void *breadcrumbs, void (*breadcrumb)(const char *name, const char *timestamp, const char *type, const char *keys[], NSUInteger keys_size, const char *values[], NSUInteger values_size)) {
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

    breadcrumb(name, timestamp, type, c_keys, count, c_values, count);
  }];
}
