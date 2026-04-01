using System.Collections.Generic;
using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests.SerializationSanitizationTests
{
    public class Breadcrumb_MetadataSetter_ShouldAddWarningForUnserializableValues
    {
        private class UnserializableObj
        {
            public override string ToString()
            {
                return "UnserializableObject";
            }
        }

        [Test]
        public void MetadataSetterAddsWarningAndConvertsValueToString()
        {
            var breadcrumb = new Breadcrumb("test", new Dictionary<string, object>(), BreadcrumbType.Manual);

            var metadata = new Dictionary<string, object>
            {
                { "ok", "value" },
                { "bad", new UnserializableObj() }
            };

            breadcrumb.Metadata = metadata;

            var stored = breadcrumb.Metadata;

            Assert.AreEqual("value", stored["ok"]);
            Assert.IsInstanceOf<string>(stored["bad"]);
            Assert.AreEqual("UnserializableObject", stored["bad"]);

            const string warnKey = "__bugsnag_unserializable_values";
            Assert.IsTrue(stored.ContainsKey(warnKey));
            var warnings = stored[warnKey] as System.Collections.IEnumerable;
            Assert.IsNotNull(warnings);
        }
    }
}
