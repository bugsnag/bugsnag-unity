using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnity.Tests
{
  public class TestConfiguration : AbstractConfiguration
  {
    internal TestConfiguration(string apiKey) : base() {
      SetupDefaults(apiKey, true);
    }
  }
}
