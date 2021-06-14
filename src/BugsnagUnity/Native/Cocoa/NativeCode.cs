using System;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
    [StructLayout(LayoutKind.Sequential)]
    struct NativeUser
    {
        public IntPtr Id;
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
        internal static extern void bugsnag_retrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

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
        internal static extern void bugsnag_setAppHangs(IntPtr configuration, bool appHangs);

        [DllImport(Import)]
        internal static extern void bugsnag_setContext(IntPtr configuration, string context);

        [DllImport(Import)]
        internal static extern void bugsnag_setMaxBreadcrumbs(IntPtr configuration, int maxBreadcrumbs);

        [DllImport(Import)]
        internal static extern void bugsnag_setContextConfig(IntPtr configuration, string context);

        [DllImport(Import)]
        internal static extern void bugsnag_setAppVersion(IntPtr configuration, string appVersion);

        [DllImport(Import)]
        internal static extern void bugsnag_setNotifyUrl(IntPtr configuration, string endpoint);

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
        internal static extern void bugsnag_registerSession(string id, long startedAt, int unhandledCount, int handledCount);
    }
}
