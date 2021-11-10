using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        public bool? Unhandled { get => GetNativeBool(UNHANDLED_KEY); set => SetNativeBool(UNHANDLED_KEY, value); }

        private List<IBreadcrumb> _breadcrumbs = new List<IBreadcrumb>();

        public List<IBreadcrumb> Breadcrumbs => _breadcrumbs;

        private List<IError> _errors = new List<IError>();

        public List<IError> Errors => _errors;

        public Severity Severity { get => GetSeverityFromEvent(); set => NativeCode.bugsnag_setEventSeverity(NativePointer, value.ToString().ToLower()); }

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
            _user.Id = id;
            _user.Email = email;
            _user.Name = name;
        }

        public void AddMetadata(string section, string key, object value)
        {
            var metadataSection = new Dictionary<string, object>() {
                { key, value }
            };
            AddMetadata(section, metadataSection);
        }

        public void AddMetadata(string section, Dictionary<string, object> metadata)
        {
            var index = 0;
            var count = 0;
            if (metadata != null)
            {
                var metadataArray = new string[metadata.Count * 2];

                foreach (var data in metadata)
                {
                    if (data.Key != null)
                    {
                        metadataArray[index] = data.Key;
                        metadataArray[index + 1] = data.Value.ToString();
                        count += 2;
                    }
                    index += 2;
                }
                NativeCode.bugsnag_setEventMetadata(NativePointer, section, metadataArray, count);
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

        public Dictionary<string, object> GetMetadata(string section)
        {
            var metadata = new Dictionary<string, object>();
            var handle = GCHandle.Alloc(metadata);
            try
            {
                NativeCode.bugsnag_getEventMetaData(NativePointer, GCHandle.ToIntPtr(handle), section, PopulateMetaDataSection);
            }
            finally
            {
                handle.Free();
            }
            return metadata;
        }

        [MonoPInvokeCallback(typeof(NativeCode.EventMetadata))]
        static void PopulateMetaDataSection(IntPtr instance, string[] keys, int keysSize, string[] values, int valuesSize)
        {
            var handle = GCHandle.FromIntPtr(instance);
            if (handle.Target is Dictionary<string,object> metadata)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    var value = values[i];
                    if (key.Equals("simulator"))
                    {
                        value = value.Equals("0") || value.Equals("false") ? "false" : "true";
                    }
                    metadata.Add(key, value);
                }
            }
        }


    }
}
