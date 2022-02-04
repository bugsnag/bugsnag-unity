using System;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
    [StructLayout(LayoutKind.Sequential)]
    struct NativeUserVisitor
    {
        public IntPtr Id;
        public IntPtr Name;
        public IntPtr Email;
    }

    partial class NativeCode
    {
        [DllImport(Import)]
        internal static extern void bugsnag_startBugsnagWithConfiguration(IntPtr configuration, string notifierVersion);

        [DllImport(Import)]
        internal static extern void bugsnag_setMetadata(string section, string jsonString);

        [DllImport(Import)]
        internal static extern void bugsnag_removeMetadata(IntPtr configuration, string tab);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveLastRunInfo(IntPtr instance, Action<IntPtr, bool, bool, int> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_registerForOnSendCallbacks(IntPtr configuration, Func<IntPtr, bool> callback);

        [DllImport(Import)]
        internal static extern void bugsnag_registerForSessionCallbacks(IntPtr configuration, Func<IntPtr, bool> callback);

        internal delegate void SessionInformation(IntPtr instance, string sessionId, string startedAt, int handled, int unhandled);
        [DllImport(Import)]
        internal static extern void bugsnag_retrieveCurrentSession(IntPtr instance, SessionInformation callback);

        [DllImport(Import)]
        internal static extern string bugsnag_retrieveMetaData();

        [DllImport(Import)]
        internal static extern void bugsnag_populateUser(ref NativeUserVisitor user);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_createConfiguration(string apiKey);

        [DllImport(Import)]
        internal static extern void bugsnag_setReleaseStage(IntPtr configuration, string releaseStage);

        [DllImport(Import)]
        internal static extern void bugsnag_addFeatureFlagOnConfig(IntPtr configuration, string name, string variant);

        [DllImport(Import)]
        internal static extern void bugsnag_addFeatureFlag(string name, string variant);

        [DllImport(Import)]
        internal static extern void bugsnag_clearFeatureFlag(string name);

        [DllImport(Import)]
        internal static extern void bugsnag_clearFeatureFlags();

        [DllImport(Import)]
        internal static extern void bugsnag_setAutoNotifyConfig(IntPtr configuration, bool autoNotify);

        [DllImport(Import)]
        internal static extern void bugsnag_setAutoTrackSessions(IntPtr configuration, bool autoTrackSessions);

        [DllImport(Import)]
        internal static extern void bugsnag_setPersistUser(IntPtr configuration, bool persistUser);

        [DllImport(Import)]
        internal static extern void bugsnag_setSendLaunchCrashesSynchronously(IntPtr configuration, bool sendLaunchCrashesSynchronously);

        [DllImport(Import)]
        internal static extern void bugsnag_setContext(IntPtr configuration, string context);

        [DllImport(Import)]
        internal static extern void bugsnag_setMaxBreadcrumbs(IntPtr configuration, int maxBreadcrumbs);

        [DllImport(Import)]
        internal static extern void bugsnag_setLaunchDurationMillis(IntPtr configuration, ulong launchDurationMillis);

        [DllImport(Import)]
        internal static extern void bugsnag_markLaunchCompleted();

        [DllImport(Import)]
        internal static extern void bugsnag_setMaxPersistedEvents(IntPtr configuration, int maxPersistedEvents);

        [DllImport(Import)]
        internal static extern void bugsnag_setAppHangThresholdMillis(IntPtr configuration, ulong appHangThresholdMillis);

        [DllImport(Import)]
        internal static extern void bugsnag_setEnabledBreadcrumbTypes(IntPtr configuration, string[] types, int count);

        [DllImport(Import)]
        internal static extern void bugsnag_setEnabledErrorTypes(IntPtr configuration, string[] types, int count);

        [DllImport(Import)]
        internal static extern void bugsnag_setDiscardClasses(IntPtr configuration, string[] classNames, int count);

        [DllImport(Import)]
        internal static extern void bugsnag_setUserInConfig(IntPtr configuration, string id, string email, string name);

        [DllImport(Import)]
        internal static extern void bugsnag_setRedactedKeys(IntPtr configuration, string[] redactedKeys, int count);

        [DllImport(Import)]
        internal static extern void bugsnag_setContextConfig(IntPtr configuration, string context);

        [DllImport(Import)]
        internal static extern void bugsnag_setAppVersion(IntPtr configuration, string appVersion);

        [DllImport(Import)]
        internal static extern void bugsnag_setBundleVersion(IntPtr configuration, string bundleVersion);

        [DllImport(Import)]
        internal static extern void bugsnag_setThreadSendPolicy(IntPtr configuration, string threadSendPolicy);

        [DllImport(Import)]
        internal static extern void bugsnag_setAppType(IntPtr configuration, string appType);

        [DllImport(Import)]
        internal static extern void bugsnag_setEndpoints(IntPtr configuration, string notifyURL, string sessionsURL);

        internal delegate void NotifyReleaseStageCallback(IntPtr instance, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] string[] releaseStages, long count);

        [DllImport(Import)]
        internal static extern void bugsnag_setNotifyReleaseStages(IntPtr configuration, string[] releaseStages, int count);

        [DllImport(Import)]
        internal static extern void bugsnag_addBreadcrumb(string name, string type, string metadataJson);

        internal delegate void BreadcrumbInformation(IntPtr instance, string name, string timestamp, string type, string metadataJson);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveBreadcrumbs(IntPtr instance, BreadcrumbInformation visitor);

        [DllImport(Import)]
        internal static extern void bugsnag_setUser(string id, string email, string name);

        [DllImport(Import)]
        internal static extern void bugsnag_startSession();

        [DllImport(Import)]
        internal static extern void bugsnag_pauseSession();

        [DllImport(Import)]
        internal static extern bool bugsnag_resumeSession();

        [DllImport(Import)]
        internal static extern void bugsnag_registerSession(string id, long startedAt, int unhandledCount, int handledCount);

        //Callback Getters and setters
               
        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getAppFromSession(IntPtr session);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getAppFromEvent(IntPtr @event);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getDeviceFromSession(IntPtr session);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getDeviceFromEvent(IntPtr @event);

        [DllImport(Import)]
        internal static extern void bugsnag_setStringValue(IntPtr @object, string key, string value);

        [DllImport(Import)]
        internal static extern string bugsnag_getValueAsString(IntPtr @object, string key);

        [DllImport(Import)]
        internal static extern void bugsnag_setNumberValue(IntPtr @object, string key, string value);

        [DllImport(Import)]
        internal static extern void bugsnag_setBoolValue(IntPtr @object, string key, string value);

        [DllImport(Import)]
        internal static extern string bugsnag_getRuntimeVersionsFromDevice(IntPtr nativeDevice);

        [DllImport(Import)]
        internal static extern string bugsnag_getErrorTypeFromError(IntPtr nativeError);

        [DllImport(Import)]
        internal static extern string bugsnag_getThreadTypeFromThread(IntPtr nativeThread);

        [DllImport(Import)]
        internal static extern void bugsnag_setRuntimeVersionsFromDevice(IntPtr nativeDevice, string[] versions, int count);

        [DllImport(Import)]
        internal static extern double bugsnag_getTimestampFromDateInObject(IntPtr @object, string key);

        [DllImport(Import)]
        internal static extern void bugsnag_setTimestampFromDateInObject(IntPtr @object, string key, double timeStamp);       

        [DllImport(Import)]
        internal static extern string bugsnag_getBreadcrumbType(IntPtr nativeBreadcrumb);

        [DllImport(Import)]
        internal static extern void bugsnag_setBreadcrumbType(IntPtr nativeBreadcrumb, string type);

        [DllImport(Import)]
        internal static extern void bugsnag_setBreadcrumbMetadata(IntPtr nativeBreadcrumb,  string jsonString);

        [DllImport(Import)]
        internal static extern string bugsnag_getBreadcrumbMetadata(IntPtr nativeBreadcrumb);

        internal delegate void EventBreadcrumbs(IntPtr breadcrumbList,[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] breadcrumbs, int breadcrumbsSize);
        [DllImport(Import)]
        internal static extern void bugsnag_getBreadcrumbsFromEvent(IntPtr @event, IntPtr breadcrumbList, EventBreadcrumbs visitor);

        internal delegate void EventErrors(IntPtr errorList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] errors, int errorsSize);
        [DllImport(Import)]
        internal static extern void bugsnag_getErrorsFromEvent(IntPtr @event, IntPtr errorList, EventErrors visitor);

        internal delegate void ErrorStackframes(IntPtr stackframeList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] stackframes, int stackframesSize);
        [DllImport(Import)]
        internal static extern void bugsnag_getStackframesFromError(IntPtr error, IntPtr stackframeList, ErrorStackframes visitor);

        [DllImport(Import)]
        internal static extern void bugsnag_getStackframesFromThread(IntPtr error, IntPtr stackframeList, ErrorStackframes visitor);

        [DllImport(Import)]
        internal static extern string bugsnag_getSeverityFromEvent(IntPtr nativeEvent);

        [DllImport(Import)]
        internal static extern void bugsnag_setEventSeverity(IntPtr nativeEvent, string severity);

        internal delegate void EventThreads(IntPtr threadsList, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] nativeThreads, int nativeThreadsSize);
        [DllImport(Import)]
        internal static extern void bugsnag_getThreadsFromEvent(IntPtr @event, IntPtr threadsList, EventThreads visitor);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getUserFromEvent(IntPtr @event);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_setUserFromEvent(IntPtr @event, string userId, string userEmail, string userName);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getUserFromSession(IntPtr session);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_setUserFromSession(IntPtr session, string userId, string userEmail, string userName);

        [DllImport(Import)]
        internal static extern void bugsnag_setEventMetadata(IntPtr @event, string tab, string metadataJson);

        [DllImport(Import)]
        internal static extern void bugsnag_clearEventMetadataSection(IntPtr @event, string section);

        [DllImport(Import)]
        internal static extern void bugsnag_clearEventMetadataWithKey(IntPtr @event, string section, string key);

        [DllImport(Import)]
        internal static extern string bugsnag_getEventMetaData(IntPtr @event, string section);

        [DllImport(Import)]
        internal static extern void bugsnag_clearMetadata(string section);

        [DllImport(Import)]
        internal static extern void bugsnag_clearMetadataWithKey(string section, string key);

        [DllImport(Import)]
        internal static extern void bugsnag_addFeatureFlagOnEvent(IntPtr @event, string name, string variant);

        [DllImport(Import)]
        internal static extern void bugsnag_clearFeatureFlagOnEvent(IntPtr @event, string name);

        [DllImport(Import)]
        internal static extern void bugsnag_clearFeatureFlagsOnEvent(IntPtr @event);
    }
}


