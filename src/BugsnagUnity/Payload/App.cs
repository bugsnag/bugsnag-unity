using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents the "app" key in the error report payload.
    /// </summary>
    public class App : PayloadContainer, IApp
    {

        private const string BINARY_ARCH_KEY = "binaryArch";
        private const string BUILD_UUID_KEY = "buildUuid";
        private const string BUNDLE_VERSION_KEY = "bundleVersion";
        private const string CODE_BUNDLE_ID = "codeBundleId";
        private const string DSYM_UUID_KEY = "dsymUuid";
        private const string ID_KEY = "id";
        private const string RELEASESTAGE_KEY = "releaseStage";
        private const string TYPE_KEY = "type";
        private const string VERSION_KEY = "version";
        private const string VERSION_CODE_KEY = "versionCode";


        public string? BinaryArch
        {
            get => (string?)Get(BINARY_ARCH_KEY);
            set => Add(BINARY_ARCH_KEY, value);
        }

        public string? BuildUuid
        {
            get => (string?)Get(BUILD_UUID_KEY);
            set => Add(BUILD_UUID_KEY, value);
        }

        public string? BundleVersion
        {
            get => (string?)Get(BUNDLE_VERSION_KEY);
            set => Add(BUNDLE_VERSION_KEY, value);
        }

        public string? CodeBundleId
        {
            get => (string?)Get(CODE_BUNDLE_ID);
            set => Add(CODE_BUNDLE_ID, value);
        }

        public string? DsymUuid
        {
            get => (string?)Get(DSYM_UUID_KEY);
            set => Add(DSYM_UUID_KEY, value);
        }

        public string? Id
        {
            get => (string?)Get(ID_KEY);
            set => Add(ID_KEY, value);
        }

        public string? ReleaseStage
        {
            get => (string?)Get(RELEASESTAGE_KEY);
            set => Add(RELEASESTAGE_KEY, value);
        }

        public string? Type
        {
            get => (string?)Get(TYPE_KEY);
            set => Add(TYPE_KEY, value);
        }

        public string? Version
        {
            get => (string?)Get(VERSION_KEY);
            set => Add(VERSION_KEY, value);
        }

        public int? VersionCode
        {
            get => (int?)Get(VERSION_CODE_KEY);
            set => Add(VERSION_CODE_KEY, value);
        }

        internal App(Dictionary<string, object> cachedData)
        {
            Add(cachedData);
        }

        internal App(Configuration configuration)
        {
            Id = Application.identifier;
            ReleaseStage = configuration.ReleaseStage;
            Type = GetAppType(configuration);
            Version = configuration.AppVersion;
        }

        private string GetAppType(Configuration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.AppType))
            {
                return configuration.AppType;
            }
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                default:
                    return string.Empty;
            }
        }

    }
}
