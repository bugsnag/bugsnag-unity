using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity
{
  public class Configuration : AbstractConfiguration
  {
    public Configuration(string apiKey) : base() {
      SetupDefaults(apiKey);
    }
  }
}
