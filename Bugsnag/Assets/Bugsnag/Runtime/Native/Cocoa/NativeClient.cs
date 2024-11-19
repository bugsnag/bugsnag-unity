#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BSG_COCOA_DEV

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    class NativeClient : INativeClient
    {
        public Configuration Configuration { get; }
        public IBreadcrumbs Breadcrumbs { get; }
        private static Session _nativeSession;
        IntPtr NativeConfiguration { get; }
        private static NativeClient _instance;
        private bool _registeredForSessionCallbacks;

        private LoadedImages loadedImages = new LoadedImages();

        public NativeClient(Configuration configuration)
        {
            _instance = this;
            Configuration = configuration;
            NativeConfiguration = CreateNativeConfig(configuration);
            NativeCode.bugsnag_startBugsnagWithConfiguration(NativeConfiguration, NotifierInfo.NotifierVersion);
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
            if (!string.IsNullOrEmpty(config.BundleVersion))
            {
                NativeCode.bugsnag_setBundleVersion(obj, config.BundleVersion);
            }
            NativeCode.bugsnag_setAppType(obj, GetAppType(config));
            NativeCode.bugsnag_setPersistUser(obj,config.PersistUser);
            NativeCode.bugsnag_setMaxPersistedEvents(obj, config.MaxPersistedEvents);
            NativeCode.bugsnag_setMaxPersistedSessions(obj, config.MaxPersistedSessions);
            NativeCode.bugsnag_setThreadSendPolicy(obj, Enum.GetName(typeof(ThreadSendPolicy), config.SendThreads));
            NativeCode.bugsnag_setAutoTrackSessions(obj, config.AutoTrackSessions);
            NativeCode.bugsnag_setLaunchDurationMillis(obj, (ulong)config.LaunchDurationMillis);
            NativeCode.bugsnag_setSendLaunchCrashesSynchronously(obj,config.SendLaunchCrashesSynchronously);

            if (config.GetOnSendErrorCallbacks() != null && config.GetOnSendErrorCallbacks().Count > 0)
            {
                NativeCode.bugsnag_registerForOnSendCallbacks(obj, HandleOnSendCallbacks);
            }

            if (config.GetOnSessionCallbacks() != null && config.GetOnSessionCallbacks().Count > 0)
            {
                _registeredForSessionCallbacks = true;
                NativeCode.bugsnag_registerForSessionCallbacks(obj, HandleSessionCallbacks);
            }

            


            NativeCode.bugsnag_setAppHangThresholdMillis(obj, config.AppHangThresholdMillis);
            NativeCode.bugsnag_setMaxStringValueLength(obj, config.MaxStringValueLength);
            AddFeatureFlagsToConfig(obj,config);
            if (config.GetUser() != null)
            {
                var user = config.GetUser();
                NativeCode.bugsnag_setUserInConfig(obj, user.Id, user.Email, user.Name);
            }
            if (config.DiscardClasses != null && config.DiscardClasses.Count > 0)
            {
                var patternsAsStrings = new string[config.DiscardClasses.Count];
                foreach (var key in config.DiscardClasses)
                {
                    patternsAsStrings[config.DiscardClasses.IndexOf(key)] = key.ToString();
                }
                NativeCode.bugsnag_setDiscardClasses(obj, patternsAsStrings, patternsAsStrings.Length);
            }
            if (config.RedactedKeys != null && config.RedactedKeys.Count > 0)
            {
                var patternsAsStrings = new string[config.RedactedKeys.Count];
                foreach (var key in config.RedactedKeys)
                {
                    patternsAsStrings[config.RedactedKeys.IndexOf(key)] = key.ToString();
                }
                NativeCode.bugsnag_setRedactedKeys(obj, patternsAsStrings, patternsAsStrings.Length);
            }

            SetEnabledTelemetryTypes(obj,config);
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

        private void AddFeatureFlagsToConfig(IntPtr obj, Configuration config)
        {
            if (config.FeatureFlags == null || config.FeatureFlags.Count == 0)
            {
                return;
            }
            foreach (DictionaryEntry entry in config.FeatureFlags)
            {
                NativeCode.bugsnag_addFeatureFlagOnConfig(obj, (string)entry.Key, (string)entry.Value);
            }
        }

        [MonoPInvokeCallback(typeof(Func<IntPtr, bool>))]
        static bool HandleOnSendCallbacks(IntPtr nativeEvent)
        {
            var callbacks = _instance.Configuration.GetOnSendErrorCallbacks();
            if (callbacks.Count > 0)
            {
                var nativeEventWrapper = new NativeEvent(nativeEvent);

                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (!callback.Invoke(nativeEventWrapper))
                        {
                            return false;
                        }
                    }
                    catch {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
            }
            return true;
        }

        [MonoPInvokeCallback(typeof(Func<IntPtr, bool>))]
        static bool HandleSessionCallbacks(IntPtr nativeSession)
        {
            var callbacks = _instance.Configuration.GetOnSessionCallbacks();
            if (callbacks.Count > 0)
            {
                var nativeSessionWrapper = new NativeSession(nativeSession);

                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (!callback.Invoke(nativeSessionWrapper))
                        {
                            return false;
                        }
                    }
                    catch {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
            }
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

        private void SetEnabledTelemetryTypes(IntPtr obj, Configuration config)
        {
            if (config.Telemetry == null)
            {
                NativeCode.bugsnag_setEnabledTelemetryTypes(obj, null, 0);
                return;
            }
            var enabledTypes = new List<string>();
            foreach (var enabledType in config.Telemetry)
            {
                var typeName = Enum.GetName(typeof(TelemetryType), enabledType);
                enabledTypes.Add(typeName);
            }
            NativeCode.bugsnag_setEnabledTelemetryTypes(obj, enabledTypes.ToArray(), enabledTypes.Count);
        }

        private void SetEnabledErrorTypes(IntPtr obj, Configuration config)
        {
            var enabledTypes = new List<string>();
           
            if (config.EnabledErrorTypes.AppHangs)
            {
                enabledTypes.Add("AppHangs");
            }
            if (config.EnabledErrorTypes.ThermalKills)
            {
                enabledTypes.Add("ThermalKills");
            }
            if (config.EnabledErrorTypes.Crashes)
            {
                enabledTypes.Add("UnhandledExceptions");
                enabledTypes.Add("Signals");
                enabledTypes.Add("CppExceptions");
                enabledTypes.Add("MachExceptions");
            }
            if (config.EnabledErrorTypes.OOMs)
            {
                enabledTypes.Add("OOMs");
            }
            
            NativeCode.bugsnag_setEnabledErrorTypes(obj, enabledTypes.ToArray(), enabledTypes.Count);
        }

        public void PopulateApp(App app)
        {
            var result = NativeCode.bugsnag_retrieveAppData();
            var dictionary = ((JsonObject)SimpleJson.DeserializeObject(result)).GetDictionary();
            foreach (var pair in dictionary)
            {
                if (pair.Key == "isLaunching")
                {
                    if (pair.Value != null)
                    {
                        var stringValue = (pair.Value as string).ToLower();
                        app.Add(pair.Key, stringValue == "true");
                    }
                }
                else
                {
                    app.Add(pair.Key, pair.Value);
                }
            }
        }

        public void PopulateAppWithState(AppWithState app)
        {
            PopulateApp(app);
        }     

        public void PopulateDevice(Device device)
        {
            var result = NativeCode.bugsnag_retrieveDeviceData();
            var dictionary = ((JsonObject)SimpleJson.DeserializeObject(result)).GetDictionary();
            foreach (var pair in dictionary)
            {
                if (pair.Key == "jailbroken")
                {
                    if (pair.Value != null)
                    {
                        var stringValue = (pair.Value as string).ToLower();
                        device.Add(pair.Key, stringValue == "true");
                    }
                }
                else if (pair.Key == "osBuild")
                {
                    device.RuntimeVersions.AddToPayload(pair.Key, pair.Value);
                }
                else
                {
                    device.Add(pair.Key, pair.Value);
                }
            }
        }

        public void PopulateDeviceWithState(DeviceWithState device)
        {
            PopulateDevice(device);
        }

        public void PopulateUser(User user)
        {
            var nativeUser = new NativeUserVisitor();

            NativeCode.bugsnag_populateUser(ref nativeUser);

            user.Id = Marshal.PtrToStringAuto(nativeUser.Id);
        }

        public void SetUser(User user)
        {
            NativeCode.bugsnag_setUser(user.Id, user.Email, user.Name);
        }

        public void SetContext(string context)
        {
            NativeCode.bugsnag_setContext(NativeConfiguration, context);
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
                var startTime = DateTimeOffset.Parse(startedAt);
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

        public void ClearNativeMetadata(string section)
        {
            NativeCode.bugsnag_clearMetadata(section);
        }

        public void ClearNativeMetadata(string section, string key)
        {
            NativeCode.bugsnag_clearMetadataWithKey(section,key);
        }

        public void AddNativeMetadata(string section, IDictionary<string, object> data)
        {
            if (data != null)
            {
                using (var stream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
                {
                    SimpleJson.SerializeObject(data, writer);
                    writer.Flush();
                    stream.Position = 0;
                    var jsonString = reader.ReadToEnd();
                    NativeCode.bugsnag_setMetadata(section, jsonString);
                }
            }
            else
            {
                NativeCode.bugsnag_removeMetadata(NativeConfiguration, section);
            }
        }

        public IDictionary<string, object> GetNativeMetadata()
        {
            var result = NativeCode.bugsnag_retrieveMetaData();
            var dictionary = ((JsonObject)SimpleJson.DeserializeObject(result)).GetDictionary();
            return dictionary;
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            NativeCode.bugsnag_addFeatureFlag(name, variant);
        }

        public void AddFeatureFlags(FeatureFlag[] featureFlags)
        {
            foreach (var flag in featureFlags)
            {
                AddFeatureFlag(flag.Name, flag.Variant);
            }
        }

        public void ClearFeatureFlag(string name)
        {
            NativeCode.bugsnag_clearFeatureFlag(name);
        }

        public void ClearFeatureFlags()
        {
            NativeCode.bugsnag_clearFeatureFlags();
        }

        public bool ShouldAttemptDelivery()
        {
            return true;
        }

        public void RegisterForOnSessionCallbacks()
        {
            if (_registeredForSessionCallbacks)
            {
                return;
            }
            _registeredForSessionCallbacks = true;
            NativeCode.bugsnag_registerForSessionCallbacksAfterStart(HandleSessionCallbacks);
        }

        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(int gchandle);

        [DllImport("__Internal")]
        private static extern void il2cpp_free(IntPtr ptr);

        [DllImport("__Internal")]
        private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out IntPtr imageName);

        #nullable enable
        private static string? ExtractString(IntPtr pString)
        {
            return (pString == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(pString);
        }

        private StackTraceLine[] ToStackFrames(System.Exception exception, IntPtr[] nativeAddresses)
        {
            var unityTrace = new PayloadStackTrace(exception.StackTrace).StackTraceLines;
            var length = nativeAddresses.Length < unityTrace.Length ? nativeAddresses.Length : unityTrace.Length;
            var stackFrames = new StackTraceLine[length];
            for (int i = 0; i < length; i++)
            {
                var method = unityTrace[i].Method;
                var address = (UInt64)nativeAddresses[i].ToInt64();
                var image = loadedImages.FindImageAtAddress(address);

                var trace = new StackTraceLine();
                trace.FrameAddress = address.ToString();
                trace.Method = method.ToString();
                if (image != null)
                {
                    if (address < image.LoadAddress)
                    {
                        // It's a relative address
                        trace.FrameAddress = (address + image.LoadAddress).ToString();
                    }
                    trace.MachoFile = image.FileName;
                    trace.MachoLoadAddress = image.LoadAddress.ToString();
                    trace.MachoUuid = image.Uuid;
                    trace.InProject = image.IsMainImage;
                }
                stackFrames[i] = trace;
            }
            return stackFrames;
        }

        public StackTraceLine[] ToStackFrames(System.Exception exception)
        {
            var notFound = new StackTraceLine[0];

            if (exception == null)
            {
                return notFound;
            }

#if ENABLE_IL2CPP
            var hException = GCHandle.Alloc(exception);
            var pNativeAddresses = IntPtr.Zero;
            var pImageUuid = IntPtr.Zero;
            var pImageName = IntPtr.Zero;
            try
            {
                if (hException == null)
                {
                    return notFound;
                }

                var pException = il2cpp_gchandle_get_target(GCHandle.ToIntPtr(hException).ToInt32());
                if (pException == IntPtr.Zero)
                {
                    return notFound;
                }

                var frameCount = 0;
                string? mainImageFileName = null;

                il2cpp_native_stack_trace(pException, out pNativeAddresses, out frameCount, out pImageUuid, out pImageName);
                if (pNativeAddresses == IntPtr.Zero)
                {
                    return notFound;
                }

                mainImageFileName = ExtractString(pImageName);
                var nativeAddresses = new IntPtr[frameCount];
                Marshal.Copy(pNativeAddresses, nativeAddresses, 0, frameCount);

                loadedImages.Refresh(mainImageFileName);
                return ToStackFrames(exception, nativeAddresses);
            }
            finally
            {
                if (pImageUuid != IntPtr.Zero)
                {
                    il2cpp_free(pImageUuid);
                }
                if (pImageName != IntPtr.Zero)
                {
                    il2cpp_free(pImageName);
                }
                if (pNativeAddresses != IntPtr.Zero)
                {
                    il2cpp_free(pNativeAddresses);
                }
                if (hException != null)
                {
                    hException.Free();
                }
            }
#else
            return notFound;
#endif
        }
    }
}
#endif