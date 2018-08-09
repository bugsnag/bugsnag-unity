using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BugsnagUnity.Payload
{
  /// <summary>
  /// Represents the "device" key in the error report payload.
  /// </summary>
  public class Device : Dictionary<string, object>, IFilterable
  {
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

      var matches = Regex.Match(Environment.OSVersion.VersionString, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
      if (matches.Success)
      {
        this.AddToPayload("osName", matches.Groups["osName"].Value);
        this.AddToPayload("osVersion", matches.Groups["osVersion"].Value);
      }
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
