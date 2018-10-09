using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace BugsnagUnity.Tests
{
  [TestFixture]
  public class PostProcessBuildTests
  {
    [TestCase("one")]
    [TestCase("two")]
    [TestCase("three")]
    public void Test(string fileIdentifier)
    {
      string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      directory = Path.Combine(directory, "ProjectFixtures");
      var input = new LinkedList<string>(File.ReadAllLines(Path.Combine(directory, $"test_{fileIdentifier}_input.pbxproj")));
      var output = new LinkedList<string>(File.ReadAllLines(Path.Combine(directory, $"test_{fileIdentifier}_output.pbxproj")));


      PostProcessBuild.Apply(input, "186208CC13E64B42A13CCD74");

      Assert.AreEqual(output, input);
    }
  }
}
