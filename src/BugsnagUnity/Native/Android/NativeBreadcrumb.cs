using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeBreadcrumb : NativePayloadClassWrapper, IBreadcrumb
    {
        public NativeBreadcrumb(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Message { get => GetNativeString("getMessage"); set => SetNativeString("setMessage",value); }
        public Dictionary<string, object> Metadata { get => GetNativeDictionary("getMetadata"); set => SetNativeDictionary("setMetadata",value); }

        public DateTime? Timestamp => GetNativeDateTime("getTimestamp");

        public BreadcrumbType Type { get => GetBreadcrumbType(); set => SetBreadcrumbType(value); }

        private BreadcrumbType GetBreadcrumbType()
        {
            var native = NativePointer.Call<AndroidJavaObject>("getType").Call<string>("toString").ToLower();
            return Breadcrumb.ParseBreadcrumbType(native);
        }

        private void SetBreadcrumbType(BreadcrumbType breadcrumbType)
        {
            var nativeCrumb = new AndroidJavaClass("com.bugsnag.android.BreadcrumbType").GetStatic<AndroidJavaObject>(breadcrumbType.ToString().ToUpper());
            NativePointer.Call("setType",nativeCrumb);
        }
    }
}
