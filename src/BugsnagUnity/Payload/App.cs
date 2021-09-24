using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    /// <summary>
    /// Represents the "app" key in the error report payload.
    /// </summary>
    public class App : PayloadContainer
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


        public string BinaryArch
        {
            get
            {
                if (HasKey(BINARY_ARCH_KEY))
                {
                    return (string)Payload.Get(BINARY_ARCH_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(BINARY_ARCH_KEY, value);
        }

        public string BuildUuid
        {
            get
            {
                if (HasKey(BUILD_UUID_KEY))
                {
                    return (string)Payload.Get(BUILD_UUID_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(BUILD_UUID_KEY, value);
        }

        public string BundleVersion
        {
            get
            {
                if (HasKey(BUNDLE_VERSION_KEY))
                {
                    return (string)Payload.Get(BUNDLE_VERSION_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(BUNDLE_VERSION_KEY, value);
        }

        public string CodeBundleId
        {
            get
            {
                if (HasKey(CODE_BUNDLE_ID))
                {
                    return (string)Payload.Get(CODE_BUNDLE_ID);
                }
                else
                {
                    return null;
                }
            }
            set => Add(CODE_BUNDLE_ID, value);
        }

        public string DsymUuid
        {
            get
            {
                if (HasKey(DSYM_UUID_KEY))
                {
                    return (string)Payload.Get(DSYM_UUID_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(DSYM_UUID_KEY, value);
        }

        public string Id
        {
            get
            {
                if (HasKey(ID_KEY))
                {
                    return (string)Payload.Get(ID_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(ID_KEY, value);
        }

        public string ReleaseStage
        {
            get
            {
                if (HasKey(RELEASESTAGE_KEY))
                {
                    return (string)Payload.Get(RELEASESTAGE_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(RELEASESTAGE_KEY, value);
        }

        public string Type
        {
            get
            {
                if (HasKey(TYPE_KEY))
                {
                    return (string)Payload.Get(TYPE_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(TYPE_KEY, value);
        }

        public string Version
        {
            get
            {
                if (HasKey(VERSION_KEY))
                {
                    return (string)Payload.Get(VERSION_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(VERSION_KEY, value);
        }

        public int? VersionCode
        {
            get
            {
                if (HasKey(VERSION_CODE_KEY))
                {
                    return (int)Payload.Get(VERSION_CODE_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(VERSION_CODE_KEY, value);
        }


        internal App(IConfiguration configuration)
        {

            Id = Application.identifier;
            ReleaseStage = configuration.ReleaseStage;
            Type = GetAppType(configuration);
            Version = configuration.AppVersion;

        }

        private string GetAppType(IConfiguration configuration)
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
