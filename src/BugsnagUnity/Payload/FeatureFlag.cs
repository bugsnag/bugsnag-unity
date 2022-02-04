using System;
namespace BugsnagUnity.Payload
{
    public class FeatureFlag : PayloadContainer
    {
        public FeatureFlag(string name, string variant)
        {
            Name = name;
            Variant = variant;
        }
        public string Name
        {
            get => (string)Get("featureFlag");
            set => Add("featureFlag", value);
        }
        public string Variant
        {
            get => (string)Get("variant");
            set => Add("variant", value);
        }
    }
}
