using System.Collections.Generic;
using BugsnagUnity;

namespace BugsnagUnity.Payload
{
    public class Metadata : PayloadContainer
    {
        private INativeClient _nativeClient = null;

        public Metadata()
        {
        }

        internal Metadata(INativeClient nativeClient)
        {
            _nativeClient = nativeClient;
        }


        public void AddMetadata(string section, string key, object value)
        {
            AddMetadata(section, new Dictionary<string, object>{{ key, value }});
        }

        public void AddMetadata(string section, Dictionary<string, object> metadataSection)
        {
            if (SectionExists(section))
            {
                var existingSection = (Dictionary<string, object>)Payload[section];
                foreach (var entry in metadataSection)
                {
                    if (entry.Value == null)
                    {
                        if (existingSection.ContainsKey(entry.Key))
                        {
                            existingSection.Remove(entry.Key);
                        }
                    }
                    else
                    {
                        existingSection[entry.Key] = entry.Value;
                    }
                }
            }
            else
            {
                Payload.Add(section,metadataSection);
            }
            UpdateNativeMetadata(section);
        }

        private void UpdateNativeMetadata(string section)
        {
            var existingSection = (Dictionary<string, object>)Payload[section];

            var stringDict = new Dictionary<string, string>();
            foreach (var pair in existingSection)
            {
                stringDict.Add(pair.Key, pair.Value.ToString());
            }
            _nativeClient.SetMetadata(section, stringDict);
        }

        public void ClearMetadata(string section)
        {
            if (SectionExists(section))
            {
                Payload.Remove(section);
                _nativeClient.SetMetadata(section, null);
            }
        }

        public void ClearMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (Dictionary<string, object>)Payload[section];
                if (existingSection.ContainsKey(key))
                {
                    existingSection.Remove(key);
                    UpdateNativeMetadata(section);
                }                
            }
        }

        private bool SectionExists(string section)
        {
            return Payload.ContainsKey(section);
        }

        public Dictionary<string, object> GetMetadata(string section)
        {
            return SectionExists(section) ? (Dictionary<string, object>)Payload[section] : null;
        }

        public object GetMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (Dictionary<string, object>)Payload[section];
                return existingSection[key];
            }
            return null;
        }

        internal void MergeMetadata(Dictionary<string, object> newMetadata)
        {
            foreach (var section in newMetadata)
            {
                var existingSection = (Dictionary<string,object>)newMetadata[section.Key];
                AddMetadata(section.Key, existingSection);
            }
        }

    }
}
