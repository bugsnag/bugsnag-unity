#import "BugsnagInternals.h"

 struct bugsnag_user {
        const char *user_id;
        const char *user_name;
        const char *user_email;
    };

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
}

static const char * getJson(id obj) {
    if (!obj) {
        return NULL;
    }
    @try {
        NSError *error = nil;
        NSData *data = [NSJSONSerialization dataWithJSONObject:obj options:0 error:&error];
        if (data) {
            return strndup((const char *)data.bytes, data.length);
        } else {
            NSLog(@"%@", error);
        }
    } @catch (NSException *exception) {
        NSLog(@"%@", exception);
    }
    return NULL;
}

void bugsnag_clearMetadata(const char * section){
    if(section == NULL)
    {
        return;   
    }
    [Bugsnag clearMetadataFromSection:@(section)];
}

void bugsnag_clearMetadataWithKey(const char * section, const char * key){
    if(section == NULL || key == NULL)
    {
        return;   
    }
    [Bugsnag clearMetadataFromSection:@(section) withKey:@(key)];
}

NSDictionary * getDictionaryFromMetadataJson(const char * jsonString){
    @try {
        NSError *error = nil;
        NSData *data = [NSData dataWithBytesNoCopy:(void *)jsonString length:strlen(jsonString) freeWhenDone:NO];
        id metadata = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
        if ([metadata isKindOfClass:[NSDictionary class]]) {
            return metadata;
        }
    } @catch (NSException *exception) {
        NSLog(@"%@", exception);
    }
    return nil;
}

static NSString * stringFromDate(NSDate *date) {
  static NSDateFormatter *dateFormatter;
  static dispatch_once_t onceToken;
  dispatch_once(&onceToken, ^{
    dateFormatter = [NSDateFormatter new];
    dateFormatter.locale = [[NSLocale alloc] initWithLocaleIdentifier:@"en_US_POSIX"];
    dateFormatter.timeZone = [NSTimeZone timeZoneForSecondsFromGMT:0];
    dateFormatter.dateFormat = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'SSS'Z'";
  });
  return [date isKindOfClass:[NSDate class]] ? [dateFormatter stringFromDate:date] : nil;
}

const char * bugsnag_getEventMetaData(const void *event, const char *tab) {
    NSDictionary* sectionDictionary = [((__bridge BugsnagEvent *)event) getMetadataFromSection:@(tab)];
    return getJson(sectionDictionary);
}

void bugsnag_clearEventMetadataWithKey(const void *event, const char *section, const char *key){
   [((__bridge BugsnagEvent *)event) clearMetadataFromSection:@(section) withKey:@(key)];
}

void bugsnag_clearEventMetadataSection(const void *event, const char *section){
   [((__bridge BugsnagEvent *)event) clearMetadataFromSection:@(section)];
}

void bugsnag_setEventMetadata(const void *event, const char *tab, const char *metadataJson) {
  
    if (tab == NULL || metadataJson == NULL)
    {
        return;
    }
    [((__bridge BugsnagEvent *)event) addMetadata:getDictionaryFromMetadataJson(metadataJson) toSection:@(tab)];
}


BugsnagUser * bugsnag_getUserFromSession(const void *session){
    return ((__bridge BugsnagSession *)session).user;
}

void bugsnag_setUserFromSession(const void *session, char *userId, char *userEmail, char *userName){
    [((__bridge BugsnagSession *)session) setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
            withEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]
             andName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]];
}

BugsnagUser * bugsnag_getUserFromEvent(const void *event){
    return ((__bridge BugsnagEvent *)event).user;
}

void bugsnag_setUserFromEvent(const void *event, char *userId, char *userEmail, char *userName){
    [((__bridge BugsnagEvent *)event) setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
            withEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]
             andName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]];
}

void bugsnag_getThreadsFromEvent(const void *event,const void *instance, void (*callback)(const void *instance, void *threads[], int threads_size)) {
    NSArray * theThreads = ((__bridge BugsnagEvent *)event).threads;
    void **c_array = (void **) malloc(sizeof(void *) * ((size_t)theThreads.count));
    for (NSUInteger i = 0; i < (NSUInteger)theThreads.count; i++) {
       c_array[i] = (__bridge void *)theThreads[i];
    }
    callback(instance, c_array, [theThreads count]);
    free(c_array);
}


