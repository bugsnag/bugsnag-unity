using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    
    public class Device : PayloadContainer
    {

        private const string BROWSER_NAME_KEY = "browserName";
        private const string BROWSER_VERSION_KEY = "browserVersion";
        private const string CPU_ABI_KEY = "cpuAbi";
        private const string HOSTNAME_KEY = "hostName";
        private const string ID_KEY = "id";
        private const string JAILBROKEN_KEY = "jailbroken";
        private const string LOCALE_KEY = "locale";
        private const string MANUFACTURER_KEY = "manufacturer";
        private const string MODEL_KEY = "model";
        private const string MODEL_NUMBER_KEY = "modelNumber";
        private const string OS_NAME_KEY = "osName";
        private const string OS_VERSION_KEY = "osVersion";
        private const string RUNTIME_VERSIONS_KEY = "runtimeVersions";
        private const string TOTAL_MEMORY_KEY = "totalMemory";
        private const string USER_AGENT_KEY = "userAgent";
        private const string SCREEN_DENSITY_KEY = "screenDensity";
        private const string SCREEN_RESOLUTION_KEY = "screenResolution";

        //Player prefs id for Bugsnag generated device id
        private const string GENERATED_ID_KEY = "GENERATED_ID_KEY";

        //public mutable values exposed in callbacks
        public string BrowserName
        {
            get
            {
                if (HasKey(BROWSER_NAME_KEY))
                {
                    return (string)Payload.Get(BROWSER_NAME_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(BROWSER_NAME_KEY, value);
        }

        public string BrowserVersion
        {
            get
            {
                if (HasKey(BROWSER_VERSION_KEY))
                {
                    return (string)Payload.Get(BROWSER_VERSION_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(BROWSER_VERSION_KEY, value);
        }

        public string[] CpuAbi
        {
            get
            {
                if (HasKey(CPU_ABI_KEY))
                {
                    return (string[])Payload.Get(CPU_ABI_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(CPU_ABI_KEY, value);
        }

        public string HostName
        {
            get
            {
                if (HasKey(HOSTNAME_KEY))
                {
                    return Payload.Get(HOSTNAME_KEY) as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add(HOSTNAME_KEY, value);
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

        public bool? JailBroken
        {
            get
            {
                if (HasKey(JAILBROKEN_KEY))
                {
                    return (bool)Payload.Get(JAILBROKEN_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(JAILBROKEN_KEY, value);
        }

        public string Locale
        {
            get
            {
                if (HasKey(LOCALE_KEY))
                {
                    return (string)Payload.Get(LOCALE_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(LOCALE_KEY, value);
        }

        public string Manufacturer
        {
            get
            {
                if (HasKey(MANUFACTURER_KEY))
                {
                    return (string)Payload.Get(MANUFACTURER_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(MANUFACTURER_KEY, value);
        }

        public string Model
        {
            get
            {
                if (HasKey(MODEL_KEY))
                {
                    return (string)Payload.Get(MODEL_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(MODEL_KEY, value);
        }

        public string ModelNumber
        {
            get
            {
                if (HasKey(MODEL_NUMBER_KEY))
                {
                    return (string)Payload.Get(MODEL_NUMBER_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(MODEL_NUMBER_KEY, value);
        }

        public string OsName
        {
            get
            {
                if (HasKey(OS_NAME_KEY))
                {
                    return (string)Payload.Get(OS_NAME_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(OS_NAME_KEY, value);
        }

        public string OsVersion
        {
            get
            {
                if (HasKey(OS_VERSION_KEY))
                {
                    return (string)Payload.Get(OS_VERSION_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(OS_VERSION_KEY, value);
        }

        public Dictionary<string, object> RuntimeVersions
        {
            get
            {
                if (HasKey(RUNTIME_VERSIONS_KEY))
                {
                    return (Dictionary<string,object>)Payload.Get(RUNTIME_VERSIONS_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(RUNTIME_VERSIONS_KEY, value);
        }

        public int? TotalMemory
        {
            get
            {
                if (HasKey(TOTAL_MEMORY_KEY))
                {
                    return (int)Payload.Get(TOTAL_MEMORY_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(TOTAL_MEMORY_KEY, value);
        }

        public string UserAgent
        {
            get
            {
                if (HasKey(USER_AGENT_KEY))
                {
                    return (string)Payload.Get(USER_AGENT_KEY);
                }
                else
                {
                    return null;
                }
            }
            set => Add(USER_AGENT_KEY, value);
        }


        internal Device(IConfiguration configuration)
        {
            //callback mutable values
            HostName = configuration.HostName;
            TotalMemory = SystemInfo.systemMemorySize;
            Locale = CultureInfo.CurrentCulture.ToString();
            AddOsInfo();
            AddRuntimeVersions(configuration);
            if (configuration.GenerateAnonymousId)
            {
                Id = GetGeneratedDeviceId();
            }
            Model = SystemInfo.deviceModel;

            //hidden non-mutable values
            Add(SCREEN_DENSITY_KEY, Screen.dpi);
            var res = Screen.currentResolution;
            Add(SCREEN_RESOLUTION_KEY, string.Format("{0}x{1}", res.width, res.height));
        }

        private string GetGeneratedDeviceId()
        {
            if (!PlayerPrefs.HasKey(GENERATED_ID_KEY))
            {
                PlayerPrefs.SetString(GENERATED_ID_KEY, Guid.NewGuid().ToString());
            }
            return PlayerPrefs.GetString(GENERATED_ID_KEY);
        }

        private void AddOsInfo()
        {
            // we expect that windows version strings look like:
            // "Microsoft Windows NT 10.0.17134.0"
            // if it does then we can parse out the version number into a separate field
            var matches = Regex.Match(Environment.OSVersion.VersionString, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
            if (matches.Success)
            {
                OsName = matches.Groups["osName"].Value;
                OsVersion = matches.Groups["osVersion"].Value;
            }
            else
            {
                OsName = Environment.OSVersion.Platform.ToString();
                OsVersion = Environment.OSVersion.VersionString;
            }

        }

        private void AddRuntimeVersions(IConfiguration configuration)
        {
            RuntimeVersions = new Dictionary<string, object>();
            RuntimeVersions.AddToPayload("unityScriptingBackend", configuration.ScriptingBackend);
            RuntimeVersions.AddToPayload("dotnetScriptingRuntime", configuration.DotnetScriptingRuntime);
            RuntimeVersions.AddToPayload("dotnetApiCompatibility", configuration.DotnetApiCompatibility);
            RuntimeVersions.AddToPayload("unity", Application.unityVersion);
        }

    }
}
