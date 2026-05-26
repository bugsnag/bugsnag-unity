using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class FeatureFlagTests
    {
        [Test]
        public void Constructor_NameOnly_SetsName_VariantIsNull()
        {
            var flag = new FeatureFlag("my-flag");
            Assert.AreEqual("my-flag", flag.Name);
            Assert.IsNull(flag.Variant);
        }

        [Test]
        public void Constructor_NameAndVariant_SetsBoth()
        {
            var flag = new FeatureFlag("my-flag", "v2");
            Assert.AreEqual("my-flag", flag.Name);
            Assert.AreEqual("v2", flag.Variant);
        }

        [Test]
        public void Name_CanBeChanged()
        {
            var flag = new FeatureFlag("original");
            flag.Name = "updated";
            Assert.AreEqual("updated", flag.Name);
        }

        [Test]
        public void Variant_CanBeSet()
        {
            var flag = new FeatureFlag("flag");
            flag.Variant = "beta";
            Assert.AreEqual("beta", flag.Variant);
        }

        [Test]
        public void Variant_CanBeOverwritten()
        {
            var flag = new FeatureFlag("flag", "v1");
            flag.Variant = "v2";
            Assert.AreEqual("v2", flag.Variant);
        }
    }
}
