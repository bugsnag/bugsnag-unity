using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;

public class ComplexMetadataSanitization : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
    }

    public override void Run()
    {
        // Test 1: Generic Dictionary<string, ulong>
        var genericDict = new Dictionary<string, ulong>
        {
            { "small", 100UL },
            { "large", 18446744073709551615UL }  // Max UInt64
        };
        Bugsnag.AddMetadata("generic", "dict", genericDict);

        // Test 2: Non-generic Hashtable
        var hashtable = new Hashtable
        {
            { "stringKey", 12345678901234567890UL },
            { "normalKey", "valueWithStringKey" },
            { "nested", new Dictionary<string, object> 
                { 
                    { "deep", 18446744073709551000UL } 
                } 
            }
        };
        Bugsnag.AddMetadata("nonGeneric", "hashtable", hashtable);

        // Test 3: Custom non-generic dictionary implementation
        var customDict = new CustomStringDictionary
        {
            { "custom1", 18446744073709551615UL },
            { "custom2", "normalValue" }
        };
        Bugsnag.AddMetadata("custom", "dict", customDict);

        // Test 4: Deeply nested structure (4 levels)
        Bugsnag.AddMetadata("nested", new Dictionary<string, object>
        {
            { "level1", new Dictionary<string, object>
                {
                    { "level2", new Dictionary<string, object>
                        {
                            { "level3", new Dictionary<string, object>
                                {
                                    { "level4", 18446744073709551615UL },
                                    { "array", new object[] { 1, 12345678901234567890UL, "test" } }
                                }
                            }
                        }
                    }
                }
            }
        });

        // Test 5: Mixed arrays with dictionaries
        Bugsnag.AddMetadata("arrays", new Dictionary<string, object>
        {
            { "mixedArray", new object[]
                {
                    12345678901234567890UL,
                    new Dictionary<string, object> { { "inArray", 18446744073709551615UL } },
                    new string[] { "str1", "str2" }
                }
            }
        });

        // Test 6: Dictionary with null values and edge cases
        Bugsnag.AddMetadata("edgeCases", new Dictionary<string, object>
        {
            { "nullValue", null },
            { "zeroUlong", 0UL },
            { "maxLong", 9223372036854775807L },  // Just under the limit (valid long)
            { "overMaxLong", 9223372036854775808UL }  // Just over the limit (requires sanitization)
        });

        DoSimpleNotify("ComplexMetadataSanitization");
    }

    // Custom non-generic dictionary that implements IDictionary<string, object>
    private class CustomStringDictionary : IDictionary<string, object>
    {
        private Dictionary<string, object> _inner = new Dictionary<string, object>();

        public object this[string key] 
        { 
            get => _inner[key]; 
            set => _inner[key] = value; 
        }

        public ICollection<string> Keys => _inner.Keys;
        public ICollection<object> Values => _inner.Values;
        public int Count => _inner.Count;
        public bool IsReadOnly => false;

        public void Add(string key, object value) => _inner.Add(key, value);
        public void Add(KeyValuePair<string, object> item) => _inner.Add(item.Key, item.Value);
        public void Clear() => _inner.Clear();
        public bool Contains(KeyValuePair<string, object> item) => _inner.ContainsKey(item.Key);
        public bool ContainsKey(string key) => _inner.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _inner.GetEnumerator();
        public bool Remove(string key) => _inner.Remove(key);
        public bool Remove(KeyValuePair<string, object> item) => _inner.Remove(item.Key);
        public bool TryGetValue(string key, out object value) => _inner.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    }
}
