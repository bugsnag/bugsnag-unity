using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  class Configuration : AbstractConfiguration
  {
    internal Configuration(string apiKey, bool autoNotify) : base() {
      SetupDefaults(apiKey);
      AutoNotify = autoNotify;
    }
  }
}
