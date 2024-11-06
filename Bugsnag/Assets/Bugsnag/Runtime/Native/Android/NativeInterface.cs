#if (UNITY_ANDROID && !UNITY_EDITOR) || BGS_ANDROID_DEV

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Threading;
using System.Text;

namespace BugsnagUnity
{
    internal class DisposableContainer : IDisposable
    {
        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void Dispose()
        {
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                if (_disposables[i] != null)
                {
                    _disposables[i].Dispose();
                }
            }
        }
    }

    class NativeInterface
    {
        private IntPtr BugsnagNativeInterface;
        private IntPtr BugsnagUnityClass;
        // Cache of classes used:
        private IntPtr LastRunInfoClass;
        private IntPtr BreadcrumbClass;
        private IntPtr BreadcrumbTypeClass;
        private IntPtr CollectionClass;
        private IntPtr IteratorClass;
        private IntPtr ListClass;
        private IntPtr MapClass;
        private IntPtr BooleanClass;
        private IntPtr IntClass;
        private IntPtr LongClass;
        private IntPtr DoubleClass;
        private IntPtr FloatClass;


        private IntPtr DateClass;
        private IntPtr DateUtilsClass;
        private IntPtr MapEntryClass;
        private IntPtr SetClass;
        private IntPtr StringClass;
        private IntPtr SessionClass;
        private IntPtr ClientClass;

        // Cache of methods used:
        private IntPtr BreadcrumbGetMessage;
        private IntPtr BreadcrumbGetMetadata;
        private IntPtr BreadcrumbGetTimestamp;
        private IntPtr BreadcrumbGetType;
        private IntPtr ClassIsArray;
        private IntPtr CollectionIterator;
        private IntPtr IteratorHasNext;
        private IntPtr IteratorNext;
        private IntPtr MapEntryGetKey;
        private IntPtr MapEntryGetValue;
        private IntPtr MapEntrySet;
        private IntPtr ObjectGetClass;
        private IntPtr ObjectToString;
        private IntPtr BooleanValueMethod;
        private IntPtr IntValueMethod;
        private IntPtr LongValueMethod;
        private IntPtr DoubleValueMethod;
        private IntPtr FloatValueMethod;

        private IntPtr ToIso8601;
        private IntPtr AddFeatureFlagMethod;
        private IntPtr ClearFeatureFlagMethod;
        private IntPtr ClearFeatureFlagsMethod;




        private Configuration _configuration;

        private bool CanRunOnBackgroundThread;

        private static bool Unity2019OrNewer;
        private Thread MainThread;

        private bool _registeredForSessionCallbacks;

        private class OnSessionCallback : AndroidJavaProxy
        {

            private Configuration _config;

            public OnSessionCallback(Configuration config) : base("com.bugsnag.android.OnSessionCallback")
            {
                _config = config;
            }
            public bool onSession(AndroidJavaObject session)
            {

                var wrapper = new NativeSession(session);
                foreach (var callback in _config.GetOnSessionCallbacks())
                {
                    try
                    {
                        if (!callback.Invoke(wrapper))
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        // If the callback causes an exception, ignore it and execute the next one
                    }

                }

                return true;
            }
        }

        private class OnSendErrorCallback : AndroidJavaProxy
        {
            private Configuration _config;

            public OnSendErrorCallback(Configuration config) : base("com.bugsnag.android.OnSendCallback")
            {
                _config = config;
            }
            public bool onSend(AndroidJavaObject @event)
            {
                var wrapper = new NativeEvent(@event);
                foreach (var callback in _config.GetOnSendErrorCallbacks())
                {
                    try
                    {
                        if (!callback.Invoke(wrapper))
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
                return true;
            }
        }

        public NativeInterface(Configuration cfg)
        {
            _configuration = cfg;
            AndroidJavaObject config = CreateNativeConfig(cfg);
            ConfigureNotifierInfo(config);
            Unity2019OrNewer = IsUnity2019OrNewer();
            MainThread = Thread.CurrentThread;
            using (AndroidJavaClass system = new AndroidJavaClass("java.lang.System"))
            {
                string arch = system.CallStatic<string>("getProperty", "os.arch");
                CanRunOnBackgroundThread = (arch != "x86" && arch != "i686" && arch != "x86_64");
            }
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject client = new AndroidJavaObject("com.bugsnag.android.Client", context, config))
            {
                // lookup the NativeInterface class and set the client to the local object.
                // all subsequent communication should go through the NativeInterface.
                IntPtr nativeInterfaceRef = AndroidJNI.FindClass("com/bugsnag/android/NativeInterface");
                BugsnagNativeInterface = AndroidJNI.NewGlobalRef(nativeInterfaceRef);
                AndroidJNI.DeleteLocalRef(nativeInterfaceRef);

                IntPtr setClient = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, "setClient", "(Lcom/bugsnag/android/Client;)V");

                object[] args = new object[] { client };
                jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
                AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, setClient, jargs);
                AndroidJNIHelper.DeleteJNIArgArray(args, jargs);

                // Cache JNI refs which will be used to load report data later in the
                // app lifecycle to avoid repeated lookups
                IntPtr unityRef = AndroidJNI.FindClass("com/bugsnag/android/unity/BugsnagUnity");
                BugsnagUnityClass = AndroidJNI.NewGlobalRef(unityRef);
                AndroidJNI.DeleteLocalRef(unityRef);

                IntPtr crumbRef = AndroidJNI.FindClass("com/bugsnag/android/Breadcrumb");
                BreadcrumbClass = AndroidJNI.NewGlobalRef(crumbRef);
                AndroidJNI.DeleteLocalRef(crumbRef);

                IntPtr lastRunInfoRef = AndroidJNI.FindClass("com/bugsnag/android/LastRunInfo");
                LastRunInfoClass = AndroidJNI.NewGlobalRef(lastRunInfoRef);
                AndroidJNI.DeleteLocalRef(lastRunInfoRef);

                IntPtr crumbTypeRef = AndroidJNI.FindClass("com/bugsnag/android/BreadcrumbType");
                BreadcrumbTypeClass = AndroidJNI.NewGlobalRef(crumbTypeRef);
                AndroidJNI.DeleteLocalRef(crumbTypeRef);

                IntPtr collectionRef = AndroidJNI.FindClass("java/util/Collection");
                CollectionClass = AndroidJNI.NewGlobalRef(collectionRef);
                AndroidJNI.DeleteLocalRef(collectionRef);

                IntPtr iterRef = AndroidJNI.FindClass("java/util/Iterator");
                IteratorClass = AndroidJNI.NewGlobalRef(iterRef);
                AndroidJNI.DeleteLocalRef(iterRef);

                IntPtr listRef = AndroidJNI.FindClass("java/util/List");
                ListClass = AndroidJNI.NewGlobalRef(listRef);
                AndroidJNI.DeleteLocalRef(listRef);

                IntPtr mapRef = AndroidJNI.FindClass("java/util/Map");
                MapClass = AndroidJNI.NewGlobalRef(mapRef);
                AndroidJNI.DeleteLocalRef(mapRef);

                IntPtr booleanRef = AndroidJNI.FindClass("java/lang/Boolean");
                BooleanClass = AndroidJNI.NewGlobalRef(booleanRef);
                BooleanValueMethod = AndroidJNI.GetMethodID(booleanRef, "booleanValue", "()Z");
                AndroidJNI.DeleteLocalRef(booleanRef);

                IntPtr intRef = AndroidJNI.FindClass("java/lang/Integer");
                IntClass = AndroidJNI.NewGlobalRef(intRef);
                IntValueMethod = AndroidJNI.GetMethodID(intRef, "intValue", "()I");
                AndroidJNI.DeleteLocalRef(intRef);

                IntPtr longRef = AndroidJNI.FindClass("java/lang/Long");
                LongClass = AndroidJNI.NewGlobalRef(longRef);
                LongValueMethod = AndroidJNI.GetMethodID(longRef, "longValue", "()J");
                AndroidJNI.DeleteLocalRef(longRef);

                IntPtr floatRef = AndroidJNI.FindClass("java/lang/Float");
                FloatClass = AndroidJNI.NewGlobalRef(floatRef);
                FloatValueMethod = AndroidJNI.GetMethodID(floatRef, "floatValue", "()F");
                AndroidJNI.DeleteLocalRef(floatRef);

                IntPtr doubleRef = AndroidJNI.FindClass("java/lang/Double");
                DoubleClass = AndroidJNI.NewGlobalRef(doubleRef);
                DoubleValueMethod = AndroidJNI.GetMethodID(doubleRef, "doubleValue", "()D");
                AndroidJNI.DeleteLocalRef(doubleRef);

                IntPtr dateRef = AndroidJNI.FindClass("java/util/Date");
                DateClass = AndroidJNI.NewGlobalRef(dateRef);
                AndroidJNI.DeleteLocalRef(dateRef);

                IntPtr dateUtilsRef = AndroidJNI.FindClass("com/bugsnag/android/internal/DateUtils");
                DateUtilsClass = AndroidJNI.NewGlobalRef(dateUtilsRef);

                IntPtr entryRef = AndroidJNI.FindClass("java/util/Map$Entry");
                MapEntryClass = AndroidJNI.NewGlobalRef(entryRef);
                AndroidJNI.DeleteLocalRef(entryRef);

                IntPtr setRef = AndroidJNI.FindClass("java/util/Set");
                SetClass = AndroidJNI.NewGlobalRef(setRef);
                AndroidJNI.DeleteLocalRef(setRef);

                IntPtr stringRef = AndroidJNI.FindClass("java/lang/String");
                StringClass = AndroidJNI.NewGlobalRef(stringRef);
                AndroidJNI.DeleteLocalRef(stringRef);

                IntPtr sessionRef = AndroidJNI.FindClass("com/bugsnag/android/Session");
                SessionClass = AndroidJNI.NewGlobalRef(sessionRef);
                AndroidJNI.DeleteLocalRef(sessionRef);

                IntPtr clientRef = AndroidJNI.FindClass("com/bugsnag/android/Client");
                ClientClass = AndroidJNI.NewGlobalRef(clientRef);
                AndroidJNI.DeleteLocalRef(clientRef);

                BreadcrumbGetMetadata = AndroidJNI.GetMethodID(BreadcrumbClass, "getMetadata", "()Ljava/util/Map;");
                BreadcrumbGetType = AndroidJNI.GetMethodID(BreadcrumbClass, "getType", "()Lcom/bugsnag/android/BreadcrumbType;");
                BreadcrumbGetTimestamp = AndroidJNI.GetMethodID(BreadcrumbClass, "getStringTimestamp", "()Ljava/lang/String;");
                BreadcrumbGetMessage = AndroidJNI.GetMethodID(BreadcrumbClass, "getMessage", "()Ljava/lang/String;");
                CollectionIterator = AndroidJNI.GetMethodID(CollectionClass, "iterator", "()Ljava/util/Iterator;");
                IteratorHasNext = AndroidJNI.GetMethodID(IteratorClass, "hasNext", "()Z");
                IteratorNext = AndroidJNI.GetMethodID(IteratorClass, "next", "()Ljava/lang/Object;");
                MapEntryGetKey = AndroidJNI.GetMethodID(MapEntryClass, "getKey", "()Ljava/lang/Object;");
                MapEntryGetValue = AndroidJNI.GetMethodID(MapEntryClass, "getValue", "()Ljava/lang/Object;");
                MapEntrySet = AndroidJNI.GetMethodID(MapClass, "entrySet", "()Ljava/util/Set;");
                AddFeatureFlagMethod = AndroidJNI.GetMethodID(ClientClass, "addFeatureFlag", "(Ljava/lang/String;Ljava/lang/String;)V");
                ClearFeatureFlagMethod = AndroidJNI.GetMethodID(ClientClass, "clearFeatureFlag", "(Ljava/lang/String;)V");
                ClearFeatureFlagsMethod = AndroidJNI.GetMethodID(ClientClass, "clearFeatureFlags", "()V");

                IntPtr objectRef = AndroidJNI.FindClass("java/lang/Object");
                ObjectToString = AndroidJNI.GetMethodID(objectRef, "toString", "()Ljava/lang/String;");
                ObjectGetClass = AndroidJNI.GetMethodID(objectRef, "getClass", "()Ljava/lang/Class;");
                AndroidJNI.DeleteLocalRef(objectRef);

                IntPtr classRef = AndroidJNI.FindClass("java/lang/Class");
                ClassIsArray = AndroidJNI.GetMethodID(classRef, "isArray", "()Z");
                AndroidJNI.DeleteLocalRef(classRef);

                ToIso8601 = AndroidJNI.GetStaticMethodID(DateUtilsClass, "toIso8601", "(Ljava/util/Date;)Ljava/lang/String;");
                AndroidJNI.DeleteLocalRef(dateUtilsRef);

                // the bugsnag-android notifier uses Activity lifecycle tracking to
                // determine if the application is in the foreground. As the unity
                // activity has already started at this point we need to tell the
                // notifier about the activity and the fact that it has started.
                using (AndroidJavaObject sessionTracker = client.Get<AndroidJavaObject>("sessionTracker"))
                using (AndroidJavaObject activityClass = activity.Call<AndroidJavaObject>("getClass"))
                {
                    string activityName = null;
                    using (AndroidJavaObject activityNameObject = activityClass.Call<AndroidJavaObject>("getSimpleName"))
                    {
                        if (activityNameObject != null)
                        {
                            activityName = AndroidJNI.GetStringUTFChars(activityNameObject.GetRawObject());
                        }
                    }
                    sessionTracker.Call("updateContext", activityName, true);
                }

            }
        }

        /**
         * Transforms an IConfiguration C# object into a Java Configuration object.
         */
        AndroidJavaObject CreateNativeConfig(Configuration config)
        {
            var obj = new AndroidJavaObject("com.bugsnag.android.Configuration", config.ApiKey);
            // configure automatic tracking of errors/sessions
            using (AndroidJavaObject errorTypes = new AndroidJavaObject("com.bugsnag.android.ErrorTypes"))
            {
                errorTypes.Call("setAnrs", config.EnabledErrorTypes.ANRs);
                errorTypes.Call("setNdkCrashes", config.EnabledErrorTypes.Crashes);
                errorTypes.Call("setUnhandledExceptions", config.EnabledErrorTypes.Crashes);
                obj.Call("setEnabledErrorTypes", errorTypes);
            }

            obj.Call("setAutoTrackSessions", config.AutoTrackSessions);
            obj.Call("setAutoDetectErrors", config.AutoDetectErrors);
            obj.Call("setAppVersion", config.AppVersion);
            obj.Call("setContext", config.Context);
            obj.Call("setMaxBreadcrumbs", config.MaximumBreadcrumbs);
            obj.Call("setMaxPersistedEvents", config.MaxPersistedEvents);
            obj.Call("setMaxPersistedSessions", config.MaxPersistedSessions);
            obj.Call("setPersistUser", config.PersistUser);
            obj.Call("setLaunchDurationMillis", config.LaunchDurationMillis);
            obj.Call("setSendLaunchCrashesSynchronously", config.SendLaunchCrashesSynchronously);
            obj.Call("setMaxReportedThreads", config.MaxReportedThreads);
            obj.Call("setMaxStringValueLength", config.MaxStringValueLength);
            obj.Call("setGenerateAnonymousId", config.GenerateAnonymousId);

            if (config.GetUser() != null)
            {
                var user = config.GetUser();
                obj.Call("setUser", user.Id, user.Email, user.Name);
            }

            //Register for callbacks
            if (config.GetOnSessionCallbacks() != null && config.GetOnSessionCallbacks().Count > 0)
            {
                obj.Call("addOnSession", new OnSessionCallback(config));
                _registeredForSessionCallbacks = true;
            }
            if (config.GetOnSendErrorCallbacks() != null && config.GetOnSendErrorCallbacks().Count > 0)
            {
                obj.Call("addOnSend", new OnSendErrorCallback(config));
            }

            // set endpoints
            var notify = config.Endpoints.Notify.ToString();
            var sessions = config.Endpoints.Session.ToString();
            using (AndroidJavaObject endpointConfig = new AndroidJavaObject("com.bugsnag.android.EndpointConfiguration", notify, sessions))
            {
                obj.Call("setEndpoints", endpointConfig);
            }

            //android layer expects a nonnull java Integer not just an int, so we check if it has actually been set to a valid value
            if (config.VersionCode > -1)
            {
                var javaInteger = new AndroidJavaObject("java.lang.Integer", config.VersionCode);
                obj.Call("setVersionCode", javaInteger);
            }

            //Null or empty check necessary because android will set the app.type to empty if that or null is passed as default
            if (!string.IsNullOrEmpty(config.AppType))
            {
                obj.Call("setAppType", config.AppType);
            }

            // set EnabledBreadcrumbTypes
            if (config.EnabledBreadcrumbTypes != null)
            {

                using (AndroidJavaObject enabledBreadcrumbs = new AndroidJavaObject("java.util.HashSet"))
                {
                    AndroidJavaClass androidBreadcrumbEnumClass = new AndroidJavaClass("com.bugsnag.android.BreadcrumbType");
                    for (int i = 0; i < config.EnabledBreadcrumbTypes.Length; i++)
                    {
                        var stringValue = Enum.GetName(typeof(BreadcrumbType), config.EnabledBreadcrumbTypes[i]).ToUpperInvariant();
                        using (AndroidJavaObject crumbType = androidBreadcrumbEnumClass.CallStatic<AndroidJavaObject>("valueOf", stringValue))
                        {
                            enabledBreadcrumbs.Call<Boolean>("add", crumbType);
                        }
                    }
                    obj.Call("setEnabledBreadcrumbTypes", enabledBreadcrumbs);
                }
            }

            // set enabled telemetry types
            if (config.Telemetry != null)
            {
                using AndroidJavaObject enabledTelemetry = new AndroidJavaObject("java.util.HashSet");
                AndroidJavaClass androidTelemetryEnumClass = new AndroidJavaClass("com.bugsnag.android.Telemetry");
                foreach (var telemtryType in config.Telemetry)
                {
                    var javaName = "";
                    switch (telemtryType)
                    {
                        case TelemetryType.InternalErrors:
                            javaName = "INTERNAL_ERRORS";
                            break;
                        case TelemetryType.Usage:
                            javaName = "USAGE";
                            break;
                    }
                    using AndroidJavaObject telemetryType = androidTelemetryEnumClass.CallStatic<AndroidJavaObject>("valueOf", javaName);
                    enabledTelemetry.Call<bool>("add", telemetryType);
                }
                obj.Call("setTelemetry", enabledTelemetry);
            }


            // set feature flags
            if (config.FeatureFlags != null && config.FeatureFlags.Count > 0)
            {
                foreach (DictionaryEntry entry in config.FeatureFlags)
                {
                    obj.Call("addFeatureFlag", (string)entry.Key, (string)entry.Value);
                }
            }

            // set sendThreads
            AndroidJavaClass androidThreadSendPolicyClass = new AndroidJavaClass("com.bugsnag.android.ThreadSendPolicy");
            using (AndroidJavaObject policy = androidThreadSendPolicyClass.CallStatic<AndroidJavaObject>("valueOf", GetAndroidFormatThreadSendName(config.SendThreads)))
            {
                obj.Call("setSendThreads", policy);
            }

            // set release stages
            obj.Call("setReleaseStage", config.ReleaseStage);

            if (config.EnabledReleaseStages != null && config.EnabledReleaseStages.Length > 0)
            {
                obj.Call("setEnabledReleaseStages", GetAndroidStringSetFromArray(config.EnabledReleaseStages));
            }

            // set DiscardedClasses
            if (config.DiscardClasses != null && config.DiscardClasses.Count > 0)
            {
                var patternsAsStrings = new string[config.DiscardClasses.Count];
                foreach (var pattern in config.DiscardClasses)
                {
                    patternsAsStrings[config.DiscardClasses.IndexOf(pattern)] = pattern.ToString();
                }
                
                obj.Call("setDiscardClasses", GetAndroidRegexPatternSetFromArray(patternsAsStrings));
            }

            // set ProjectPackages
            if (config.ProjectPackages != null && config.ProjectPackages.Length > 0)
            {
                obj.Call("setProjectPackages", GetAndroidStringSetFromArray(config.ProjectPackages));
            }

            // set redacted keys
            if (config.RedactedKeys != null && config.RedactedKeys.Count > 0)
            {
                var patternsAsStrings = new string[config.RedactedKeys.Count];
                foreach (var key in config.RedactedKeys)
                {
                    patternsAsStrings[config.RedactedKeys.IndexOf(key)] = key.ToString();
                }
                obj.Call("setRedactedKeys", GetAndroidRegexPatternSetFromArray(patternsAsStrings));
            }

            // add unity event callback
            var BugsnagUnity = new AndroidJavaClass("com.bugsnag.android.unity.BugsnagUnity");
            obj.Call("addOnError", BugsnagUnity.CallStatic<AndroidJavaObject>("getNativeCallback", new object[] { }));

            // set persistence directory
            if (!string.IsNullOrEmpty(config.PersistenceDirectory))
            {
                AndroidJavaObject androidFile = new AndroidJavaObject("java.io.File", config.PersistenceDirectory);
                obj.Call("setPersistenceDirectory", androidFile);
            }

            return obj;
        }

        private string GetAndroidFormatThreadSendName(ThreadSendPolicy threadSendPolicy)
        {
            switch (threadSendPolicy)
            {
                case ThreadSendPolicy.Always:
                    return "ALWAYS";
                case ThreadSendPolicy.UnhandledOnly:
                    return "UNHANDLED_ONLY";
                default:
                    return "NEVER";
            }
        }

        private AndroidJavaObject GetAndroidStringSetFromArray(string[] array)
        {
            AndroidJavaObject set = new AndroidJavaObject("java.util.HashSet");
            foreach (var item in array)
            {
                set.Call<Boolean>("add", item);
            }
            return set;
        }

        private AndroidJavaObject GetAndroidRegexPatternSetFromArray(string[] array)
        {
            AndroidJavaObject set = new AndroidJavaObject("java.util.HashSet");
            AndroidJavaClass patternClass = new AndroidJavaClass("java.util.regex.Pattern");

            foreach (var item in array)
            {
                try
                {
                    AndroidJavaObject pattern = patternClass.CallStatic<AndroidJavaObject>("compile", item);
                    set.Call<bool>("add", pattern);
                }
                catch (AndroidJavaException e)
                {
                    Debug.LogWarning("Failed to compile regex pattern: " + item + " " + e.Message);
                }
            }

            return set;
        }

        private void ConfigureNotifierInfo(AndroidJavaObject config)
        {
            using (AndroidJavaObject notifier = config.Call<AndroidJavaObject>("getNotifier"))
            {
                AndroidJavaObject androidNotifier = new AndroidJavaObject("com.bugsnag.android.Notifier");
                androidNotifier.Call("setUrl", androidNotifier.Get<string>("url"));
                androidNotifier.Call("setName", androidNotifier.Get<string>("name"));
                androidNotifier.Call("setVersion", androidNotifier.Get<string>("version"));
                AndroidJavaObject list = new AndroidJavaObject("java.util.ArrayList");
                list.Call<Boolean>("add", androidNotifier);
                notifier.Call("setDependencies", list);

                notifier.Call("setUrl", NotifierInfo.NotifierUrl);
                notifier.Call("setName", "Unity Bugsnag Notifier");
                notifier.Call("setVersion", NotifierInfo.NotifierVersion);
            }
        }

        /**
         * Pushes a local JNI frame with 128 capacity. This avoids the reference table
         * being exceeded, which can happen on some lower-end Android devices in extreme conditions
         * (e.g. Nexus 7 running Android 6). This is likely due to AndroidJavaObject
         * not deleting local references immediately.
         *
         * If this call is unsuccessful it indicates the device is low on memory so the caller should no-op.
         * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
         */
        private bool PushLocalFrame()
        {
            if (AndroidJNI.PushLocalFrame(128) != 0)
            {
                AndroidJNI.ExceptionClear(); // clear pending OutOfMemoryError.
                return false;
            }
            return true;
        }

        /**
         * Pops the local JNI frame, freeing any references in the table.
         * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
         */
        private void PopLocalFrame()
        {
            AndroidJNI.PopLocalFrame(System.IntPtr.Zero);
        }

        public void SetAutoDetectErrors(bool newValue)
        {
            CallNativeVoidMethod("setAutoNotify", "(Z)V", new object[] { newValue });
        }

        public void SetContext(string newValue)
        {
            CallNativeVoidMethod("setContext", "(Ljava/lang/String;)V", new object[] { newValue });
        }

        public void SetUser(User user)
        {
            var method = "setUser";
            var description = "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V";
            if (user == null)
            {
                CallNativeVoidMethod(method, description, new object[] { null, null, null });
            }
            else
            {
                CallNativeVoidMethod(method, description,
                    new object[] { user.Id, user.Email, user.Name });
            }
        }

        public void StartSession()
        {
            CallNativeVoidMethod("startSession", "()V", new object[] { });
        }

        public bool ResumeSession()
        {
            return CallNativeBoolMethod("resumeSession", "()Z", new object[] { });
        }

        public void PauseSession()
        {
            CallNativeVoidMethod("pauseSession", "()V", new object[] { });
        }

        public void UpdateSession(Session session)
        {
            if (session != null)
            {
                // The ancient version of the runtime used doesn't have an equivalent to GetUnixTime()
                var startedAt = (long)(session.StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, 0))?.TotalMilliseconds;
                CallNativeVoidMethod("registerSession", "(JLjava/lang/String;II)V", new object[]{
                    startedAt, session.Id.ToString(), session.UnhandledCount(),
                    session.HandledCount()
                });
            }
        }

        public Session GetCurrentSession()
        {
            var javaSession = CallNativeObjectMethodRef("getCurrentSession", "()Lcom/bugsnag/android/Session;", new object[] { });

            var id = AndroidJNI.CallStringMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getId"), new jvalue[] { });

            if (id == null)
            {
                return null;
            }

            var javaStartedAt = AndroidJNI.CallObjectMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getStartedAt"), new jvalue[] { });
            var unhandledCount = AndroidJNI.CallIntMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getUnhandledCount"), new jvalue[] { });
            var handledCount = AndroidJNI.CallIntMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getHandledCount"), new jvalue[] { });
            var timeLong = AndroidJNI.CallLongMethod(javaStartedAt, AndroidJNIHelper.GetMethodID(DateClass, "getTime"), new jvalue[] { });
            var unityDateTime = new DateTime(1970, 1, 1).AddMilliseconds(timeLong);

            return new Session(id, unityDateTime, handledCount, unhandledCount );

        }

        public void MarkLaunchCompleted()
        {
            CallNativeVoidMethod("markLaunchCompleted", "()V", new object[] { });
        }

        public Dictionary<string, object> GetApp()
        {
            return GetJavaMapData("getApp");
        }

        public Dictionary<string, object> GetDevice()
        {
            return GetJavaMapData("getDevice");
        }

        public Dictionary<string, object> GetMetadata()
        {
            var metadata = GetJavaMapData("getMetadata");
            return metadata;
        }

        public Dictionary<string, object> GetUser()
        {
            return GetJavaMapData("getUser");
        }

        public void ClearMetadata(string tab)
        {
            if (tab == null)
            {
                return;
            }
            CallNativeVoidMethod("clearMetadata", "(Ljava/lang/String;Ljava/lang/String;)V", new object[] { tab, null });
        }

        public void ClearMetadata(string tab, string key)
        {
            if (tab == null)
            {
                return;
            }
            CallNativeVoidMethod("clearMetadata", "(Ljava/lang/String;Ljava/lang/String;)V", new object[] { tab, key });
        }

        public void AddMetadata(string section, IDictionary<string,object> metadata)
        {
            if (section == null || metadata == null)
            {
                return;
            }
            using (var disposableContainer = new DisposableContainer())
            {
                CallNativeVoidMethod("addMetadata", "(Ljava/lang/String;Ljava/util/Map;)V",
                new object[] { section, DictionaryToJavaMap(metadata, disposableContainer) });
            }
        }



        internal static AndroidJavaObject DictionaryToJavaMap(IDictionary<string, object> inputDict, DisposableContainer disposables)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            disposables.Add(map);
            if (inputDict != null)
            {
                foreach (var entry in inputDict)
                {
                    
                    var key = new AndroidJavaObject("java.lang.String", entry.Key);
                    disposables.Add(key);
                    if (entry.Value == null)
                    {
                        map.Call<AndroidJavaObject>("put", key, null);
                    }
                    else if (entry.Value is IDictionary)
                    {
                        if (entry.Value is IDictionary<string, object> dictionary)
                        {
                            map.Call<AndroidJavaObject>("put", key, DictionaryToJavaMap(dictionary, disposables));
                        }
                        else
                        {
                            var convertedDictionary = ConvertIfPoss(entry.Value);
                            if (convertedDictionary != null)
                            {
                                map.Call<AndroidJavaObject>("put", key, DictionaryToJavaMap(convertedDictionary, disposables));
                            }
                        }
                    }
                    else if (IsListOrArray(entry.Value))
                    {
                        map.Call<AndroidJavaObject>("put", key, GetJavaArrayListFromCollection(entry.Value, disposables));
                    }
                    else
                    {
                        map.Call<AndroidJavaObject>("put", key, GetAndroidObjectFromBasicObject(entry.Value, disposables));
                    }
                }
            }
            return map;
        }

        private static IDictionary<string, object> ConvertIfPoss(object o)
        {
            var stringDict = o as IDictionary<string, object>;
            if (stringDict != null)
            {
                return stringDict;
            }

            var dict = o as IDictionary;

            if (dict != null)
            {
                stringDict = new Dictionary<string, object>();
                foreach (var key in dict.Keys)
                {
                    stringDict[key?.ToString() ?? ""] = dict[key];
                }
                return stringDict;
            }
            return null;
        }

        private static bool IsListOrArray(object theObject)
        {
            var oType = theObject.GetType();
            return (oType.IsGenericType && oType.GetGenericTypeDefinition() == typeof(List<>)) || oType.IsArray;
        }

        private static AndroidJavaObject GetJavaArrayListFromCollection(object theObject, DisposableContainer disposableContainer)
        {
            var collection = (IEnumerable)theObject;
            var arrayList = new AndroidJavaObject("java.util.ArrayList");
            disposableContainer.Add(arrayList);
            foreach (var item in collection)
            {
                arrayList.Call<Boolean>("add", GetAndroidObjectFromBasicObject(item, disposableContainer));
            }
            return arrayList;
        }

        private static AndroidJavaObject GetAndroidObjectFromBasicObject(object theObject, DisposableContainer disposableContainer)
        {
            if (theObject == null)
            {
                return null;
            }
            string nativeClass;
            if (theObject.GetType() == typeof(string))
            {
                nativeClass = "java.lang.String";
            }
            else if (theObject.GetType() == typeof(bool))
            {
                nativeClass = "java.lang.Boolean";
            }
            else if (theObject.GetType() == typeof(int))
            {
                nativeClass = "java.lang.Integer";
            }
            else if (theObject.GetType() == typeof(double))
            {
                nativeClass = "java.lang.Double";
            }
            else if (theObject.GetType() == typeof(float))
            {
                nativeClass = "java.lang.Float";
            }
            else if (theObject.GetType() == typeof(long))
            {
                nativeClass = "java.lang.Long";
            }
            else
            {
                var stringObj = new AndroidJavaObject("java.lang.String", theObject.ToString());
                disposableContainer.Add(stringObj);
                return stringObj;
            }
            var androidObj = new AndroidJavaObject(nativeClass, theObject);
            disposableContainer.Add(androidObj);
            return androidObj;
        }

        public void LeaveBreadcrumb(string name, string type, IDictionary<string, object> metadata)
        {
            if (!CanRunJNI())
            {
                return;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }
            if (PushLocalFrame())
            {
                using (var disposableContainer = new DisposableContainer())
                {
                    var map = DictionaryToJavaMap(metadata, disposableContainer);
                    CallNativeVoidMethod("leaveBreadcrumb", "(Ljava/lang/String;Ljava/lang/String;Ljava/util/Map;)V",
                            new object[] { name, type, map });
                }
                PopLocalFrame();
            }
            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
        }

        public List<Breadcrumb> GetBreadcrumbs()
        {
            List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();
            if (!CanRunJNI())
            {
                return breadcrumbs;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            IntPtr javaBreadcrumbs = CallNativeObjectMethodRef("getBreadcrumbs", "()Ljava/util/List;", new object[] { });
            IntPtr iterator = AndroidJNI.CallObjectMethod(javaBreadcrumbs, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(javaBreadcrumbs);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr crumb = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                breadcrumbs.Add(ConvertToBreadcrumb(crumb));
                AndroidJNI.DeleteLocalRef(crumb);
            }
            AndroidJNI.DeleteLocalRef(iterator);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }

            return breadcrumbs;
        }

        private Dictionary<string, object> GetJavaMapData(string methodName)
        {
            if (!CanRunJNI())
            {
                return new Dictionary<string, object>();
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            IntPtr map = CallNativeObjectMethodRef(methodName, "()Ljava/util/Map;", new object[] { });
            Dictionary<string, object> value = DictionaryFromJavaMap(map);
            AndroidJNI.DeleteLocalRef(map);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }

            return value;
        }

        // Manually converts any C# strings in the arguments, replacing invalid chars with the replacement char..
        // If we don't do this, C# will coerce them using NewStringUTF, which crashes on invalid UTF-8.
        // Arg lists processed this way must be released using ReleaseConvertedStringArgs.
        private object[] ConvertStringArgsToNative(object[] args)
        {
            object[] itemsAsJavaObjects = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var obj = args[i];

                if (obj is string)
                {
                    itemsAsJavaObjects[i] = new AndroidJavaObject("java.lang.String", obj as string);
                }
                else
                {
                    itemsAsJavaObjects[i] = obj;
                }
            }
            return itemsAsJavaObjects;
        }

        // Release any strings in a processed argument list.
        // @param originalArgs: The original C# args.
        // @param convertedArgs: The args list returned by ConvertStringArgsToNative.
        private void ReleaseConvertedStringArgs(object[] originalArgs, object[] convertedArgs)
        {
            for (int i = 0; i < originalArgs.Length; i++)
            {
                if (originalArgs[i] is string)
                {
                    (convertedArgs[i] as AndroidJavaObject).Dispose();
                }
            }
        }

        private void CallNativeVoidMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(convertedArgs, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
        }

        private IntPtr CallNativeObjectMethodRef(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return IntPtr.Zero;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return nativeValue;
        }

        private IntPtr ConvertToStringJNIArrayRef(string[] items)
        {
            if (items == null || items.Length == 0)
            {
                return IntPtr.Zero;
            }

            AndroidJavaObject[] itemsAsJavaObjects = new AndroidJavaObject[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                itemsAsJavaObjects[i] = new AndroidJavaObject("java.lang.String", items[i]);
            }

            AndroidJavaObject first = itemsAsJavaObjects[0];
            IntPtr rawArray = AndroidJNI.NewObjectArray(items.Length, StringClass, first.GetRawObject());
            first.Dispose();

            for (int i = 1; i < items.Length; i++)
            {
                AndroidJNI.SetObjectArrayElement(rawArray, i, itemsAsJavaObjects[i].GetRawObject());
                itemsAsJavaObjects[i].Dispose();
            }

            return rawArray;
        }

        private string CallNativeStringMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return "";
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            string value = null;
            if (nativeValue != null && nativeValue != IntPtr.Zero)
            {
                value = AndroidJNI.GetStringUTFChars(nativeValue);
            }
            AndroidJNI.DeleteLocalRef(nativeValue);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return value;
        }

        private bool CallNativeBoolMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return false;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            bool nativeValue = AndroidJNI.CallStaticBooleanMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);
            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return nativeValue;
        }

        [DllImport("bugsnag-unity")]
        private static extern bool bsg_unity_isJNIAttached();

        private Breadcrumb ConvertToBreadcrumb(IntPtr javaBreadcrumb)
        {

            IntPtr javaMetadata = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMetadata, new jvalue[] { });
            var metadata = DictionaryFromJavaMap(javaMetadata);
            AndroidJNI.DeleteLocalRef(javaMetadata);


            IntPtr type = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetType, new jvalue[] { });
            string typeName = AndroidJNI.CallStringMethod(type, ObjectToString, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(type);

            string message = "<empty>";
            IntPtr messageObj = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMessage, new jvalue[] { });
            if (messageObj != IntPtr.Zero)
            {
                message = AndroidJNI.GetStringUTFChars(messageObj);
            }
            AndroidJNI.DeleteLocalRef(messageObj);

            string timestamp = AndroidJNI.CallStringMethod(javaBreadcrumb, BreadcrumbGetTimestamp, new jvalue[] { });
            return new Breadcrumb(message, timestamp, typeName, metadata);
        }

        internal static bool IsUnity2019OrNewer()
        {
            using (AndroidJavaClass CharsetClass = new AndroidJavaClass("java.nio.charset.Charset"))
            using (AndroidJavaObject Charset = CharsetClass.CallStatic<AndroidJavaObject>("defaultCharset"))
            {
                try
                { // should succeed on Unity 2019.1 and above
                    using (AndroidJavaObject obj = new AndroidJavaObject("java.lang.String", new sbyte[0], Charset))
                    {
                        return true;
                    }
                }
                catch (System.Exception _)
                { // use legacy API on older versions
                    return false;
                }
            }
        }

        private bool CanRunJNI()
        {
            return CanRunOnBackgroundThread || object.ReferenceEquals(Thread.CurrentThread, MainThread);
        }

        private Dictionary<string, object> DictionaryFromJavaMap(IntPtr source)
        {
            var dict = new Dictionary<string, object>();

            IntPtr entries = AndroidJNI.CallObjectMethod(source, MapEntrySet, new jvalue[] { });
            IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(entries);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                string key = AndroidJNI.CallStringMethod(entry, MapEntryGetKey, new jvalue[] { });
                IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[] { });
                AndroidJNI.DeleteLocalRef(entry);

                if (value != null && value != IntPtr.Zero)
                {
                    IntPtr valueClass = AndroidJNI.CallObjectMethod(value, ObjectGetClass, new jvalue[] { });
                    if (AndroidJNI.CallBooleanMethod(valueClass, ClassIsArray, new jvalue[] { }))
                    {
                        string[] values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value);
                        dict.AddToPayload(key, values);
                    }
                    else if (AndroidJNI.IsInstanceOf(value, MapClass))
                    {
                        dict.AddToPayload(key, DictionaryFromJavaMap(value));
                    }
                    else if (AndroidJNI.IsInstanceOf(value, DateClass))
                    {
                        jvalue[] args = new jvalue[1];
                        args[0].l = value;
                        var time = AndroidJNI.CallStaticStringMethod(DateUtilsClass, ToIso8601, args);
                        dict.AddToPayload(key, time);
                    }
                    else // parse for basic data types
                    {
                        if (AndroidJNI.IsInstanceOf(value, BooleanClass))
                        {
                            var boolValue = AndroidJNI.CallBooleanMethod(value, BooleanValueMethod, new jvalue[] { });
                            dict.AddToPayload(key, boolValue);
                        }
                        else if (AndroidJNI.IsInstanceOf(value, IntClass))
                        {
                            var intValue = AndroidJNI.CallIntMethod(value, IntValueMethod, new jvalue[] { });
                            dict.AddToPayload(key, intValue);
                        }
                        else if (AndroidJNI.IsInstanceOf(value, LongClass))
                        {
                            var longValue = AndroidJNI.CallLongMethod(value, LongValueMethod, new jvalue[] { });
                            dict.AddToPayload(key, longValue);
                        }
                        else if (AndroidJNI.IsInstanceOf(value, FloatClass))
                        {
                            var floatValue = AndroidJNI.CallFloatMethod(value, FloatValueMethod, new jvalue[] { });
                            dict.AddToPayload(key, floatValue);
                        }
                        else if (AndroidJNI.IsInstanceOf(value, DoubleClass))
                        {
                            var doubleValue = AndroidJNI.CallDoubleMethod(value, DoubleValueMethod, new jvalue[] { });
                            dict.AddToPayload(key, doubleValue);
                        }
                        else // last case, cast to string
                        {
                            var stringValue = AndroidJNI.CallStringMethod(value, ObjectToString, new jvalue[] { });
                            if (!string.IsNullOrEmpty(stringValue) && stringValue == "null")
                            {
                                dict.AddToPayload(key, null);
                            }
                            else
                            {
                                dict.AddToPayload(key, stringValue);
                            }
                        }
                    }
                    AndroidJNI.DeleteLocalRef(valueClass);
                }
                AndroidJNI.DeleteLocalRef(value);
            }
            AndroidJNI.DeleteLocalRef(iterator);

            return dict;
        }

        public LastRunInfo GetlastRunInfo()
        {
            var javaLastRunInfo = CallNativeObjectMethodRef("getLastRunInfo", "()Lcom/bugsnag/android/LastRunInfo;", new object[] { });
            var crashed = AndroidJNI.GetBooleanField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "crashed"));
            var consecutiveLaunchCrashes = AndroidJNI.GetIntField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "consecutiveLaunchCrashes"));
            var crashedDuringLaunch = AndroidJNI.GetBooleanField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "crashedDuringLaunch"));
            var lastRunInfo = new LastRunInfo
            {
                ConsecutiveLaunchCrashes = consecutiveLaunchCrashes,
                Crashed = crashed,
                CrashedDuringLaunch = crashedDuringLaunch
            };
            return lastRunInfo;
        }

        private IntPtr GetClientRef()
        {
            return CallNativeObjectMethodRef("getClient", "()Lcom/bugsnag/android/Client;", new object[] { });
        }

        public void AddFeatureFlag(string name, string variant)
        {
            object[] args = new object[] { name, variant };
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
            AndroidJNI.CallVoidMethod(GetClientRef(), AddFeatureFlagMethod, jargs);
        }

        public void ClearFeatureFlag(string name)
        {
            object[] args = new object[] { name };
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
            AndroidJNI.CallVoidMethod(GetClientRef(), ClearFeatureFlagMethod, jargs);
        }

        public void ClearFeatureFlags()
        {
            AndroidJNI.CallVoidMethod(GetClientRef(), ClearFeatureFlagsMethod, null);
        }

        public void RegisterForOnSessionCallbacks()
        {
            if (_registeredForSessionCallbacks || _configuration == null)
            {
                return;
            }

            var addOnSessionmethodId = AndroidJNI.GetMethodID(ClientClass, "addOnSession", "(Lcom/bugsnag/android/OnSessionCallback;)V");
            var callback = new OnSessionCallback(_configuration);
            object[] args = new object[] { callback };
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
            AndroidJNI.CallVoidMethod(GetClientRef(), addOnSessionmethodId, jargs);
            _registeredForSessionCallbacks = true;
        }

    }
}
#endif