using System.Collections.Generic;
using System.Linq;
using BugsnagUnity;
using UnityEngine;

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

        public void AddMetadata(string section, IDictionary<string, object> metadataSection)
        {
            if (metadataSection == null)
            {
                ClearMetadata(section);
                return;
            }
            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Get(section);

                foreach (var key in metadataSection.Keys.ToList())
                {
                    var value = metadataSection[key];
                    if (value == null)
                    {
                        ClearMetadata(section, key);
                    }
                    else
                    {
                        existingSection[key] = value;
                    }
                }
            }
            else
            {
                Add(section,metadataSection);
            }
            if (_nativeClient != null)
            {
                _nativeClient.AddNativeMetadata(section,metadataSection);
            }
        }

       

        public void ClearMetadata(string section)
        {
            if (SectionExists(section))
            {
                Payload.Remove(section);
            }
            if (_nativeClient != null)
            {
                _nativeClient.ClearNativeMetadata(section);
            }
        }

        public void ClearMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Payload[section];
                if (existingSection.ContainsKey(key))
                {
                    existingSection.Remove(key);
                }                
            }
            if (_nativeClient != null)
            {
                _nativeClient.ClearNativeMetadata(section,key);
            }
        }

        private bool SectionExists(string section)
        {
            return Payload.ContainsKey(section);
        }

        public IDictionary<string, object> GetMetadata(string section)
        {
            return SectionExists(section) ? (IDictionary<string, object>)Payload[section] : null;
        }

        public object GetMetadata(string section, string key)
        {
            if (SectionExists(section))
            {
                var existingSection = (IDictionary<string, object>)Payload[section];
                return existingSection[key];
            }
            return null;
        }

        internal void MergeMetadata(IDictionary<string, object> newMetadata)
        {
            if (newMetadata == null)
            {
                return;
            }
            foreach (var section in newMetadata)
            {
                var sectionkey = section.Key;
                var sectionToMergeIn = (IDictionary<string, object>)section.Value;
                AddMetadata(sectionkey, sectionToMergeIn);
            }
        }

    }
}
