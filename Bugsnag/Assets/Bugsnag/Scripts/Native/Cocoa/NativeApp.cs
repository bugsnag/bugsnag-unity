#if UNITY_IOS || UNITY_STANDALONE_OSX && !UNITY_EDITOR

using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeApp : IApp
    {

        private const string BINARY_ARCH_KEY = "binaryArch";
        private const string BUNDLE_VERSION_KEY = "bundleVersion";
        private const string CODE_BUNDLE_ID_KEY = "codeBundleId";
        private const string DSYM_UUID_KEY = "dsymUuid";
        private const string ID_KEY = "id";
        private const string RELEASE_STAGE_KEY = "releaseStage";
        private const string TYPE_KEY = "type";
        private const string VERSION_KEY = "version";

        internal NativePayloadClassWrapper NativeWrapper;

        internal NativeApp(IntPtr nativeApp)
        {
            NativeWrapper = new NativePayloadClassWrapper(nativeApp);
        }

        public string BuildUuid { get; set; }
        public int? VersionCode { get; set; }

        public string BinaryArch { get => NativeWrapper.GetNativeString(BINARY_ARCH_KEY); set => NativeWrapper.SetNativeString(BINARY_ARCH_KEY, value); }

        public string BundleVersion { get => NativeWrapper.GetNativeString(BUNDLE_VERSION_KEY); set => NativeWrapper.SetNativeString(BUNDLE_VERSION_KEY,value); }

        public string CodeBundleId { get => NativeWrapper.GetNativeString(CODE_BUNDLE_ID_KEY); set => NativeWrapper.SetNativeString(CODE_BUNDLE_ID_KEY, value); }

        public string DsymUuid { get => NativeWrapper.GetNativeString(DSYM_UUID_KEY); set => NativeWrapper.SetNativeString(DSYM_UUID_KEY, value); }

        public string Id { get => NativeWrapper.GetNativeString(ID_KEY); set => NativeWrapper.SetNativeString(ID_KEY, value); }

        public string ReleaseStage { get => NativeWrapper.GetNativeString(RELEASE_STAGE_KEY); set => NativeWrapper.SetNativeString(RELEASE_STAGE_KEY, value); }

        public string Type { get => NativeWrapper.GetNativeString(TYPE_KEY); set => NativeWrapper.SetNativeString(TYPE_KEY, value); }

        public string Version { get => NativeWrapper.GetNativeString(VERSION_KEY); set => NativeWrapper.SetNativeString(VERSION_KEY, value); }

       
    }
}
#endif