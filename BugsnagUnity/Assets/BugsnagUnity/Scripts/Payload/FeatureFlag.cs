using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public class FeatureFlag : PayloadContainer
    {

        internal FeatureFlag(Dictionary<string,object> data)
        {
            Add(data);
        }

        public FeatureFlag(string name)
        {
            Name = name;
        }

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