void bugsnag_setEventSeverity(const void *event, const char *severity){   
    if (strcmp(severity, "error") == 0)
    {
        ((__bridge BugsnagEvent *)event).severity = BSGSeverityError;
    }
    else if (strcmp(severity, "warning") == 0)
    {
        ((__bridge BugsnagEvent *)event).severity = BSGSeverityWarning;
    }
    else
    {
        ((__bridge BugsnagEvent *)event).severity = BSGSeverityInfo;
    }     
}

const char * bugsnag_getSeverityFromEvent(const void *event){   
    BSGSeverity theSeverity = ((__bridge BugsnagEvent *)event).severity;
    if(theSeverity == BSGSeverityError)
    {
        return strdup("error");
    }
    if(theSeverity == BSGSeverityWarning)
    {
        return strdup("warning");
    }
    return strdup("info"); 
}


void bugsnag_getStackframesFromError(const void *error,const void *instance, void (*callback)(const void *instance, void *stackframes[], int stackframes_size)) {

    NSArray * theStackframes = ((__bridge BugsnagError *)error).stacktrace;
    void **c_array = (void **) malloc(sizeof(void *) * ((size_t)theStackframes.count));
    for (NSUInteger i = 0; i < (NSUInteger)theStackframes.count; i++) {
       c_array[i] = (__bridge void *)theStackframes[i];
    }
    callback(instance, c_array, [theStackframes count]);
    free(c_array);
}

void bugsnag_getStackframesFromThread(const void *thread,const void *instance, void (*callback)(const void *instance, void *stackframes[], int stackframes_size)) {

    NSArray * theStackframes = ((__bridge BugsnagThread *)thread).stacktrace;
    void **c_array = (void **) malloc(sizeof(void *) * ((size_t)theStackframes.count));
    for (NSUInteger i = 0; i < (NSUInteger)theStackframes.count; i++) {
       c_array[i] = (__bridge void *)theStackframes[i];
    }
    callback(instance, c_array, [theStackframes count]);
    free(c_array);
}

void bugsnag_getErrorsFromEvent(const void *event,const void *instance, void (*callback)(const void *instance, void *errors[], int errors_size)) {

    NSArray * theErrors = ((__bridge BugsnagEvent *)event).errors;
    void **c_array = (void **) malloc(sizeof(void *) * ((size_t)theErrors.count));
    for (NSUInteger i = 0; i < (NSUInteger)theErrors.count; i++) {
       c_array[i] = (__bridge void *)theErrors[i];
    }
    callback(instance, c_array, [theErrors count]);
    free(c_array);
}

void bugsnag_getBreadcrumbsFromEvent(const void *event,const void *instance, void (*callback)(const void *instance, void *breadcrumbs[], int breadcrumbs_size)) {

    NSArray * theBreadcrumbs = ((__bridge BugsnagEvent *)event).breadcrumbs;
    void **c_array = (void **) malloc(sizeof(void *) * ((size_t)theBreadcrumbs.count));
    for (NSUInteger i = 0; i < (NSUInteger)theBreadcrumbs.count; i++) {
       c_array[i] = (__bridge void *)theBreadcrumbs[i];
    }
    callback(instance, c_array, [theBreadcrumbs count]);
    free(c_array);
}

const char * bugsnag_getFeatureFlagsFromEvent(BugsnagEvent *event) {
    NSMutableArray *array = [NSMutableArray array];
    for (BugsnagFeatureFlag *flag in event.featureFlags) {
        [array addObject:[NSDictionary dictionaryWithObjectsAndKeys:
                          flag.name, @"featureFlag",
                          flag.variant, @"variant",
                          nil]];
    }
    return getJson(array);
}

const char * bugsnag_getBreadcrumbMetadata(const void *breadcrumb) {
    NSDictionary* sectionDictionary = ((__bridge BugsnagBreadcrumb *)breadcrumb).metadata;
    return getJson(sectionDictionary);
}

void bugsnag_setBreadcrumbMetadata(const void *breadcrumb, const char *jsonString) {
  ((__bridge BugsnagBreadcrumb *)breadcrumb).metadata = jsonString  ? getDictionaryFromMetadataJson(jsonString) : nil;
}

const char * bugsnag_getBreadcrumbType(const void *breadcrumb){
    return strdup([BSGBreadcrumbTypeValue(((__bridge BugsnagBreadcrumb *)breadcrumb).type) UTF8String]);    
}

void bugsnag_setBreadcrumbType(const void *breadcrumb, char * type){
    ((__bridge BugsnagBreadcrumb *)breadcrumb).type = BSGBreadcrumbTypeFromString(@(type));
}

