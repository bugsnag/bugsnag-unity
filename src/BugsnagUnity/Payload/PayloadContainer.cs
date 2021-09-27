using System;
using System.Collections.Generic;
#nullable enable

namespace BugsnagUnity.Payload
{

    public class PayloadContainer
    {
        internal Dictionary<string, object> Payload = new Dictionary<string, object>();

        internal void Add(string key, object value)
        {
            Payload.AddToPayload(key, value);
        }

        internal void Add(Dictionary<string, object> newValues)
        {
            foreach (var entry in newValues)
            {
                Add(entry.Key, entry.Value);
            }
        }

        public bool HasKey(string key)
        {
            return Payload.ContainsKey(key);
        }

        internal object? Get(string key)
        {
            if (HasKey(key))
            {
                return Payload[key];
            }
            else
            {
                return null;
            }
        }
       
    }
}
