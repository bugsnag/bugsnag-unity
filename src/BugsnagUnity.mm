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
