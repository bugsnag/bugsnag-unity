using System;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class NativeApp : NativePayloadClassWrapper, IApp
    {
        internal NativeApp(AndroidJavaObject nativePointer) : base (nativePointer){}

        public string BinaryArch { get => GetNativeString("getBinaryArch"); set => SetNativeString("setBinaryArch", value); }
        public string BuildUuid { get => GetNativeString("getBuildUuid"); set => SetNativeString("setBuildUuid", value); }
        public string CodeBundleId { get => GetNativeString("getCodeBundleId"); set => SetNativeString("setCodeBundleId", value); }
        public string Id { get => GetNativeString("getId"); set => SetNativeString("setId", value); }
        public string ReleaseStage { get => GetNativeString("getReleaseStage"); set => SetNativeString("setReleaseStage", value); }
        public string Type { get => GetNativeString("getType"); set => SetNativeString("setType", value); }
        public string Version { get => GetNativeString("getVersion"); set => SetNativeString("setVersion", value); }
        public int? VersionCode { get => GetNativeInt("getVersionCode"); set => SetNativeInt("setVersionCode", value); }

        public string BundleVersion { get; set; }
        public string DsymUuid { get; set; }

    }
}