const char * bugsnag_getValueAsString(const void *object, char *key) {
    id value = [(__bridge id)object valueForKey:@(key)];
    if ([value isKindOfClass:[NSString class]]) {
        return strdup([value UTF8String]);
    } else if ([value respondsToSelector:@selector(stringValue)]) {
        return strdup([[value stringValue] UTF8String]);
    } else {
        return NULL;
    }
}

void bugsnag_setNumberValue(const void *object, char * key, const char * value){       
    NSNumberFormatter *f = [[NSNumberFormatter alloc] init];
    f.numberStyle = NSNumberFormatterDecimalStyle;
    NSNumber *myNumber = [f numberFromString:@(value)];
    [(__bridge id)object setValue:myNumber forKey:@(key)];
}


    
double bugsnag_getTimestampFromDateInObject(const void *object, char * key){
    NSDate *value = (NSDate *)[(__bridge id)object valueForKey:@(key)];
    if(value != NULL && [value isKindOfClass:[NSDate class]])
    {
        return [value timeIntervalSince1970];
    }
    return -1;
}

void bugsnag_setTimestampFromDateInObject(const void *object, char * key, double timeStamp){       
    if(timeStamp < 0)
    {
        [(__bridge id)object setValue:NULL forKey:@(key)];
    }
    else
    {
        [(__bridge id)object setValue:[NSDate dateWithTimeIntervalSince1970:timeStamp] forKey:@(key)];
    }
}

void bugsnag_setRuntimeVersionsFromDevice(const void *device, const char *versions[], int count){
    
    NSMutableDictionary *versionsDict =  [[NSMutableDictionary alloc] init];       
    for (int i = 0; i < count; i+=2) {

        NSString *key = @(versions[i]);
        NSString *value = @(versions[i+1]);

        versionsDict[key] = value;
    }

    ((__bridge BugsnagDevice *)device).runtimeVersions = versionsDict;

}

const char * bugsnag_getRuntimeVersionsFromDevice(const void *device){
    NSDictionary * versions = ((__bridge BugsnagDevice *)device).runtimeVersions;
    NSMutableString *returnString = [[NSMutableString alloc] initWithString:@""];
    for(id key in versions) {
        NSString *keyString = @([key UTF8String]);
        [returnString appendString:keyString];
        [returnString appendString:@"|"];

        NSString *valueString = @([[versions objectForKey:key] UTF8String]);
        [returnString appendString:valueString];
        [returnString appendString:@"|"];
    }
    return strdup([returnString UTF8String]);
}


void bugsnag_setBoolValue(const void *object, char * key, char * value){
    NSString *nsValue = @(value);
    if([nsValue isEqualToString:@"null"])
    {
        [(__bridge id)object setValue:NULL forKey:@(key)];
    }
    else
    {
        [(__bridge id)object setValue:@([nsValue boolValue]) forKey:@(key)];
    }
}

void bugsnag_setStringValue(const void *object, char * key, char * value)
{
    [(__bridge id)object setValue:value ? @(value) : nil forKey:@(key)];
}

const char * bugsnag_getErrorTypeFromError(const void *error){
    BSGErrorType theType = ((__bridge BugsnagError *)error).type;
    if(theType == BSGErrorTypeCocoa)
    {
        return strdup("cocoa");
    }
    if(theType == BSGErrorTypeC)
    {
        return strdup("c");
    }
    return strdup("");
}

const char * bugsnag_getThreadTypeFromThread(const void *thread){
     BSGThreadType theType = ((__bridge BugsnagThread *)thread).type;
    if(theType == BSGThreadTypeCocoa)
    {
        return strdup("cocoa");
    }
    return strdup("");
}

BugsnagApp * bugsnag_getAppFromSession(const void *session){
    return ((__bridge BugsnagSession *)session).app;
}

BugsnagAppWithState * bugsnag_getAppFromEvent(const void *event){
    return ((__bridge BugsnagEvent *)event).app;
}

BugsnagDevice * bugsnag_getDeviceFromSession(const void *session){
    return ((__bridge BugsnagSession *)session).device;
}

BugsnagDeviceWithState * bugsnag_getDeviceFromEvent(const void *event){
    return ((__bridge BugsnagEvent *)event).device;
}

void bugsnag_registerForSessionCallbacks(const void *configuration, bool (*callback)(void *session)){
    [((__bridge BugsnagConfiguration *)configuration) addOnSessionBlock:^BOOL (BugsnagSession *session) {    
        return callback((__bridge void *)session);
    }];
}

