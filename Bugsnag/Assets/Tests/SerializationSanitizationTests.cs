using System;
using System.Collections.Generic;
using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class SerializationSanitizationTests
    {
        [Test]
        public void Metadata_AddMetadata_ShouldAddWarningForUnserializableValues()
        {
            var metadata = new Metadata();
            var bad = new Action(() => { });
            var section = "testSection";
            var input = new Dictionary<string, object> { { "bad", (object)bad } };

            metadata.AddMetadata(section, input);

            var stored = metadata.GetMetadata(section);
            Assert.IsNotNull(stored, "Section should exist");
            Assert.IsTrue(stored.ContainsKey("__bugsnag_unserializable_values"), "Warnings key should be present");
            var warnings = stored["__bugsnag_unserializable_values"] as List<string>;
            Assert.IsNotNull(warnings, "Warnings should be a list of strings");
            Assert.IsTrue(warnings.Count >= 1, "There should be at least one warning");
            StringAssert.Contains("bad", warnings[0]);
        }

        [Test]
        public void Breadcrumb_MetadataSetter_ShouldAddWarningForUnserializableValues()
        {
            var bad = new Action(() => { });
            var metadata = new Dictionary<string, object> { { "bad", (object)bad } };

            var crumb = new Breadcrumb("m", metadata, BreadcrumbType.Manual);

            var stored = crumb.Metadata;
            Assert.IsNotNull(stored, "Breadcrumb metadata should exist");
            Assert.IsTrue(stored.ContainsKey("__bugsnag_unserializable_values"), "Breadcrumb warnings key should be present");
            var warnings = stored["__bugsnag_unserializable_values"] as List<string>;
            Assert.IsNotNull(warnings, "Breadcrumb warnings should be a list of strings");
            Assert.IsTrue(warnings.Count >= 1, "There should be at least one breadcrumb warning");
            StringAssert.Contains("bad", warnings[0]);
        }
    }
}
