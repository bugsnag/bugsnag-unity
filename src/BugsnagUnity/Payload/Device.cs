using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
#nullable enable


namespace BugsnagUnity.Payload
{

    public class Device : PayloadContainer, IDevice
    {

        private const string BROWSER_NAME_KEY = "browserName";
        private const string BROWSER_VERSION_KEY = "browserVersion";
        private const string CPU_ABI_KEY = "cpuAbi";
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


        public string? BrowserName
        {
            get => (string?)Get(BROWSER_NAME_KEY);
            set => Add(BROWSER_NAME_KEY, value);
        }

        public string? BrowserVersion
        {
            get => (string?)Get(BROWSER_VERSION_KEY);
            set => Add(BROWSER_VERSION_KEY, value);
        }

        public string[]? CpuAbi
        {
            get => (string[]?)Get(CPU_ABI_KEY);
            set => Add(CPU_ABI_KEY, value);
        }

        public string? Id
        {
            get => (string?)Get(ID_KEY);
            set => Add(ID_KEY, value);
        }

        public bool? Jailbroken
        {
            get => (bool?)Get(JAILBROKEN_KEY);
            set => Add(JAILBROKEN_KEY, value);
        }

        public string? Locale
        {
            get => (string?)Get(LOCALE_KEY);
            set => Add(LOCALE_KEY, value);
        }

        public string? Manufacturer
        {
            get => (string?)Get(MANUFACTURER_KEY);
            set => Add(MANUFACTURER_KEY, value);
        }

        public string? Model
        {
            get => (string?)Get(MODEL_KEY);
            set => Add(MODEL_KEY, value);
        }

        public string? ModelNumber
        {
            get => (string?)Get(MODEL_NUMBER_KEY);
            set => Add(MODEL_NUMBER_KEY, value);
        }

        public string? OsName
        {
            get => (string?)Get(OS_NAME_KEY);
            set => Add(OS_NAME_KEY, value);
        }

        public string? OsVersion
        {
            get => (string?)Get(OS_VERSION_KEY);
            set => Add(OS_VERSION_KEY, value);
        }

        public IDictionary<string, object>? RuntimeVersions
        {
            get => (IDictionary<string, object>?)Get(RUNTIME_VERSIONS_KEY);
            set => Add(RUNTIME_VERSIONS_KEY, value);
        }

        public long? TotalMemory
        {
            get => (long?)Get(TOTAL_MEMORY_KEY);
            set => Add(TOTAL_MEMORY_KEY, value);
        }

        public string? UserAgent
        {
            get => (string?)Get(USER_AGENT_KEY);
            set => Add(USER_AGENT_KEY, value);
        }

        internal Device(Dictionary<string, object> cachedData)
        {
            Add(cachedData);
        }

        internal Device(Configuration configuration)
        {
            TotalMemory = SystemInfo.systemMemorySize;
            Locale = CultureInfo.CurrentCulture.ToString();
            AddOsInfo();
            AddRuntimeVersions(configuration);
            Id = FileManager.GetDeviceId();
            Model = SystemInfo.deviceModel;
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

        private void AddRuntimeVersions(Configuration configuration)
        {
            RuntimeVersions = new Dictionary<string, object>();
            RuntimeVersions.AddToPayload("unityScriptingBackend", configuration.ScriptingBackend);
            RuntimeVersions.AddToPayload("dotnetScriptingRuntime", configuration.DotnetScriptingRuntime);
            RuntimeVersions.AddToPayload("dotnetApiCompatibility", configuration.DotnetApiCompatibility);
            RuntimeVersions.AddToPayload("unity", Application.unityVersion);
        }

    }
}
