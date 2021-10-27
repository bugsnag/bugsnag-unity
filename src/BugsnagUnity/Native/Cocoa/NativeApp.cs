using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeApp : NativePayloadClassWrapper, IApp
    {

        private const string BINARY_ARCH_KEY = "binaryArch";
        private const string BUNDLE_VERSION_KEY = "bundleVersion";
        private const string CODE_BUNDLE_ID_KEY = "codeBundleId";
        private const string DSYM_UUID_KEY = "dsymUuid";
        private const string ID_KEY = "id";
        private const string RELEASE_STAGE_KEY = "releaseStage";
        private const string TYPE_KEY = "type";
        private const string VERSION_KEY = "version";

        internal NativeApp(IntPtr nativeApp) : base(nativeApp)
        {
        }

        public string BuildUuid { get; set; }
        public int? VersionCode { get; set; }

        public string BinaryArch { get => GetNativeString(BINARY_ARCH_KEY); set => SetNativeString(BINARY_ARCH_KEY, value); }

        public string BundleVersion { get => GetNativeString(BUNDLE_VERSION_KEY); set => SetNativeString(BUNDLE_VERSION_KEY,value); }

        public string CodeBundleId { get => GetNativeString(CODE_BUNDLE_ID_KEY); set => SetNativeString(CODE_BUNDLE_ID_KEY, value); }

        public string DsymUuid { get => GetNativeString(DSYM_UUID_KEY); set => SetNativeString(DSYM_UUID_KEY, value); }

        public string Id { get => GetNativeString(ID_KEY); set => SetNativeString(ID_KEY, value); }

        public string ReleaseStage { get => GetNativeString(RELEASE_STAGE_KEY); set => SetNativeString(RELEASE_STAGE_KEY, value); }

        public string Type { get => GetNativeString(TYPE_KEY); set => SetNativeString(TYPE_KEY, value); }

        public string Version { get => GetNativeString(VERSION_KEY); set => SetNativeString(VERSION_KEY, value); }

       
    }
}
