using System;
using System.Collections.Generic;

namespace BugsnagUnity.Payload
{

    // When unity is able to use newer versions of C# we should add a generic Get method here that can return T?, unfortunatly that's not possible right now

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

        internal bool HasKey(string key)
        {
            return Payload.ContainsKey(key);
        }
       
    }
}