void bugsnag_registerForOnSendCallbacks(const void *configuration, bool (*callback)(void *event)){
    [((__bridge BugsnagConfiguration *)configuration) addOnSendErrorBlock:^BOOL (BugsnagEvent *event) {       
        return callback((__bridge void *)event);
    }];
}

// Called when a C# error is reported in order to keep handledCount and unhandledCount in sync
void bugsnag_registerSession(char *sessionId, long startedAt, int unhandledCount, int handledCount) {
    [Bugsnag.client updateSession:^(BugsnagSession * _Nullable session) {
        session.handledCount = (NSUInteger)handledCount;
        session.unhandledCount = (NSUInteger)unhandledCount;
        return session;
    }];
}

void bugsnag_retrieveCurrentSession(const void *ptr, void (*callback)(const void *instance, const char *sessionId, const char *startedAt, int handled, int unhandled)) {
    BugsnagSession *session = Bugsnag.client.session;
    NSString *startedAt = stringFromDate(session.startedAt);
    callback(ptr, session.id.UTF8String, startedAt.UTF8String, (int)session.handledCount, (int)session.unhandledCount);
}

void bugsnag_markLaunchCompleted() {
  [Bugsnag markLaunchCompleted];
}

void *bugsnag_createConfiguration(char *apiKey) {
    return (void *)CFBridgingRetain([[BugsnagConfiguration alloc] initWithApiKey:@(apiKey)]);
}

void bugsnag_setReleaseStage(const void *configuration, char *releaseStage) {
  NSString *ns_releaseStage = releaseStage == NULL ? nil : [NSString stringWithUTF8String: releaseStage];
  ((__bridge BugsnagConfiguration *)configuration).releaseStage = ns_releaseStage;
}

void bugsnag_addFeatureFlagOnConfig(const void *configuration, char *name, char *variant) {
    NSString *ns_name = name == NULL ? nil : [NSString stringWithUTF8String: name];
    NSString *ns_variant = variant == NULL ? nil : [NSString stringWithUTF8String: variant];
    [(__bridge BugsnagConfiguration *)configuration addFeatureFlagWithName:ns_name variant:ns_variant];
}

void bugsnag_addFeatureFlag(char *name, char *variant) {
    NSString *ns_name = name == NULL ? nil : [NSString stringWithUTF8String: name];
    NSString *ns_variant = variant == NULL ? nil : [NSString stringWithUTF8String: variant];
    [Bugsnag addFeatureFlagWithName:ns_name variant:ns_variant];
}

void bugsnag_clearFeatureFlag(char *name) {
    NSString *ns_name = name == NULL ? nil : [NSString stringWithUTF8String: name];
    [Bugsnag clearFeatureFlagWithName:ns_name];
}

void bugsnag_clearFeatureFlags() {
    [Bugsnag clearFeatureFlags];
}

void bugsnag_addFeatureFlagOnEvent(const void *event, char *name, char *variant) {
    NSString *ns_name = name == NULL ? nil : [NSString stringWithUTF8String: name];
    NSString *ns_variant = variant == NULL ? nil : [NSString stringWithUTF8String: variant];
    [(__bridge BugsnagEvent *)event addFeatureFlagWithName:ns_name variant:ns_variant];
}

void bugsnag_clearFeatureFlagOnEvent(const void *event, char *name) {
    NSString *ns_name = name == NULL ? nil : [NSString stringWithUTF8String: name];
    [(__bridge BugsnagEvent *)event clearFeatureFlagWithName:ns_name];
}

void bugsnag_clearFeatureFlagsOnEvent(const void *event) {
    [(__bridge BugsnagEvent *)event clearFeatureFlags];
}

NSMutableSet * getSetFromStringArray(const char *values[], int valuesCount)
{
    NSMutableSet *theSet = [NSMutableSet new];
    for (int i = 0; i < valuesCount; i++) {
        const char *value = values[i];
        if (value != nil) {
            [theSet addObject: @(value)];
        }
    }
    return theSet;
}

void bugsnag_setNotifyReleaseStages(const void *configuration, const char *releaseStages[], int releaseStagesCount){
  ((__bridge BugsnagConfiguration *)configuration).enabledReleaseStages = getSetFromStringArray(releaseStages,releaseStagesCount);
}

