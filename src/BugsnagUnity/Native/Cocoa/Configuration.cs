using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    internal Configuration(string apiKey, bool autoNotify) : base()
    {
      SetupDefaults(apiKey, autoNotify);
    }

    protected override void SetupDefaults(string apiKey, bool autoNotify)
    {
      base.SetupDefaults(apiKey, autoNotify);
    }
  }
}
