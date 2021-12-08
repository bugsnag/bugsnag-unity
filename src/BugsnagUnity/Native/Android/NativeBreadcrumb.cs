using System;
using System.Collections.Generic;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    public class NativeBreadcrumb : NativePayloadClassWrapper, IBreadcrumb
    {
        public NativeBreadcrumb(AndroidJavaObject androidJavaObject) : base(androidJavaObject){}

        public string Message { get => GetNativeString("getMessage"); set => SetNativeString("setMessage",value); }
        public Dictionary<string, object> Metadata { get => GetNativeDictionary("getMetadata"); set => SetNativeDictionary("setMetadata",value); }

        public DateTime? Timestamp => GetNativeDateTime("getTimestamp");

        public BreadcrumbType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
