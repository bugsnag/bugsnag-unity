using UnityEngine;
using System.Collections.Generic;

namespace Bugsnag.Unity
{
  class UnityMetadata
  {
    internal static Dictionary<string, string> Data => new Dictionary<string, string> {
      { "unityVersion", Application.unityVersion },
      { "platform", Application.platform.ToString() },
      { "osLanguage", Application.systemLanguage.ToString() },
      { "bundleIdentifier", Application.identifier },
      //{ "bundleIdentifier", Application.bundleIdentifier },// this would seem to be the property in older versions of unity
      { "version", Application.version },
      { "companyName", Application.companyName },
      { "productName", Application.productName },
    };
  }
}
