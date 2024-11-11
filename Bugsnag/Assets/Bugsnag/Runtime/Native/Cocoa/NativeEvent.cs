#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BSG_COCOA_DEV

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeEvent : NativePayloadClassWrapper, IEvent
    {

        private const string CONTEXT_KEY = "context";
        private const string API_KEY_KEY = "apiKey";
        private const string GROUPING_HASH_KEY = "groupingHash";
        private const string UNHANDLED_KEY = "unhandled";

        public string Context { get => GetNativeString(CONTEXT_KEY); set => SetNativeString(CONTEXT_KEY, value); }

        public IAppWithState App { get; set; }

        public IDeviceWithState Device { get; set; }

        public string ApiKey { get => GetNativeString(API_KEY_KEY); set => SetNativeString(API_KEY_KEY, value); }

        public string GroupingHash { get => GetNativeString(GROUPING_HASH_KEY); set => SetNativeString(GROUPING_HASH_KEY, value); }

        public bool Unhandled { 
            get {
                var nativeBool = GetNativeBool(UNHANDLED_KEY); 
                return nativeBool.HasValue ? nativeBool.Value : false;
            }
            set => SetNativeBool(UNHANDLED_KEY, value); 
        }

        private List<IBreadcrumb> _breadcrumbs = new List<IBreadcrumb>();

        public ReadOnlyCollection<IBreadcrumb> Breadcrumbs => new ReadOnlyCollection<IBreadcrumb>(_breadcrumbs);

        private List<IError> _errors = new List<IError>();

        public List<IError> Errors => _errors;

        public Severity Severity { get => GetSeverityFromEvent(); set => NativeCode.bugsnag_setEventSeverity(NativePointer, value.ToString().ToLowerInvariant()); }

        private List<IThread> _threads = new List<IThread>();

        public List<IThread> Threads => _threads;

        private IUser _user;

        public NativeEvent(IntPtr nativeEvent) : base(nativeEvent)
        {
            App = new NativeAppWithState(NativeCode.bugsnag_getAppFromEvent(nativeEvent));
            Device = new NativeDeviceWithState(NativeCode.bugsnag_getDeviceFromEvent(nativeEvent));

            var breadcrumbHandle = GCHandle.Alloc(_breadcrumbs);
            NativeCode.bugsnag_getBreadcrumbsFromEvent(nativeEvent, GCHandle.ToIntPtr(breadcrumbHandle), GetBreadcrumbs);

            var errorsHandle = GCHandle.Alloc(_errors);
            NativeCode.bugsnag_getErrorsFromEvent(nativeEvent, GCHandle.ToIntPtr(errorsHandle), GetErrors);

            var threadsHandle = GCHandle.Alloc(_threads);
            NativeCode.bugsnag_getThreadsFromEvent(nativeEvent, GCHandle.ToIntPtr(threadsHandle), GetThreads);

            _user = new NativeUser(NativeCode.bugsnag_getUserFromEvent(nativeEvent));
        }

        private Severity GetSeverityFromEvent()
        {
            var stringValue = NativeCode.bugsnag_getSeverityFromEvent(NativePointer);
            if (stringValue == "error")
            {
                return Severity.Error;
            }
            if (stringValue == "warning")
            {
                return Severity.Warning;
            }
            return Severity.Info;
        }

        [MonoPInvokeCallback(typeof(NativeCode.EventBreadcrumbs))]
        private static void GetBreadcrumbs(IntPtr instance, IntPtr[] breadcrumbPointers, int count)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<IBreadcrumb> breadcrumbs)
            {
                foreach (var pointer in breadcrumbPointers)
                {
                    breadcrumbs.Add(new NativeBreadcrumb(pointer));
                }
            }
        }

        [MonoPInvokeCallback(typeof(NativeCode.EventErrors))]
        private static void GetErrors(IntPtr instance, IntPtr[] errorPointers, int count)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<IError> errors)
            {
                foreach (var pointer in errorPointers)
                {
                    errors.Add(new NativeError(pointer));
                }
            }

        }

        [MonoPInvokeCallback(typeof(NativeCode.EventThreads))]
        private static void GetThreads(IntPtr instance, IntPtr[] threadPointers, int count)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is List<IThread> threads)
            {
                foreach (var pointer in threadPointers)
                {
                    threads.Add(new NativeThread(pointer));
                }
            }
        }

        public IUser GetUser()
        {
            return _user;
        }

        public void SetUser(string id, string email, string name)
        {
            NativeCode.bugsnag_setUserFromEvent(NativePointer, id, email, name);
        }

        public void AddMetadata(string section, string key, object value)
        {
            var metadataSection = new Dictionary<string, object>() {
                { key, value }
            };
            AddMetadata(section, metadataSection);
        }

        public void AddMetadata(string section, IDictionary<string, object> metadata)
        {
           
            if (metadata != null)
            {
                using (var stream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
                {
                    SimpleJson.SerializeObject(metadata, writer);
                    writer.Flush();
                    stream.Position = 0;
                    var jsonString = reader.ReadToEnd();
                    NativeCode.bugsnag_setEventMetadata(NativePointer, section, jsonString);
                }
            }
            else
            {
                NativeCode.bugsnag_clearEventMetadataSection(NativePointer, section);
            }
        }

        public void ClearMetadata(string section) => NativeCode.bugsnag_clearEventMetadataSection(NativePointer, section);

        public void ClearMetadata(string section, string key) => NativeCode.bugsnag_clearEventMetadataWithKey(NativePointer, section, key);

        public object GetMetadata(string section, string key)
        {
            var metadata = GetMetadata(section);
            foreach (var item in metadata)
            {
                if (item.Key == key)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public IDictionary<string, object> GetMetadata(string section)
        {
            var result = NativeCode.bugsnag_getEventMetaData(NativePointer, section);
            var dictionary = ((JsonObject)SimpleJson.DeserializeObject(result)).GetDictionary();
            return dictionary;
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            NativeCode.bugsnag_addFeatureFlagOnEvent(NativePointer, name, variant);
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
            NativeCode.bugsnag_clearFeatureFlagOnEvent(NativePointer, name);
        }

        public void ClearFeatureFlags()
        {
            NativeCode.bugsnag_clearFeatureFlagsOnEvent(NativePointer);
        }

        public ReadOnlyCollection<FeatureFlag> FeatureFlags
        {
            get
            {
                var jsonString = NativeCode.bugsnag_getFeatureFlagsFromEvent(NativePointer);
                var jsonArray = (JsonArray)SimpleJson.DeserializeObject(jsonString);
                var objects = jsonArray.Select((item)
                    => new FeatureFlag(((JsonObject)item).GetDictionary()));
                return new ReadOnlyCollection<FeatureFlag>(objects.ToList());
            }
        }
    }
}
#endif