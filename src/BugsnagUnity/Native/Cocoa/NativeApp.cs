using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public class NativeApp: IApp
    {

        private IntPtr _nativeApp;


        public NativeApp(IntPtr nativeApp)
        {
            _nativeApp = nativeApp;
        }

        public string BinaryArch { get; set; }
        public string BuildUuid { get; set; }
        public string BundleVersion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string CodeBundleId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DsymUuid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ReleaseStage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? VersionCode { get; set; }
    }
}
