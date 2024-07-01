using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugsnagUnity.Tests
{
    [TestClass]
    public class PostProcessBuildTests
    {
        [DataTestMethod]
        [DataRow("one")]
        [DataRow("two")]
        [DataRow("three")]
        public void Test(string fileIdentifier)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "ProjectFixtures");
            var input = new LinkedList<string>(File.ReadAllLines(Path.Combine(directory, $"test_{fileIdentifier}_input.pbxproj")));
            var output = new LinkedList<string>(File.ReadAllLines(Path.Combine(directory, $"test_{fileIdentifier}_output.pbxproj")));

            PostProcessBuild.Apply(input, "186208CC13E64B42A13CCD74");

            CollectionAssert.AreEqual(new List<string>(output), new List<string>(input));
        }
    }
}
