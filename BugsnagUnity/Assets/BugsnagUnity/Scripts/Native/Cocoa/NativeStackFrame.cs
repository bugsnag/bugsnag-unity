#if UNITY_IOS || UNITY_STANDALONE_OSX && !UNITY_EDITOR

using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeStackFrame : NativePayloadClassWrapper, IStackframe
    {

        private const string FRAME_ADDRESS_KEY = "frameAddress";
        private const string IS_LR_KEY = "isLr";
        private const string IS_PC_KEY = "isPc";
        private const string MACHO_FILE_KEY = "machoFile";
        private const string MACHO_LOAD_ADDRESS = "machoLoadAddress";
        private const string MACHO_UUID_KEY = "machoUuid";
        private const string MACHO_VM_ADDRESS = "machoVmAddress";
        private const string METHOD_KEY = "method";
        private const string SYMBOL_ADDRESS_KEY = "symbolAddress";

        public NativeStackFrame(IntPtr nativeFrame) : base (nativeFrame)
        {
        }

        public string FrameAddress { get => GetNativeString(FRAME_ADDRESS_KEY); set => SetNativeString(FRAME_ADDRESS_KEY,value); }

        public bool? IsLr { get => GetNativeBool(IS_LR_KEY); set => SetNativeBool(IS_LR_KEY, value); }

        public bool? IsPc { get => GetNativeBool(IS_PC_KEY); set => SetNativeBool(IS_PC_KEY, value); }

        public string MachoFile { get => GetNativeString(MACHO_FILE_KEY); set => SetNativeString(MACHO_FILE_KEY, value); }

        public string MachoLoadAddress { get => GetNativeString(MACHO_LOAD_ADDRESS); set => SetNativeString(MACHO_LOAD_ADDRESS, value); }

        public string MachoUuid { get => GetNativeString(MACHO_UUID_KEY); set => SetNativeString(MACHO_UUID_KEY, value); }

        public string MachoVmAddress { get => GetNativeString(MACHO_VM_ADDRESS); set => SetNativeString(MACHO_VM_ADDRESS, value); }

        public string Method { get => GetNativeString(METHOD_KEY); set => SetNativeString(METHOD_KEY, value); }

        public string SymbolAddress { get => GetNativeString(SYMBOL_ADDRESS_KEY); set => SetNativeString(SYMBOL_ADDRESS_KEY, value); }

        public string File { get; set; }

        public bool? InProject { get; set; }

        public int? LineNumber { get; set; }

    }
}
#endif