void bugsnag_setAppVersion(const void *configuration, char *appVersion) {
  NSString *ns_appVersion = appVersion == NULL ? nil : [NSString stringWithUTF8String: appVersion];
  ((__bridge BugsnagConfiguration *)configuration).appVersion = ns_appVersion;
}

void bugsnag_setAppHangThresholdMillis(const void *configuration, NSUInteger appHangThresholdMillis) {
    if(appHangThresholdMillis == 0)
    {
        ((__bridge BugsnagConfiguration *)configuration).appHangThresholdMillis = BugsnagAppHangThresholdFatalOnly;
        return;
    }
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

void bugsnag_setMaxStringValueLength(const void *configuration, int maxStringValueLength) {
  ((__bridge BugsnagConfiguration *)configuration).maxStringValueLength = maxStringValueLength;
}

void bugsnag_setMaxPersistedEvents(const void *configuration, int maxPersistedEvents) {
  ((__bridge BugsnagConfiguration *)configuration).maxPersistedEvents = maxPersistedEvents;
}

void bugsnag_setMaxPersistedSessions(const void *configuration, int maxPersistedSessions) {
  ((__bridge BugsnagConfiguration *)configuration).maxPersistedSessions = maxPersistedSessions;
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

void bugsnag_setEnabledTelemetryTypes(const void *configuration, const char *types[], int count){
    if(types == NULL)
    {
        return;
    }

    ((__bridge BugsnagConfiguration *)configuration).telemetry = 0;
    for (int i = 0; i < count; i++) {
        const char *enabledType = types[i];
        if (enabledType != nil) {
            NSString *typeString = [[NSString alloc] initWithUTF8String:enabledType];
            if([typeString isEqualToString:@"InternalErrors"])
            {
                ((__bridge BugsnagConfiguration *)configuration).telemetry |= BSGTelemetryInternalErrors;
            }
            if([typeString isEqualToString:@"Usage"])
            {
                ((__bridge BugsnagConfiguration *)configuration).telemetry |= BSGTelemetryUsage;
            }
        }
    }
}

void bugsnag_setThreadSendPolicy(const void *configuration, char *threadSendPolicy){
    NSString *ns_threadSendPolicy = [[NSString alloc] initWithUTF8String:threadSendPolicy];
    if([ns_threadSendPolicy isEqualToString:@"Always"])
    {
        ((__bridge BugsnagConfiguration *)configuration).sendThreads = BSGThreadSendPolicyAlways;
    }
    if([ns_threadSendPolicy isEqualToString:@"UnhandledOnly"])
    {
        ((__bridge BugsnagConfiguration *)configuration).sendThreads = BSGThreadSendPolicyUnhandledOnly;
    }
    if([ns_threadSendPolicy isEqualToString:@"Never"])
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
    ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.thermalKills = NO;

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
            if([typeString isEqualToString:@"ThermalKills"])
            {
                ((__bridge BugsnagConfiguration *)configuration).enabledErrorTypes.thermalKills = YES;
            }
        }
      }
}

void bugsnag_setDiscardClasses(const void *configuration, const char *classNames[], int count){
  ((__bridge BugsnagConfiguration *)configuration).discardClasses = getSetFromStringArray(classNames, count);
}

void bugsnag_setUserInConfig(const void *configuration, char *userId, char *userEmail, char *userName)
{
    [((__bridge BugsnagConfiguration *)configuration) setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
            withEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]
             andName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]];
}

