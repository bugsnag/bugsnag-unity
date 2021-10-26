﻿using System;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
    [StructLayout(LayoutKind.Sequential)]
    struct NativeUser
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
        internal static extern void bugsnag_setMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

        [DllImport(Import)]
        internal static extern void bugsnag_removeMetadata(IntPtr configuration, string tab);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveLastRunInfo(IntPtr instance, Action<IntPtr, bool, bool, int> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

        [DllImport(Import)]
        internal static extern void bugsnag_registerForOnSendCallbacks(IntPtr configuration, Func<string,bool> callback);

        [DllImport(Import)]
        internal static extern void bugsnag_registerForSessionCallbacks(IntPtr configuration, Func<IntPtr, bool> callback);

        internal delegate void SessionInformation(IntPtr instance, string sessionId, string startedAt, int handled, int unhandled);
        [DllImport(Import)]
        internal static extern void bugsnag_retrieveCurrentSession(IntPtr instance, SessionInformation callback);

        internal delegate void MetadataInformation(IntPtr instance, string tab, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] string[] keys, int keysSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] string[] values, int valuesSize);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveMetaData(IntPtr instance, MetadataInformation visitor);

        [DllImport(Import)]
        internal static extern void bugsnag_populateUser(ref NativeUser user);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_createConfiguration(string apiKey);

        [DllImport(Import)]
        internal static extern void bugsnag_setReleaseStage(IntPtr configuration, string releaseStage);

        [DllImport(Import)]
        internal static extern void bugsnag_setAutoNotify(bool autoNotify);

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
        internal static extern void bugsnag_addBreadcrumb(string name, string type, string[] metadata, int metadataCount);

        internal delegate void BreadcrumbInformation(IntPtr instance, string name, string timestamp, string type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] string[] keys, int keysSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)] string[] values, int valuesSize);

        [DllImport(Import)]
        internal static extern void bugsnag_retrieveBreadcrumbs(IntPtr instance, BreadcrumbInformation visitor);

        [DllImport(Import)]
        internal static extern void bugsnag_setUser(string id, string name, string email);

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
        internal static extern void bugsnag_populateUserFromSession(IntPtr session, ref NativeUser user);

        [DllImport(Import)]
        internal static extern void bugsnag_setUserFromSession(IntPtr session, string id, string name, string email);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getAppFromSession(IntPtr session);

        [DllImport(Import)]
        internal static extern IntPtr bugsnag_getDeviceFromSession(IntPtr session);

        //APP

        [DllImport(Import)]
        internal static extern string bugsnag_getStringValue(IntPtr @object,string key);

        [DllImport(Import)]
        internal static extern void bugsnag_setStringValue(IntPtr @object, string key, string value);

        [DllImport(Import)]
        internal static extern string bugsnag_getBoolValue(IntPtr @object, string key);

        [DllImport(Import)]
        internal static extern void bugsnag_setBoolValue(IntPtr @object, string key, string value);

        [DllImport(Import)]
        internal static extern string bugsnag_getRuntimeVersionsFromDevice(IntPtr nativeDevice);

        [DllImport(Import)]
        internal static extern void bugsnag_setRuntimeVersionsFromDevice(IntPtr nativeDevice, string[] versions, int count);

        [DllImport(Import)]
        internal static extern double bugsnag_getTimestampFromDateInObject(IntPtr @object, string key);

        [DllImport(Import)]
        internal static extern void bugsnag_setTimestampFromDateInObject(IntPtr @object, string key, double timeStamp);

    }
}


