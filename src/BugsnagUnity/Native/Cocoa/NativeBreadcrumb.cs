using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeBreadcrumb : NativePayloadClassWrapper, IBreadcrumb
    {

        private const string MESSAGE_KEY = "message";
        private const string TIMESTAMP_KEY = "timestamp";

        public NativeBreadcrumb(IntPtr nativeBreadcrumb) : base(nativeBreadcrumb)
        {
        }

        public string Message { get => GetNativeString(MESSAGE_KEY); set => SetNativeString(MESSAGE_KEY, value); }

        public Dictionary<string, object> Metadata { get => GetMetadata(); set => SetMetadata(value); }

        private Dictionary<string, object> GetMetadata()
        {
            var metadata = new Dictionary<string, object>();
            var handle = GCHandle.Alloc(metadata);
            NativeCode.bugsnag_getBreadcrumbMetadata(NativePointer, GCHandle.ToIntPtr(handle),GetMetadata);
            return metadata;
        }

        [MonoPInvokeCallback(typeof(NativeCode.BreadcrumbMetadata))]
        private static void GetMetadata(IntPtr metadata,string[] keys, int keysCount,string[] values, int valuesCount)
        {
            var handle = GCHandle.FromIntPtr(metadata);
            if (handle.Target is Dictionary<string, object> theMetadata)
            {
                for (var i = 0; i < keysCount; i++)
                {
                    theMetadata.Add(keys[i], values[i]);
                }
            }

        }

        private void SetMetadata(Dictionary<string, object> metadata)
        {
            var stringValues = new List<string>();
            foreach (var item in metadata)
            {
                stringValues.Add(item.Key);
                stringValues.Add(item.Value.ToString());
            }
            NativeCode.bugsnag_setBreadcrumbMetadata(NativePointer, stringValues.ToArray(), stringValues.Count);
        }

        public DateTime? Timestamp => GetNativeDate(TIMESTAMP_KEY);

        public BreadcrumbType Type {
            get => Breadcrumb.ParseBreadcrumbType( NativeCode.bugsnag_getBreadcrumbType(NativePointer) );
            set => NativeCode.bugsnag_setBreadcrumbType(NativePointer, value.ToString().ToLower());
        }
    }
}