void bugsnag_setRedactedKeys(const void *configuration, const char *redactedKeys[], int count){
  ((__bridge BugsnagConfiguration *)configuration).redactedKeys = getSetFromStringArray(redactedKeys, count);
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

void bugsnag_setEndpoints(const void *configuration, char *notifyURL, char *sessionsURL) {
  if (notifyURL == NULL || sessionsURL == NULL)
    return;

  NSString *ns_notifyURL = [NSString stringWithUTF8String: notifyURL];
  NSString *ns_sessionsURL = [NSString stringWithUTF8String: sessionsURL];

  ((__bridge BugsnagConfiguration *)configuration).endpoints = [[BugsnagEndpointConfiguration alloc] initWithNotify:ns_notifyURL sessions:ns_sessionsURL];
}

void bugsnag_setMetadata(const char *section, const char *jsonString) {

  if (section == NULL || jsonString == NULL)
  {
    return;
  }

  NSString *tabName = [NSString stringWithUTF8String: section];

 [Bugsnag.client addMetadata:getDictionaryFromMetadataJson(jsonString) toSection:tabName];

}

const char * bugsnag_retrieveMetaData() {
    return getJson([Bugsnag.client metadata].dictionary);
}

void bugsnag_removeMetadata(const void *configuration, const char *tab) {
  if (tab == NULL)
    return;

  NSString *tabName = [NSString stringWithUTF8String:tab];
  [Bugsnag.client clearMetadataFromSection:tabName];
}

void bugsnag_addBreadcrumb(char *message, char *type, char *metadataJson) {
  [Bugsnag leaveBreadcrumbWithMessage:message ? @(message) : @"<empty>"
                             metadata:metadataJson ? getDictionaryFromMetadataJson(metadataJson) : nil
                              andType:BSGBreadcrumbTypeFromString(@(type))];
}

void bugsnag_retrieveBreadcrumbs(const void *managedBreadcrumbs, void (*breadcrumb)(const void *instance, const char *name, const char *timestamp, const char *type, const char *metadataJson)) {
  [Bugsnag.breadcrumbs enumerateObjectsUsingBlock:^(BugsnagBreadcrumb *crumb, __unused NSUInteger index, __unused BOOL *stop){
    const char *message = [crumb.message UTF8String];
    const char *timestamp = stringFromDate(crumb.timestamp).UTF8String;
    const char *type = [BSGBreadcrumbTypeValue(crumb.type) UTF8String];

    NSDictionary *metadata = crumb.metadata;
    breadcrumb(managedBreadcrumbs, message, timestamp, type, getJson(metadata));

  }];
}

void bugsnag_retrieveAppData(const void *appData, void (*callback)(const void *instance, const char *key, const char *value)) {
  BugsnagAppWithState *app = [Bugsnag.client generateAppWithState:BSGGetSystemInfo()];
  callback(appData, "bundleVersion", app.bundleVersion.UTF8String);
  callback(appData, "id", app.id.UTF8String);
  callback(appData, "isLaunching", strdup(app.isLaunching ? "true" : "false"));
  callback(appData, "type", app.type.UTF8String);
  callback(appData, "version", app.version.UTF8String);
}

void bugsnag_retrieveLastRunInfo(const void *lastRuninfo, void (*callback)(const void *instance, bool crashed, bool crashedDuringLaunch, int consecutiveLaunchCrashes)) {

  int consecutiveLaunchCrashes = Bugsnag.lastRunInfo.consecutiveLaunchCrashes;
  bool crashed = Bugsnag.lastRunInfo.crashed;
  bool crashedDuringLaunch = Bugsnag.lastRunInfo.crashedDuringLaunch;

  callback(lastRuninfo, crashed, crashedDuringLaunch, consecutiveLaunchCrashes);

}

void bugsnag_retrieveDeviceData(const void *deviceData, void (*callback)(const void *instance, const char *key, const char *value)) {
  BugsnagDeviceWithState *device = [Bugsnag.client generateDeviceWithState:BSGGetSystemInfo()];
  callback(deviceData, "freeDisk", device.freeDisk.stringValue.UTF8String);
  callback(deviceData, "freeMemory", device.freeMemory.stringValue.UTF8String);
  callback(deviceData, "id", device.id.UTF8String);
  callback(deviceData, "jailbroken", strdup(device.jailbroken ? "1" : "0"));
  callback(deviceData, "locale", device.locale.UTF8String);
  callback(deviceData, "manufacturer", device.manufacturer.UTF8String);
  callback(deviceData, "model", device.model.UTF8String);
  callback(deviceData, "modelNumber", device.modelNumber.UTF8String);
  callback(deviceData, "osBuild", device.runtimeVersions[@"osBuild"].UTF8String);
  callback(deviceData, "osName", device.osName.UTF8String);
  callback(deviceData, "osVersion", device.osVersion.UTF8String);
}

void bugsnag_populateUser(struct bugsnag_user *user) {
  user->user_id = BSGGetDefaultDeviceId().UTF8String;
}

void bugsnag_setUser(char *userId, char *userEmail, char *userName) {
    [Bugsnag setUser:userId == NULL ? nil : [NSString stringWithUTF8String:userId]
            withEmail:userEmail == NULL ? nil : [NSString stringWithUTF8String:userEmail]
             andName:userName == NULL ? nil : [NSString stringWithUTF8String:userName]];
}

void bugsnag_startSession() {
    [Bugsnag startSession];
}

void bugsnag_pauseSession() {
    [Bugsnag pauseSession];
}

bool bugsnag_resumeSession() {
    return [Bugsnag resumeSession];
}
