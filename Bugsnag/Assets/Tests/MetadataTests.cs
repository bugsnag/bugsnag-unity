using NUnit.Framework;
using BugsnagUnity.Payload;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void AddMetadata_KeyValue_StoresValue()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key", "value");
            var result = metadata.GetMetadata("section") as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.AreEqual("value", result["key"]);
        }

        [Test]
        public void AddMetadata_KeyValue_OverwritesExisting()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key", "first");
            metadata.AddMetadata("section", "key", "second");
            var result = metadata.GetMetadata("section") as IDictionary<string, object>;
            Assert.AreEqual("second", result["key"]);
        }

        [Test]
        public void AddMetadata_Dictionary_StoresAllKeys()
        {
            var metadata = new Metadata();
            var dict = new Dictionary<string, object> { { "k1", "v1" }, { "k2", 42 } };
            metadata.AddMetadata("section", dict);
            var result = metadata.GetMetadata("section") as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.AreEqual("v1", result["k1"]);
            Assert.AreEqual(42, result["k2"]);
        }

        [Test]
        public void AddMetadata_NullDictionary_ClearsSection()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key", "value");
            metadata.AddMetadata("section", (IDictionary<string, object>)null);
            Assert.IsNull(metadata.GetMetadata("section"));
        }

        [Test]
        public void AddMetadata_NullValue_RemovesKey()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key1", "val1");
            metadata.AddMetadata("section", "key2", "val2");
            metadata.AddMetadata("section", "key1", null);
            var result = metadata.GetMetadata("section") as IDictionary<string, object>;
            Assert.IsFalse(result.ContainsKey("key1"));
            Assert.AreEqual("val2", result["key2"]);
        }

        [Test]
        public void GetMetadata_NonExistentSection_ReturnsNull()
        {
            var metadata = new Metadata();
            Assert.IsNull(metadata.GetMetadata("missing"));
        }

        [Test]
        public void ClearMetadata_Section_RemovesSection()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key", "value");
            metadata.ClearMetadata("section");
            Assert.IsNull(metadata.GetMetadata("section"));
        }

        [Test]
        public void ClearMetadata_NonExistentSection_DoesNotThrow()
        {
            var metadata = new Metadata();
            Assert.DoesNotThrow(() => metadata.ClearMetadata("missing"));
        }

        [Test]
        public void ClearMetadata_SectionAndKey_RemovesSingleKey()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section", "key1", "val1");
            metadata.AddMetadata("section", "key2", "val2");
            metadata.ClearMetadata("section", "key1");
            var result = metadata.GetMetadata("section") as IDictionary<string, object>;
            Assert.IsFalse(result.ContainsKey("key1"));
            Assert.AreEqual("val2", result["key2"]);
        }

        [Test]
        public void ClearMetadata_KeyInNonExistentSection_DoesNotThrow()
        {
            var metadata = new Metadata();
            Assert.DoesNotThrow(() => metadata.ClearMetadata("missing", "key"));
        }

        [Test]
        public void MergeMetadata_MergesAllSections()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("section1", "key1", "val1");
            var toMerge = new Dictionary<string, object>
            {
                { "section1", new Dictionary<string, object> { { "key2", "val2" } } },
                { "section2", new Dictionary<string, object> { { "keyA", "valA" } } }
            };
            metadata.MergeMetadata(toMerge);
            var s1 = metadata.GetMetadata("section1") as IDictionary<string, object>;
            var s2 = metadata.GetMetadata("section2") as IDictionary<string, object>;
            Assert.AreEqual("val1", s1["key1"]);
            Assert.AreEqual("val2", s1["key2"]);
            Assert.AreEqual("valA", s2["keyA"]);
        }

        [Test]
        public void MergeMetadata_NullArg_DoesNotThrow()
        {
            var metadata = new Metadata();
            Assert.DoesNotThrow(() => metadata.MergeMetadata(null));
        }

        [Test]
        public void AddMetadata_MultipleSections_AreIndependent()
        {
            var metadata = new Metadata();
            metadata.AddMetadata("sectionA", "key", "A");
            metadata.AddMetadata("sectionB", "key", "B");
            var a = metadata.GetMetadata("sectionA") as IDictionary<string, object>;
            var b = metadata.GetMetadata("sectionB") as IDictionary<string, object>;
            Assert.AreEqual("A", a["key"]);
            Assert.AreEqual("B", b["key"]);
        }
    }
}
