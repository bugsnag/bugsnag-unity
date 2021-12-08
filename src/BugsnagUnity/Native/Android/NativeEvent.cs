using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeEvent : NativePayloadClassWrapper, IEvent
    {
        public NativeEvent(AndroidJavaObject androidJavaObject) : base (androidJavaObject){}

        public string ApiKey { get => GetNativeString("getApiKey"); set => SetNativeString("setApiKey",value); }

        public IAppWithState App => new NativeAppWithState(NativePointer.Call<AndroidJavaObject>("getApp"));

        public string Context { get => GetNativeString("getContext"); set => SetNativeString("setContext", value); }

        public IDeviceWithState Device => new NativeDeviceWithState(NativePointer.Call<AndroidJavaObject>("getDevice"));

        public List<IBreadcrumb> Breadcrumbs => GetBreadcrumbs();

        public List<IError> Errors => GetErrors();

        public string GroupingHash { get => GetNativeString("getGroupingHash"); set => SetNativeString("setGroupingHash", value); }

        public Severity Severity { get => GetSeverity(); set => SetSeverity(value); }

        public List<IThread> Threads => GetThreads();

        public bool? Unhandled { get => GetNativeBool("isUnhandled"); set => SetNativeBool("setUnhandled",value); }

        private Severity GetSeverity()
        {
            var nativeSeverity = NativePointer.Call<AndroidJavaObject>("getSeverity").Call<string>("toString").ToLower();
            if (nativeSeverity.Contains("error"))
            {
                return Severity.Error;
            }
            if (nativeSeverity.Contains("warning"))
            {
                return Severity.Warning;
            }
            return Severity.Info;
        }

        private void SetSeverity(Severity severity)
        {
            var nativeSeverity = new AndroidJavaObject("com.bugsnag.android.Severity",severity.ToString().ToLower());
            NativePointer.Call("setSeverity",nativeSeverity);
        }

        private List<IBreadcrumb> GetBreadcrumbs()
        {
            var nativeList = NativePointer.Call<AndroidJavaObject>("getBreadcrumbs");
            if (nativeList == null)
            {
                return null;
            }
            var theBreadcrumbs = new List<IBreadcrumb>();
            var iterator = nativeList.Call<AndroidJavaObject>("iterator");
            while (iterator.Call<bool>("hasNext"))
            {
                var next = iterator.Call<AndroidJavaObject>("next");
                theBreadcrumbs.Add(new NativeBreadcrumb(next));
            }
            return theBreadcrumbs;
        }

        private List<IError> GetErrors()
        {
            var nativeList = NativePointer.Call<AndroidJavaObject>("getErrors");
            if (nativeList == null)
            {
                return null;
            }
            var theErrors = new List<IError>();
            var iterator = nativeList.Call<AndroidJavaObject>("iterator");
            while (iterator.Call<bool>("hasNext"))
            {
                var next = iterator.Call<AndroidJavaObject>("next");
                theErrors.Add(new NativeError(next));
            }
            return theErrors;
        }

        private List<IThread> GetThreads()
        {
            var nativeList = NativePointer.Call<AndroidJavaObject>("getThreads");
            if (nativeList == null)
            {
                return null;
            }
            var theThreads = new List<IThread>();
            var iterator = nativeList.Call<AndroidJavaObject>("iterator");
            while (iterator.Call<bool>("hasNext"))
            {
                var next = iterator.Call<AndroidJavaObject>("next");
                theThreads.Add(new NativeThread(next));
            }
            return theThreads;
        }

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

        public IUser GetUser() => new NativeUser(NativePointer.Call<AndroidJavaObject>("getUser"));

        public void SetUser(string id, string email, string name)
        {
            NativePointer.Call("setUser", id, email, name);
        }
    }
}
