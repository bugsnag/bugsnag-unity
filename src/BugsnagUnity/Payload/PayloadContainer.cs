using System;
using System.Collections.Generic;
#nullable enable

namespace BugsnagUnity.Payload
{

    public class PayloadContainer
    {
        internal PayloadDictionary Payload = new PayloadDictionary();

        internal PayloadContainer()
        {
        }

        internal void Add(string key, object? value)
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

        internal object? Get(string key)
        {
            if (Payload.ContainsKey(key))
            {
                return Payload[key];
            }
            else
            {
                return null;
            }
        }
       
    }

    internal class PayloadDictionary : Dictionary<string,object>, IFilterable
    {

    }
}
