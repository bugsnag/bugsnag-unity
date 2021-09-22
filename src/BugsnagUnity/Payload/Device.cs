using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BugsnagUnity.Payload
{
    
    public class Device : PayloadHolder
    {

        private IConfiguration _configuration;
        
        public ulong? FreeDisk
        {
            get {
                if (HasKey("freeDisk"))
                {
                    return (ulong)Payload.Get("freeDisk");
                }
                else
                {
                    return null;
                }                
            }
            set => Add("freeDisk", value);
        }

        public ulong? FreeMemory
        {
            get
            {
                if (HasKey("freeMemory"))
                {
                    return (ulong)Payload.Get("freeMemory");
                }
                else
                {
                    return null;
                }
            }
            set => Add("freeMemory", value);
        }

        public string Id
        {
            get
            {
                if (HasKey("id"))
                {
                    return Payload.Get("id") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("id", value);
        }

        public bool? JailBroken
        {
            get
            {
                if (HasKey("jailBroken"))
                {
                    return (bool)Payload.Get("jailBroken");
                }
                else
                {
                    return null;
                }
            }
            set => Add("jailBroken", value);
        }

        public string Locale
        {
            get
            {
                if (HasKey("locale"))
                {
                    return Payload.Get("locale") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("locale", value);
        }

        public string Manufacturer
        {
            get
            {
                if (HasKey("manufacturer"))
                {
                    return Payload.Get("manufacturer") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("manufacturer", value);
        }

        public string Model
        {
            get
            {
                if (HasKey("model"))
                {
                    return Payload.Get("model") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("model", value);
        }

        public string ModelNumber
        {
            get
            {
                if (HasKey("modelNumber"))
                {
                    return Payload.Get("modelNumber") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("modelNumber", value);
        }

        public string Orientation
        {
            get
            {
                if (HasKey("orientation"))
                {
                    return Payload.Get("orientation") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("orientation", value);
        }

        public string OsName
        {
            get
            {
                if (HasKey("osName"))
                {
                    return Payload.Get("osName") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("osName", value);
        }

        public string OsVersion
        {
            get
            {
                if (HasKey("osVersion"))
                {
                    return Payload.Get("osVersion") as string;
                }
                else
                {
                    return null;
                }
            }
            set => Add("osVersion", value);
        }

        public Dictionary<string, object> RuntimeVersions
        {
            get
            {
                if (HasKey("runtimeVersions"))
                {
                    return (Dictionary<string,object>)Payload.Get("runtimeVersions");
                }
                else
                {
                    return null;
                }
            }
            set => Add("runtimeVersions", value);
        }

        public DateTime? Time
        {
            get
            {
                if (HasKey("time"))
                {
                    return (DateTime)Payload.Get("time");
                }
                else
                {
                    return null;
                }
            }
            set => Add("time", value);
        }

        public int? TotalMemory
        {
            get
            {
                if (HasKey("totalMemory"))
                {
                    return (int)Payload.Get("totalMemory");
                }
                else
                {
                    return null;
                }
            }
            set => Add("TotalMemory", value);
        }

        internal Device(IConfiguration configuration)
        {

            _configuration = configuration;

            TotalMemory = SystemInfo.systemMemorySize;

            Locale = CultureInfo.CurrentCulture.ToString();

            Time = DateTime.UtcNow;

            AddBatteryLevel();

            AddOsInfo();

            AddRuntimeVersions();

            Add("charging", SystemInfo.batteryStatus.Equals(BatteryStatus.Charging));
            Add("screenDensity", Screen.dpi);
            var res = Screen.currentResolution;
            Add("screenResolution", string.Format("{0}x{1}", res.width, res.height));

            
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

        private void AddBatteryLevel()
        {
            if (SystemInfo.batteryLevel > -1)
            {
                Add("batteryLevel",SystemInfo.batteryLevel);
            }
        }

        private void AddRuntimeVersions()
        {
            RuntimeVersions = new Dictionary<string, object>();
            RuntimeVersions.AddToPayload("unityScriptingBackend", _configuration.ScriptingBackend);
            RuntimeVersions.AddToPayload("dotnetScriptingRuntime", _configuration.DotnetScriptingRuntime);
            RuntimeVersions.AddToPayload("dotnetApiCompatibility", _configuration.DotnetApiCompatibility);
            RuntimeVersions.AddToPayload("unity", Application.unityVersion);
        }


    }
}
