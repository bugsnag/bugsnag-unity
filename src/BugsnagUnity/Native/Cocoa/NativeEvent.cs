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

        public NativeEvent(IntPtr nativeEvent) : base(nativeEvent)
        {
            App = new NativeAppWithState(NativeCode.bugsnag_getAppFromEvent(nativeEvent));
            Device = new NativeDeviceWithState(NativeCode.bugsnag_getDeviceFromEvent(nativeEvent));

            var breadcrumbHandle = GCHandle.Alloc(_breadcrumbs);
            NativeCode.bugsnag_getBreadcrumbsFromEvent(nativeEvent, GCHandle.ToIntPtr(breadcrumbHandle), GetBreadcrumbs);

            var errorsHandle = GCHandle.Alloc(_errors);
            NativeCode.bugsnag_getErrorsFromEvent(nativeEvent, GCHandle.ToIntPtr(errorsHandle), GetErrors);
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

        public string Context { get => GetNativeString(CONTEXT_KEY); set => SetNativeString(CONTEXT_KEY,value); }

        public IAppWithState App { get; set; }

        public IDeviceWithState Device { get; set; }

        public string ApiKey { get => GetNativeString(API_KEY_KEY); set => SetNativeString(API_KEY_KEY,value); }

        public string GroupingHash { get => GetNativeString(GROUPING_HASH_KEY); set => SetNativeString(GROUPING_HASH_KEY, value); }

        public bool? Unhandled { get => GetNativeBool(UNHANDLED_KEY); set => SetNativeBool(UNHANDLED_KEY,value); }

        private List<IBreadcrumb> _breadcrumbs = new List<IBreadcrumb>();

        public List<IBreadcrumb> Breadcrumbs => _breadcrumbs;

        private List<IError> _errors = new List<IError>();

        public List<IError> Errors => _errors;










        public Severity Severity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<IThread> Threads => throw new NotImplementedException();

        public void AddMetadata(string section, string key, object value)
        {
            throw new NotImplementedException();
        }

        public void AddMetadata(string section, Dictionary<string, object> metadata)
        {
            throw new NotImplementedException();
        }

        public void ClearMetadata(string section)
        {
            throw new NotImplementedException();
        }

        public void ClearMetadata(string section, string key)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetMetadata(string section)
        {
            throw new NotImplementedException();
        }

        public object GetMetadata(string section, string key)
        {
            throw new NotImplementedException();
        }

        public User GetUser()
        {
            throw new NotImplementedException();
        }

        public void SetUser(string id, string email, string name)
        {
            throw new NotImplementedException();
        }
    }
}
