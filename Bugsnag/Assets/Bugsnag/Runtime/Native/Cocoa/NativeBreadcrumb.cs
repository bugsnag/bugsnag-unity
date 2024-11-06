#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BGS_COCOA_DEV

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

        public IDictionary<string, object> Metadata { get => GetMetadata(); set => SetMetadata(value); }

        private IDictionary<string, object> GetMetadata()
        {
            var result = NativeCode.bugsnag_getBreadcrumbMetadata(NativePointer);
            var dictionary = ((JsonObject)SimpleJson.DeserializeObject(result)).GetDictionary();
            return dictionary;
        }

        private void SetMetadata(IDictionary<string, object> metadata)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(metadata, writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                NativeCode.bugsnag_setBreadcrumbMetadata(NativePointer, jsonString);
            }
        }

        public DateTimeOffset? Timestamp => GetNativeDate(TIMESTAMP_KEY);

        public BreadcrumbType Type {
            get => Breadcrumb.ParseBreadcrumbType( NativeCode.bugsnag_getBreadcrumbType(NativePointer) );
            set => NativeCode.bugsnag_setBreadcrumbType(NativePointer, value.ToString().ToLowerInvariant());
        }
    }
}
#endif