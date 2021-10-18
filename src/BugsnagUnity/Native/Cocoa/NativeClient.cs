using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public Configuration Configuration { get; }
        public IBreadcrumbs Breadcrumbs { get; }
        public IDelivery Delivery { get; }
        private static Session _nativeSession;
        IntPtr NativeConfiguration { get; }

        public NativeClient(Configuration configuration)
        {
            Configuration = configuration;
            NativeConfiguration = CreateNativeConfig(configuration);
            NativeCode.bugsnag_startBugsnagWithConfiguration(NativeConfiguration, NotifierInfo.NotifierVersion);
            Delivery = new Delivery();
            Breadcrumbs = new Breadcrumbs();
        }

        /**
         * Transforms an IConfiguration C# object into a Cocoa Configuration object.
         */
        IntPtr CreateNativeConfig(Configuration config)
        {
            IntPtr obj = NativeCode.bugsnag_createConfiguration(config.ApiKey);
            NativeCode.bugsnag_setAutoNotifyConfig(obj, config.AutoDetectErrors);
            NativeCode.bugsnag_setReleaseStage(obj, config.ReleaseStage);
            NativeCode.bugsnag_setAppVersion(obj, config.AppVersion);
            NativeCode.bugsnag_setEndpoints(obj, config.Endpoints.Notify.ToString(), config.Endpoints.Session.ToString());
            NativeCode.bugsnag_setMaxBreadcrumbs(obj, config.MaximumBreadcrumbs);
            NativeCode.bugsnag_setBundleVersion(obj, config.BundleVersion);
            NativeCode.bugsnag_setAppType(obj, GetAppType(config));
            NativeCode.bugsnag_setPersistUser(obj,config.PersistUser);
            NativeCode.bugsnag_setMaxPersistedEvents(obj, config.MaxPersistedEvents);
            NativeCode.bugsnag_setThreadSendPolicy(obj, Enum.GetName(typeof(ThreadSendPolicy), config.SendThreads));
            NativeCode.bugsnag_setAutoTrackSessions(obj, config.AutoTrackSessions);
            NativeCode.bugsnag_setLaunchDurationMillis(obj, (ulong)config.LaunchDurationMillis);
            NativeCode.bugsnag_setSendLaunchCrashesSynchronously(obj,config.SendLaunchCrashesSynchronously);
            NativeCode.bugsnag_registerForOnSendCallbacks(obj, HandleOnSendCallbacks);
            NativeCode.bugsnag_registerForSessionCallbacks(obj, HandleSessionCallbacks);

            if (config.DiscardClasses != null && config.DiscardClasses.Length > 0)
            {
                NativeCode.bugsnag_setDiscardClasses(obj, config.DiscardClasses, config.DiscardClasses.Length);
            }
            if (config.RedactedKeys != null && config.RedactedKeys.Length > 0)
            {
                NativeCode.bugsnag_setRedactedKeys(obj, config.RedactedKeys, config.RedactedKeys.Length);
            }
            if (config.AppHangThresholdMillis > 0)
            {
                NativeCode.bugsnag_setAppHangThresholdMillis(obj, config.AppHangThresholdMillis);
            }
            SetEnabledBreadcrumbTypes(obj,config);
            SetEnabledErrorTypes(obj, config);
            if (config.Context != null)
            {
                NativeCode.bugsnag_setContextConfig(obj, config.Context);
            }
            var releaseStages = config.EnabledReleaseStages;
            if (releaseStages != null && releaseStages.Length > 0)
            {
                NativeCode.bugsnag_setNotifyReleaseStages(obj, releaseStages, releaseStages.Length);
            }
            return obj;
        }

        [MonoPInvokeCallback(typeof(Func<string, bool>))]
        static bool HandleOnSendCallbacks(string test)
        {
            return true;
        }

        [MonoPInvokeCallback(typeof(Func<IntPtr, bool>))]
        static bool HandleSessionCallbacks(IntPtr nativeSession)
        {
            var wrapper = new NativeSession(nativeSession);
            UnityEngine.Debug.Log("RICH Session ID: " + wrapper.GetId());
            return true;
        }

        private string GetAppType(Configuration config)
        {
            if (!string.IsNullOrEmpty(config.AppType))
            {
                return config.AppType;
            }
            switch (UnityEngine.Application.platform)
            {
                case UnityEngine.RuntimePlatform.OSXPlayer:
                    return "MacOS";
                case UnityEngine.RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case UnityEngine.RuntimePlatform.tvOS:
                    return "tvOS";
            }
            return string.Empty;
        }

        private void SetEnabledBreadcrumbTypes(IntPtr obj, Configuration config)
        {
            if (config.EnabledBreadcrumbTypes == null)
            {
                NativeCode.bugsnag_setEnabledBreadcrumbTypes(obj, null, 0);
                return;
            }
            var enabledTypes = new List<string>();
            foreach (var enabledType in config.EnabledBreadcrumbTypes)
            {
                var typeName = Enum.GetName(typeof(BreadcrumbType), enabledType);
                enabledTypes.Add(typeName);
            }
            NativeCode.bugsnag_setEnabledBreadcrumbTypes(obj, enabledTypes.ToArray(),enabledTypes.Count);
        }

        private void SetEnabledErrorTypes(IntPtr obj, Configuration config)
        {
            var enabledTypes = new List<string>();
            if (config.IsErrorTypeEnabled(ErrorTypes.AppHangs))
            {
                enabledTypes.Add("AppHangs");
            }
            if (config.IsErrorTypeEnabled(ErrorTypes.NativeCrashes))
            {
                enabledTypes.Add("UnhandledExceptions");
            }
            if (config.IsErrorTypeEnabled(ErrorTypes.NativeCrashes))
            {
                enabledTypes.Add("Signals");
            }
            if (config.IsErrorTypeEnabled(ErrorTypes.NativeCrashes))
            {
                enabledTypes.Add("CppExceptions");
            }
            if (config.IsErrorTypeEnabled(ErrorTypes.NativeCrashes))
            {
                enabledTypes.Add("MachExceptions");
            }
            if (config.IsErrorTypeEnabled(ErrorTypes.OOMs))
            {
                enabledTypes.Add("OOMs");
            }

            NativeCode.bugsnag_setEnabledErrorTypes(obj, enabledTypes.ToArray(), enabledTypes.Count);
        }

        public void PopulateApp(App app)
        {
            GCHandle handle = GCHandle.Alloc(app);

            try
            {
                NativeCode.bugsnag_retrieveAppData(GCHandle.ToIntPtr(handle), PopulateAppData);
            }
            finally
            {
                if (handle != null)
                {
                    handle.Free();
                }
            }
        }

        public void PopulateAppWithState(AppWithState app)
        {
            PopulateApp(app);
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
        static void PopulateAppData(IntPtr instance, string key, string value)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is App app)
            {
                app.Add(key, value);
            }
        }

        public void PopulateDevice(Device device)
        {
            var handle = GCHandle.Alloc(device);

            try
            {
                NativeCode.bugsnag_retrieveDeviceData(GCHandle.ToIntPtr(handle), PopulateDeviceData);
            }
            finally
            {
                handle.Free();
            }
        }

        public void PopulateDeviceWithState(DeviceWithState device)
        {
            PopulateDevice(device);
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, string, string>))]
        static void PopulateDeviceData(IntPtr instance, string key, string value)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Device device)
            {
                switch (key)
                {
                    case "jailbroken":
                        switch (value)
                        {
                            case "0":
                                device.Add(key, false);
                                break;
                            case "1":
                                device.Add(key, true);
                                break;
                            default:
                                device.Add(key, value);
                                break;
                        }
                        break;
                    case "osBuild": // add to nested runtimeVersions dictionary
                        device.RuntimeVersions.AddToPayload(key, value);
                        break;
                    default:
                        device.Add(key, value);
                        break;
                }
            }
        }

        public void PopulateUser(User user)
        {
            var nativeUser = new NativeUser();

            NativeCode.bugsnag_populateUser(ref nativeUser);

            user.Id = Marshal.PtrToStringAuto(nativeUser.Id);
        }

        public void SetMetadata(string section, Dictionary<string, object> metadataSection)
        {

            var index = 0;
            var count = 0;
            if (metadataSection != null)
            {
                var metadata = new string[metadataSection.Count * 2];

                foreach (var data in metadataSection)
                {



                    if (data.Key != null)
                    {
                        metadata[index] = data.Key;
                        metadata[index + 1] = data.Value.ToString();
                        count += 2;
                    }
                    index += 2;
                }
                NativeCode.bugsnag_setMetadata(NativeConfiguration, section, metadata, count);
            }
            else
            {
                NativeCode.bugsnag_removeMetadata(NativeConfiguration, section);
            }
        }

        public void PopulateMetadata(Metadata metadata)
        {
            var handle = GCHandle.Alloc(metadata);
            try
            {
                NativeCode.bugsnag_retrieveMetaData(GCHandle.ToIntPtr(handle), PopulateMetaDataData);
            }
            finally
            {
                handle.Free();
            }
        }

        [MonoPInvokeCallback(typeof(NativeCode.MetadataInformation))]
        static void PopulateMetaDataData(IntPtr instance, string tab, string[] keys, int keysSize, string[] values, int valuesSize)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Metadata metadata)
            {
                var metadataObject = new Dictionary<string, object>();
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    if (key.Equals("simulator"))
                    {
                        value = value.Equals("0") || value.Equals("false") ? "false" : "true";
                    }
                    metadataObject.Add(key, value);
                }
                metadata.AddMetadata(tab, metadataObject);
            }
        }

        public void SetUser(User user)
        {
            NativeCode.bugsnag_setUser(user.Id, user.Name, user.Email);
        }

        public void SetContext(string context)
        {
            NativeCode.bugsnag_setContext(NativeConfiguration, context);
        }

        public void SetAutoDetectErrors(bool autoDetectErrors)
        {
            NativeCode.bugsnag_setAutoNotify(autoDetectErrors);
        }

        public void SetAutoDetectAnrs(bool autoDetectAnrs)
        {
        }

        public void MarkLaunchCompleted()
        {
            NativeCode.bugsnag_markLaunchCompleted();
        }

        public void StartSession()
        {
            NativeCode.bugsnag_startSession();
        }

        public void PauseSession()
        {
            NativeCode.bugsnag_pauseSession();
        }

        public bool ResumeSession()
        {
            return NativeCode.bugsnag_resumeSession();
        }

        public void UpdateSession(Session session)
        {
            if (session != null)
            {
                var startedAt = Convert.ToInt64((session.StartedAt?.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))?.TotalSeconds);
                NativeCode.bugsnag_registerSession(session.Id.ToString(), startedAt, session.UnhandledCount(), session.HandledCount());
            }
        }

        public Session GetCurrentSession()
        {
            var session = new Session();
            var handle = GCHandle.Alloc(session);
            try
            {
                NativeCode.bugsnag_retrieveCurrentSession(GCHandle.ToIntPtr(handle), PopulateSession);
            }
            finally
            {
                handle.Free();
            }
            return _nativeSession;
        }

        [MonoPInvokeCallback(typeof(NativeCode.SessionInformation))]
        static void PopulateSession(IntPtr instance, string sessionId, string startedAt, int handled, int unhandled)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Session)
            {
                if (string.IsNullOrEmpty(sessionId) || sessionId == Guid.Empty.ToString())
                {
                    _nativeSession = null;
                    return;
                }
                var startTime = DateTime.Parse(startedAt);
                _nativeSession = new Session(sessionId, startTime, handled, unhandled);
            }
        }

        public LastRunInfo GetLastRunInfo()
        {
            var lastRunInfo = new LastRunInfo();
            var handle = GCHandle.Alloc(lastRunInfo);
            try
            {
                NativeCode.bugsnag_retrieveLastRunInfo(GCHandle.ToIntPtr(handle), PopulateLastRunInfo);
            }
            finally
            {
                handle.Free();
            }
            return lastRunInfo;
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, bool, bool, int>))]
        static void PopulateLastRunInfo(IntPtr instance, bool crashed, bool crashedDuringLaunch, int consecutiveLaunchCrashes)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is LastRunInfo info)
            {
                info.Crashed = crashed;
                info.CrashedDuringLaunch = crashedDuringLaunch;
                info.ConsecutiveLaunchCrashes = consecutiveLaunchCrashes;
            }
        }

    }
}
