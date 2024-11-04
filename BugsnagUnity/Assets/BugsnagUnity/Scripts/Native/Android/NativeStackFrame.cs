#if UNITY_ANDROID && !UNITY_EDITOR
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeStackFrame : NativePayloadClassWrapper, IStackframe
    {
        public NativeStackFrame(AndroidJavaObject androidJavaObject) : base (androidJavaObject){}

        public string FrameAddress { get; set; }
        public bool? IsLr { get; set; }
        public bool? IsPc { get; set; }
        public string MachoFile { get; set; }
        public string MachoLoadAddress { get; set; }
        public string MachoUuid { get; set; }
        public string MachoVmAddress { get; set; }
        public string Method { get => GetNativeString("getMethod"); set => SetNativeString("setMethod",value); }
        public string SymbolAddress { get; set; }
        public string File { get => GetNativeString("getFile"); set => SetNativeString("setFile", value); }
        public bool? InProject { get => GetNativeBool("getInProject"); set => SetNativeBool("setInProject",value); }
        public int? LineNumber { get => GetNativeInt("getLineNumber"); set => SetNativeInt("setLineNumber",value); }
    }
}
#endif