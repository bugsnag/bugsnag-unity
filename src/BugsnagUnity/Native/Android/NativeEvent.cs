using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ReadOnlyCollection<IBreadcrumb> Breadcrumbs => GetBreadcrumbs();

        public List<IError> Errors => GetErrors();

        public string GroupingHash { get => GetNativeString("getGroupingHash"); set =>  SetNativeString("setGroupingHash", value); }

        public Severity Severity { get => GetSeverity(); set => SetSeverity(value); }

        public List<IThread> Threads => GetThreads();

        public bool? Unhandled { get => NativePointer.Call<bool>("isUnhandled"); set => NativePointer.Call("setUnhandled", (bool)value); }

        private Severity GetSeverity()
        {
            var nativeSeverity = NativePointer.Call<AndroidJavaObject>("getSeverity").Call<string>("toString").ToLowerInvariant();
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
            var nativeSeverity = new AndroidJavaClass("com.bugsnag.android.Severity").GetStatic<AndroidJavaObject>(severity.ToString().ToUpperInvariant());
            NativePointer.Call("setSeverity",nativeSeverity);
        }

        private ReadOnlyCollection<IBreadcrumb> GetBreadcrumbs()
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
            return new ReadOnlyCollection<IBreadcrumb>( theBreadcrumbs );
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
            var existingMetadata = GetNativeMetadataSection("getMetadata",section);
            if (existingMetadata == null)
            {
                existingMetadata = new Dictionary<string, object>();
            }
            existingMetadata[key] = value;
            AddMetadata(section,existingMetadata);
        }

        public void AddMetadata(string section, IDictionary<string, object> metadata)
        {
            SetNativeMetadataSection("addMetadata", section, metadata);
        }

        public void ClearMetadata(string section)
        {
            NativePointer.Call("clearMetadata",section);
        }

        public void ClearMetadata(string section, string key)
        {
            NativePointer.Call("clearMetadata", section, key);
        }

        public IDictionary<string, object> GetMetadata(string section)
        {
            return GetNativeMetadataSection("getMetadata", section);
        }

        public object GetMetadata(string section, string key)
        {
            var metadata = GetMetadata(section);
            if (metadata.ContainsKey(key))
            {
                return metadata[key];
            }
            return null;
        }

        public IUser GetUser() => new NativeUser(NativePointer.Call<AndroidJavaObject>("getUser"));

        public void SetUser(string id, string email, string name)
        {
            NativePointer.Call("setUser", id, email, name);
        }

        public void AddFeatureFlag(string name, string variant = null)
        {
            NativePointer.Call("addFeatureFlag",name,variant);
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
            NativePointer.Call("clearFeatureFlag",name);
        }

        public void ClearFeatureFlags()
        {
            NativePointer.Call("clearFeatureFlags");
        }

        public ReadOnlyCollection<FeatureFlag> FeatureFlags
        {
            get
            {
                var objects = new List<FeatureFlag>();
                var list = NativePointer.Call<AndroidJavaObject>("getFeatureFlags");
                var iterator = list.Call<AndroidJavaObject>("iterator");
                while (iterator.Call<bool>("hasNext"))
                {
                    var javaObject = iterator.Call<AndroidJavaObject>("next");
                    var name = javaObject.Call<String>("getName");
                    var variant = javaObject.Call<String>("getVariant");
                    objects.Add(new FeatureFlag(name, variant));
                }
                return new ReadOnlyCollection<FeatureFlag>(objects);
            }
        }
    }
}
