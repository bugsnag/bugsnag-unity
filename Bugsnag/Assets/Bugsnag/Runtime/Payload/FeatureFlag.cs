using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public class FeatureFlag : PayloadContainer
    {

        internal FeatureFlag(Dictionary<string, object> data)
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
            get
            {
                // Try new key first, then fall back to deprecated key for backward compatibility
                var name = (string)Get("name");
                if (name == null)
                {
                    name = (string)Get("featureFlag");
                }
                return name;
            }
            set => Add("name", value);
        }

        public string Variant
        {
            get => (string)Get("variant");
            set => Add("variant", value);
        }
    }
}
