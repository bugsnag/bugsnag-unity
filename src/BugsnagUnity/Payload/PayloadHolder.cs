using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{
    public class PayloadHolder
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

        internal bool HasKey(string key)
        {
            return Payload.ContainsKey(key);
        }
       
    }
}
