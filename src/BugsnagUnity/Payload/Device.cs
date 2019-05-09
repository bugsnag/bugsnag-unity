using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using BugsnagUnity;

namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents the "device" key in the error report payload.
  /// </summary>
  public class Device : Dictionary<string, object>, IFilterable
  {
    private static string UnityVersion;
     internal static void InitUnityVersion() {
       if (UnityVersion == null) {
         UnityVersion = Application.unityVersion;
       }
    }
    internal Device() : this(Hostname)
    {
    }

    internal Device(string hostname)
    {
      this.AddToPayload("hostname", hostname);
      this.AddToPayload("locale", CultureInfo.CurrentCulture.ToString());
      this.AddToPayload("timezone", TimeZone.CurrentTimeZone.StandardName);
      this.AddToPayload("osName", OsName);
      this.AddToPayload("time", DateTime.UtcNow);

      // we expect that windows version strings look like:
      // "Microsoft Windows NT 10.0.17134.0"
      // if it does then we can parse out the version number into a separate field
      var matches = Regex.Match(Environment.OSVersion.VersionString, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
      if (matches.Success)
      {
        this.AddToPayload("osName", matches.Groups["osName"].Value);
        this.AddToPayload("osVersion", matches.Groups["osVersion"].Value);
      }

      var versions = new Dictionary<string, object>() {
        {"unity", UnityVersion}
      };
      this.AddToPayload("runtimeVersions", versions);
    }

    internal void AddRuntimeVersions(IConfiguration config) {
      var versions = (Dictionary<string, object>) this.Get("runtimeVersions");
      versions.AddToPayload("unityScriptingBackend", config.ScriptingBackend);
      versions.AddToPayload("dotnetScriptingRuntime", config.DotnetScriptingRuntime);
      versions.AddToPayload("dotnetApiCompatibility", config.DotnetApiCompatibility);
    }

    /// <summary>
    /// Resolve the hostname using either "COMPUTERNAME" (win) or "HOSTNAME" (*nix) environment variable.
    /// </summary>
    private static string Hostname
    {
      get
      {
        return Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
      }
    }

    private static string OsName
    {
      get
      {
        return Environment.OSVersion.VersionString;
      }
    }
  }
}
