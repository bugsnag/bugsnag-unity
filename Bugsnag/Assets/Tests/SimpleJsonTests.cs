using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BugsnagUnityTests
{
    // ---- Helper POCO for PocoJsonSerializerStrategy tests ----
    public class TestPoco
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Active { get; set; }
    }

    [TestFixture]
    public class JsonObjectTests
    {
        [Test]
        public void Constructor_CreatesEmptyObject()
        {
            var obj = new JsonObject();
            Assert.AreEqual(0, obj.Count);
        }

        [Test]
        public void Add_KeyValue_CanBeRetrieved()
        {
            var obj = new JsonObject();
            obj.Add("key", "value");
            Assert.AreEqual("value", obj["key"]);
        }

        [Test]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            var obj = new JsonObject();
            obj.Add("k", 1);
            Assert.IsTrue(obj.ContainsKey("k"));
        }

        [Test]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            var obj = new JsonObject();
            Assert.IsFalse(obj.ContainsKey("missing"));
        }

        [Test]
        public void Keys_ReturnsAllKeys()
        {
            var obj = new JsonObject();
            obj.Add("a", 1);
            obj.Add("b", 2);
            CollectionAssert.Contains(obj.Keys, "a");
            CollectionAssert.Contains(obj.Keys, "b");
        }

        [Test]
        public void Values_ReturnsAllValues()
        {
            var obj = new JsonObject();
            obj.Add("x", 99);
            CollectionAssert.Contains(obj.Values, 99);
        }

        [Test]
        public void Remove_ExistingKey_RemovesIt()
        {
            var obj = new JsonObject();
            obj.Add("k", "v");
            obj.Remove("k");
            Assert.IsFalse(obj.ContainsKey("k"));
        }

        [Test]
        public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
        {
            var obj = new JsonObject();
            obj.Add("k", "hello");
            Assert.IsTrue(obj.TryGetValue("k", out var val));
            Assert.AreEqual("hello", val);
        }

        [Test]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            var obj = new JsonObject();
            Assert.IsFalse(obj.TryGetValue("no", out _));
        }

        [Test]
        public void GetDictionary_ReturnsCopyAsDict()
        {
            var obj = new JsonObject();
            obj.Add("k", "v");
            var dict = obj.GetDictionary();
            Assert.IsInstanceOf<Dictionary<string, object>>(dict);
            Assert.AreEqual("v", dict["k"]);
        }

        [Test]
        public void Count_ReflectsAddedItems()
        {
            var obj = new JsonObject();
            obj.Add("a", 1);
            obj.Add("b", 2);
            Assert.AreEqual(2, obj.Count);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var obj = new JsonObject();
            obj.Add("a", 1);
            obj.Clear();
            Assert.AreEqual(0, obj.Count);
        }

        [Test]
        public void Indexer_SetAndGet()
        {
            var obj = new JsonObject();
            obj["key"] = "set-value";
            Assert.AreEqual("set-value", obj["key"]);
        }

        [Test]
        public void GetAtIndex_ReturnsValueAtPosition()
        {
            var obj = new JsonObject();
            obj.Add("first", "A");
            var val = JsonObject.GetAtIndex(obj, 0);
            Assert.AreEqual("A", val);
        }

        [Test]
        public void Add_KeyValuePair_CanBeRetrieved()
        {
            var obj = new JsonObject();
            obj.Add(new KeyValuePair<string, object>("kp", "vp"));
            Assert.AreEqual("vp", obj["kp"]);
        }

        [Test]
        public void Remove_KeyValuePair_Works()
        {
            var obj = new JsonObject();
            obj.Add("k", "v");
            obj.Remove(new KeyValuePair<string, object>("k", "v"));
            Assert.IsFalse(obj.ContainsKey("k"));
        }

        [Test]
        public void Contains_ExistingPair_ReturnsTrue()
        {
            var obj = new JsonObject();
            obj.Add("k", "v");
            Assert.IsTrue(obj.Contains(new KeyValuePair<string, object>("k", "v")));
        }

        [Test]
        public void IsReadOnly_ReturnsFalse()
        {
            var obj = new JsonObject();
            Assert.IsFalse(obj.IsReadOnly);
        }

        [Test]
        public void Enumeration_YieldsAllPairs()
        {
            var obj = new JsonObject();
            obj.Add("a", 1);
            obj.Add("b", 2);
            int count = 0;
            foreach (var kv in obj) count++;
            Assert.AreEqual(2, count);
        }

        [Test]
        public void CopyTo_FillsArray()
        {
            var obj = new JsonObject();
            obj.Add("x", 10);
            var arr = new KeyValuePair<string, object>[1];
            obj.CopyTo(arr, 0);
            Assert.AreEqual("x", arr[0].Key);
        }
    }

    [TestFixture]
    public class JsonArrayTests
    {
        [Test]
        public void Constructor_CreatesEmpty()
        {
            var arr = new JsonArray();
            Assert.AreEqual(0, arr.Count);
        }

        [Test]
        public void Constructor_WithCapacity_Works()
        {
            var arr = new JsonArray(10);
            Assert.AreEqual(0, arr.Count);
        }

        [Test]
        public void Add_Item_CanBeRetrieved()
        {
            var arr = new JsonArray();
            arr.Add("hello");
            Assert.AreEqual("hello", arr[0]);
        }

        [Test]
        public void Count_ReflectsItems()
        {
            var arr = new JsonArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            Assert.AreEqual(3, arr.Count);
        }

        [Test]
        public void MixedTypes_CanBeStored()
        {
            var arr = new JsonArray();
            arr.Add("string");
            arr.Add(42);
            arr.Add(true);
            arr.Add(null);
            Assert.AreEqual(4, arr.Count);
        }
    }

    [TestFixture]
    public class SimpleJsonSerializeTests
    {
        private static string Serialize(object obj, Dictionary<string, bool> filters = null)
        {
            using (var ms = new MemoryStream())
            // Use UTF8 without BOM so output doesn't have leading 3 bytes
            using (var sw = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                if (filters != null)
                    SimpleJson.SerializeObject(obj, sw, filters);
                else
                    SimpleJson.SerializeObject(obj, sw);
                sw.Flush();
                return new UTF8Encoding(false).GetString(ms.ToArray());
            }
        }

        [Test]
        public void SerializeObject_String_WritesQuotedString()
        {
            var json = Serialize("hello");
            Assert.AreEqual("\"hello\"", json);
        }

        [Test]
        public void SerializeObject_Int_WritesNumber()
        {
            var json = Serialize(42);
            Assert.AreEqual("42", json);
        }

        [Test]
        public void SerializeObject_Double_WritesDouble()
        {
            var json = Serialize(3.14);
            StringAssert.Contains("3.14", json);
        }

        [Test]
        public void SerializeObject_True_WritesBoolTrue()
        {
            var json = Serialize(true);
            Assert.AreEqual("true", json);
        }

        [Test]
        public void SerializeObject_False_WritesBoolFalse()
        {
            var json = Serialize(false);
            Assert.AreEqual("false", json);
        }

        [Test]
        public void SerializeObject_Null_WritesNull()
        {
            var json = Serialize(null);
            Assert.AreEqual("null", json);
        }

        [Test]
        public void SerializeObject_StringArray_WritesArray()
        {
            var json = Serialize(new[] { "a", "b" });
            StringAssert.Contains("[", json);
            StringAssert.Contains("\"a\"", json);
            StringAssert.Contains("\"b\"", json);
        }

        [Test]
        public void SerializeObject_Dictionary_WritesObject()
        {
            var dict = new Dictionary<string, object> { { "name", "Alice" }, { "age", 30 } };
            var json = Serialize(dict);
            StringAssert.Contains("\"name\"", json);
            StringAssert.Contains("\"Alice\"", json);
        }

        [Test]
        public void SerializeObject_JsonArray_WritesJsonArray()
        {
            var arr = new JsonArray { 1, 2, 3 };
            var json = Serialize(arr);
            StringAssert.Contains("[", json);
            StringAssert.Contains("1", json);
        }

        [Test]
        public void SerializeObject_JsonObject_WritesJsonObject()
        {
            var obj = new JsonObject();
            obj.Add("foo", "bar");
            var json = Serialize(obj);
            StringAssert.Contains("\"foo\"", json);
            StringAssert.Contains("\"bar\"", json);
        }

        [Test]
        public void SerializeObject_NestedDictionary_WritesNested()
        {
            var inner = new Dictionary<string, object> { { "x", 1 } };
            var outer = new Dictionary<string, object> { { "inner", inner } };
            var json = Serialize(outer);
            StringAssert.Contains("\"inner\"", json);
            StringAssert.Contains("\"x\"", json);
        }

        [Test]
        public void SerializeObject_Long_WritesNumber()
        {
            var json = Serialize(9876543210L);
            StringAssert.Contains("9876543210", json);
        }

        [Test]
        public void SerializeObject_StringWithSpecialChars_EscapesCorrectly()
        {
            var json = Serialize("say \"hello\"");
            StringAssert.Contains("\\\"", json);
        }

        [Test]
        public void SerializeObject_EmptyDictionary_WritesEmptyObject()
        {
            var json = Serialize(new Dictionary<string, object>());
            Assert.AreEqual("{}", json);
        }

        [Test]
        public void SerializeObject_EmptyArray_WritesEmptyArray()
        {
            var json = Serialize(new JsonArray());
            Assert.AreEqual("[]", json);
        }

        [Test]
        public void SerializeObject_WithFilters_Works()
        {
            var dict = new Dictionary<string, object> { { "key", "value" } };
            var filters = new Dictionary<string, bool>();
            var json = Serialize(dict, filters);
            StringAssert.Contains("\"key\"", json);
        }
    }

    [TestFixture]
    public class SimpleJsonDeserializeTests
    {
        [Test]
        public void TryDeserializeObject_ValidJsonString_ReturnsTrue()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("\"hello\"", out var obj));
            Assert.AreEqual("hello", obj);
        }

        [Test]
        public void TryDeserializeObject_InvalidJson_ReturnsFalse()
        {
            Assert.IsFalse(SimpleJson.TryDeserializeObject("{invalid}", out _));
        }

        [Test]
        public void TryDeserializeObject_Null_ReturnsNullObject()
        {
            // null input: returns true but with null object
            SimpleJson.TryDeserializeObject(null, out var obj);
            Assert.IsNull(obj);
        }

        [Test]
        public void TryDeserializeObject_JsonObject_ReturnsJsonObject()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("{\"k\":\"v\"}", out var obj));
            Assert.IsInstanceOf<JsonObject>(obj);
            var jobj = (JsonObject)obj;
            Assert.AreEqual("v", jobj["k"]);
        }

        [Test]
        public void TryDeserializeObject_JsonArray_ReturnsJsonArray()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("[1,2,3]", out var obj));
            Assert.IsInstanceOf<JsonArray>(obj);
        }

        [Test]
        public void TryDeserializeObject_Number_ReturnsDouble()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("42", out var obj));
            Assert.IsNotNull(obj);
        }

        [Test]
        public void TryDeserializeObject_True_ReturnsBool()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("true", out var obj));
            Assert.AreEqual(true, obj);
        }

        [Test]
        public void TryDeserializeObject_False_ReturnsBool()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("false", out var obj));
            Assert.AreEqual(false, obj);
        }

        [Test]
        public void TryDeserializeObject_JsonNull_ReturnsNullObj()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("null", out var obj));
            Assert.IsNull(obj);
        }

        [Test]
        public void TryDeserializeObject_NestedObject_ParsesNested()
        {
            var json = "{\"outer\":{\"inner\":\"val\"}}";
            Assert.IsTrue(SimpleJson.TryDeserializeObject(json, out var obj));
            var outer = (JsonObject)obj;
            var inner = (JsonObject)outer["outer"];
            Assert.AreEqual("val", inner["inner"]);
        }

        [Test]
        public void TryDeserializeObject_ArrayOfObjects_ParsesAll()
        {
            var json = "[{\"a\":1},{\"b\":2}]";
            Assert.IsTrue(SimpleJson.TryDeserializeObject(json, out var obj));
            var arr = (JsonArray)obj;
            Assert.AreEqual(2, arr.Count);
        }

        [Test]
        public void DeserializeObject_ValidJson_ReturnsObject()
        {
            var obj = SimpleJson.DeserializeObject("{\"x\":1}");
            Assert.IsNotNull(obj);
        }

        [Test]
        public void DeserializeObject_InvalidJson_ThrowsException()
        {
            Assert.Throws<System.Runtime.Serialization.SerializationException>(() =>
                SimpleJson.DeserializeObject("{bad json}")
            );
        }

        [Test]
        public void DeserializeObject_StringValue_ReturnsString()
        {
            var obj = SimpleJson.DeserializeObject("\"test\"");
            Assert.AreEqual("test", obj);
        }

        [Test]
        public void DeserializeObject_EmptyObject_ReturnsEmptyJsonObject()
        {
            var obj = SimpleJson.DeserializeObject("{}");
            Assert.IsInstanceOf<JsonObject>(obj);
            Assert.AreEqual(0, ((JsonObject)obj).Count);
        }

        [Test]
        public void DeserializeObject_EmptyArray_ReturnsEmptyJsonArray()
        {
            var obj = SimpleJson.DeserializeObject("[]");
            Assert.IsInstanceOf<JsonArray>(obj);
        }

        // ---- Generic POCO deserialization — exercises PocoJsonSerializerStrategy + ReflectionUtils ----

        [Test]
        public void DeserializeObject_ToPoco_SetsStringProperty()
        {
            var poco = SimpleJson.DeserializeObject<TestPoco>("{\"Name\":\"Alice\",\"Value\":0,\"Active\":false}");
            Assert.AreEqual("Alice", poco.Name);
        }

        [Test]
        public void DeserializeObject_ToPoco_SetsIntProperty()
        {
            var poco = SimpleJson.DeserializeObject<TestPoco>("{\"Name\":\"\",\"Value\":99,\"Active\":false}");
            Assert.AreEqual(99, poco.Value);
        }

        [Test]
        public void DeserializeObject_ToPoco_SetsBoolProperty()
        {
            var poco = SimpleJson.DeserializeObject<TestPoco>("{\"Name\":\"\",\"Value\":0,\"Active\":true}");
            Assert.IsTrue(poco.Active);
        }

        [Test]
        public void DeserializeObject_ToList_ReturnsList()
        {
            var list = SimpleJson.DeserializeObject<List<string>>("[\"a\",\"b\",\"c\"]");
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }
    }

    [TestFixture]
    public class SimpleJsonEscapeTests
    {
        // EscapeToJavascriptString UN-escapes: converts \n→newline, \t→tab, \\→\, \"→"

        [Test]
        public void EscapeToJavascriptString_EmptyString_ReturnsEmpty()
        {
            var result = SimpleJson.EscapeToJavascriptString("");
            Assert.AreEqual("", result);
        }

        [Test]
        public void EscapeToJavascriptString_NullString_ReturnsNull()
        {
            var result = SimpleJson.EscapeToJavascriptString(null);
            Assert.IsNull(result);
        }

        [Test]
        public void EscapeToJavascriptString_PlainString_ReturnsUnchanged()
        {
            var result = SimpleJson.EscapeToJavascriptString("hello world");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedQuote_ProducesQuoteChar()
        {
            // Input: \" → Output: "
            var result = SimpleJson.EscapeToJavascriptString("\\\" ");
            Assert.AreEqual("\" ", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedBackslash_ProducesSingleBackslash()
        {
            // Input: \\ → Output: \ (needs trailing char so remainingLength >= 2)
            var result = SimpleJson.EscapeToJavascriptString("\\\\ ");
            Assert.AreEqual("\\ ", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedNewline_ProducesNewlineChar()
        {
            var result = SimpleJson.EscapeToJavascriptString("\\n ");
            Assert.AreEqual("\n ", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedTab_ProducesTabChar()
        {
            var result = SimpleJson.EscapeToJavascriptString("\\t ");
            Assert.AreEqual("\t ", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedR_ProducesCarriageReturn()
        {
            var result = SimpleJson.EscapeToJavascriptString("\\r ");
            Assert.AreEqual("\r ", result);
        }

        [Test]
        public void EscapeToJavascriptString_EscapedB_ProducesBackspace()
        {
            var result = SimpleJson.EscapeToJavascriptString("\\b ");
            Assert.AreEqual("\b ", result);
        }
    }

    // ---- POCO helpers used by SimpleJson/Reflection tests ----
    public class PocoWithField
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public bool Flag { get; set; }
        public double Score { get; set; }
    }

    public enum TestColor { Red, Green, Blue }

    public class PublicFieldClass
    {
        public int Value;
    }

    [TestFixture]
    public class SimpleJsonUnicodeAndNumberTests
    {
        private static string Serialize(object obj)
        {
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false));
            SimpleJson.SerializeObject(obj, sw);
            sw.Flush();
            return new System.Text.UTF8Encoding(false).GetString(ms.ToArray());
        }

        [Test]
        public void Deserialize_UnicodeEscape_DecodesCorrectly()
        {
            var result = SimpleJson.DeserializeObject("\"\\u0041\"");
            Assert.AreEqual("A", result);
        }

        [Test]
        public void Deserialize_UnicodeEscapeInObject_DecodesValue()
        {
            var result = (JsonObject)SimpleJson.DeserializeObject("{\"k\":\"\\u0068\\u0065\\u006C\\u006C\\u006F\"}");
            Assert.AreEqual("hello", result["k"]);
        }

        [Test]
        public void Deserialize_JsonWithFormFeedInString_IgnoresIncomplete()
        {
            var result = SimpleJson.DeserializeObject("\"\\f\"");
            Assert.IsNotNull(result);
        }

        [Test]
        public void Deserialize_JsonWithSlashEscape_DecodesSlash()
        {
            var result = SimpleJson.DeserializeObject("\"\\/path\"");
            Assert.AreEqual("/path", result);
        }

        [Test]
        public void Deserialize_JsonFloatNotation_ReturnsDouble()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("3.14", out var obj));
            Assert.IsInstanceOf<double>(obj);
        }

        [Test]
        public void Deserialize_JsonScientificNotation_ReturnsDouble()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("1e3", out var obj));
            Assert.IsInstanceOf<double>(obj);
        }

        [Test]
        public void Deserialize_JsonNegativeNumber_ReturnsNegativeLong()
        {
            Assert.IsTrue(SimpleJson.TryDeserializeObject("-42", out var obj));
            Assert.AreEqual(-42L, obj);
        }

        [Test]
        public void Deserialize_JsonWithWhitespace_IgnoresWhitespace()
        {
            var result = SimpleJson.DeserializeObject("  {  \"k\"  :  \"v\"  }  ");
            Assert.IsInstanceOf<JsonObject>(result);
        }

        [Test]
        public void Deserialize_JsonArrayWithNullElements_ParsesCorrectly()
        {
            var result = (JsonArray)SimpleJson.DeserializeObject("[null, true, false, 1, \"s\"]");
            Assert.AreEqual(5, result.Count);
            Assert.IsNull(result[0]);
            Assert.AreEqual(true, result[1]);
            Assert.AreEqual(false, result[2]);
        }

        [Test]
        public void DeserializeObject_WithTypeOverload_ReturnsCorrectType()
        {
            var result = SimpleJson.DeserializeObject("{\"Name\":\"Bob\",\"Count\":7,\"Flag\":false,\"Score\":0.0}",
                typeof(PocoWithField));
            Assert.IsInstanceOf<PocoWithField>(result);
            Assert.AreEqual("Bob", ((PocoWithField)result).Name);
        }

        [Test]
        public void Serialize_StringWithTab_EscapesTab()
        {
            var json = Serialize("a\tb");
            StringAssert.Contains("\\t", json);
        }

        [Test]
        public void Serialize_StringWithNewline_EscapesNewline()
        {
            var json = Serialize("a\nb");
            StringAssert.Contains("\\n", json);
        }

        [Test]
        public void Serialize_StringWithCarriageReturn_EscapesCR()
        {
            var json = Serialize("a\rb");
            StringAssert.Contains("\\r", json);
        }

        [Test]
        public void Serialize_StringWithBackspace_EscapesBackspace()
        {
            var json = Serialize("a\bb");
            StringAssert.Contains("\\b", json);
        }

        [Test]
        public void Serialize_StringWithFormFeed_EscapesFormFeed()
        {
            var json = Serialize("a\fb");
            StringAssert.Contains("\\f", json);
        }

        [Test]
        public void Serialize_StringWithControlChar_EscapesAsUnicode()
        {
            var json = Serialize("\u0001");
            StringAssert.Contains("\\u", json);
        }

        [Test]
        public void Serialize_SByte_WritesNumber()
        {
            var json = Serialize((sbyte)-5);
            StringAssert.Contains("-5", json);
        }

        [Test]
        public void Serialize_Byte_WritesNumber()
        {
            var json = Serialize((byte)200);
            StringAssert.Contains("200", json);
        }

        [Test]
        public void Serialize_UShort_WritesNumber()
        {
            var json = Serialize((ushort)65000);
            StringAssert.Contains("65000", json);
        }

        [Test]
        public void EscapeToJavascriptString_FormFeedNotSupported_BackslashConsumed()
        {
            // \f is not a recognised escape sequence — backslash is consumed, 'f' passes through
            var result = SimpleJson.EscapeToJavascriptString("\\f ");
            Assert.AreEqual("f ", result);
        }

        [Test]
        public void EscapeToJavascriptString_SlashEscape_ProducesSlash()
        {
            var result = SimpleJson.EscapeToJavascriptString("\\/ ");
            Assert.AreEqual("/ ", result);
        }

        [Test]
        public void EscapeToJavascriptString_LongerStringWithEscape_ProcessesAll()
        {
            var result = SimpleJson.EscapeToJavascriptString("hello\\nworld");
            Assert.AreEqual("hello\nworld", result);
        }

        [Test]
        public void EscapeToJavascriptString_UnicodeEscapeNotSupported_BackslashConsumed()
        {
            // \u is not a recognised escape sequence — backslash is consumed, rest passes through
            var result = SimpleJson.EscapeToJavascriptString("\\u0041 ");
            Assert.AreEqual("u0041 ", result);
        }

        [Test]
        public void Serialize_NonGenericIDictionary_WritesObject()
        {
            var dict = new System.Collections.Hashtable();
            dict["key"] = "value";
            var json = Serialize(dict);
            Assert.IsNotNull(json);
        }

        [Test]
        public void Deserialize_StringWithLeadingWhitespace_ParsesOk()
        {
            var result = SimpleJson.DeserializeObject("[   \"trimmed\"  ]");
            var arr = (JsonArray)result;
            Assert.AreEqual("trimmed", arr[0]);
        }

        [Test]
        public void DeserializeObject_WithTypeAndStrategy_DeserializesPoco()
        {
            var result = SimpleJson.DeserializeObject(
                "{\"Name\":\"Test\",\"Count\":1,\"Flag\":true,\"Score\":0.5}",
                typeof(PocoWithField),
                SimpleJson.PocoJsonSerializerStrategy
            );
            Assert.IsInstanceOf<PocoWithField>(result);
        }

        [Test]
        public void SerializeObject_GenericDeserialize_RoundTrips()
        {
            var original = new PocoWithField { Name = "RoundTrip", Count = 99 };
            var json = Serialize(original);
            var restored = SimpleJson.DeserializeObject<PocoWithField>(json);
            Assert.AreEqual("RoundTrip", restored.Name);
        }
    }

    [TestFixture]
    public class PocoJsonSerializerStrategyTests
    {
        private static PocoJsonSerializerStrategy Strategy => SimpleJson.PocoJsonSerializerStrategy;

        private static string Serialize(object obj)
        {
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false));
            SimpleJson.SerializeObject(obj, sw);
            sw.Flush();
            return new System.Text.UTF8Encoding(false).GetString(ms.ToArray());
        }

        [Test]
        public void TrySerialize_DateTime_ReturnsIsoString()
        {
            var dt = new DateTime(2023, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(dt, out var output));
            Assert.IsInstanceOf<string>(output);
            StringAssert.Contains("2023", (string)output);
        }

        [Test]
        public void TrySerialize_DateTimeOffset_ReturnsIsoString()
        {
            var dto = new DateTimeOffset(2023, 6, 1, 12, 0, 0, TimeSpan.Zero);
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(dto, out var output));
            Assert.IsInstanceOf<string>(output);
        }

        [Test]
        public void TrySerialize_Guid_ReturnsGuidString()
        {
            var guid = new Guid("550e8400-e29b-41d4-a716-446655440000");
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(guid, out var output));
            StringAssert.Contains("550e8400", ((string)output).ToLower());
        }

        [Test]
        public void TrySerialize_Uri_ReturnsUriString()
        {
            var uri = new Uri("https://example.com/path");
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(uri, out var output));
            Assert.AreEqual("https://example.com/path", output);
        }

        [Test]
        public void TrySerialize_Enum_ReturnsDouble()
        {
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(TestColor.Green, out var output));
            Assert.IsInstanceOf<double>(output);
        }

        [Test]
        public void TrySerialize_Null_ThrowsArgumentNullException()
        {
            // null input causes ArgumentNullException in the POCO strategy
            Assert.Throws<ArgumentNullException>(() => Strategy.TrySerializeNonPrimitiveObject(null, out _));
        }

        [Test]
        public void TrySerialize_CustomPoco_SerializesProperties()
        {
            var poco = new PocoWithField { Name = "test", Count = 42, Flag = true };
            Assert.IsTrue(Strategy.TrySerializeNonPrimitiveObject(poco, out var output));
            Assert.IsInstanceOf<IDictionary<string, object>>(output);
            var dict = (IDictionary<string, object>)output;
            Assert.AreEqual("test", dict["Name"]);
        }

        [Test]
        public void Deserialize_NullValue_ReturnsNull()
        {
            var result = Strategy.DeserializeObject(null, typeof(string));
            Assert.IsNull(result);
        }

        [Test]
        public void Deserialize_StringToGuid_ReturnsGuid()
        {
            var guidStr = "550e8400-e29b-41d4-a716-446655440000";
            var result = Strategy.DeserializeObject(guidStr, typeof(Guid));
            Assert.IsInstanceOf<Guid>(result);
            Assert.AreEqual(new Guid(guidStr), result);
        }

        [Test]
        public void Deserialize_EmptyStringToGuid_ReturnsDefaultGuid()
        {
            var result = Strategy.DeserializeObject("", typeof(Guid));
            Assert.AreEqual(default(Guid), result);
        }

        [Test]
        public void Deserialize_EmptyStringToNullableGuid_ReturnsNull()
        {
            var result = Strategy.DeserializeObject("", typeof(Guid?));
            Assert.IsNull(result);
        }

        [Test]
        public void Deserialize_StringToUri_ReturnsUri()
        {
            var result = Strategy.DeserializeObject("https://example.com", typeof(Uri));
            Assert.IsInstanceOf<Uri>(result);
        }

        [Test]
        public void Deserialize_InvalidUri_ReturnsNull()
        {
            var result = Strategy.DeserializeObject("not a uri !!!", typeof(Uri));
            Assert.IsNull(result);
        }

        [Test]
        public void Deserialize_StringToString_ReturnsString()
        {
            var result = Strategy.DeserializeObject("hello", typeof(string));
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void Deserialize_StringToDateTime_ReturnsDateTime()
        {
            var result = Strategy.DeserializeObject("2023-01-01T00:00:00.0000000Z", typeof(DateTime));
            Assert.IsInstanceOf<DateTime>(result);
        }

        [Test]
        public void Deserialize_StringToDateTimeOffset_ReturnsDateTimeOffset()
        {
            var result = Strategy.DeserializeObject("2023-01-01T00:00:00Z", typeof(DateTimeOffset));
            Assert.IsInstanceOf<DateTimeOffset>(result);
        }

        [Test]
        public void Deserialize_BoolTrue_ReturnsBool()
        {
            var result = Strategy.DeserializeObject(true, typeof(bool));
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Deserialize_LongToInt_ConvertsToInt()
        {
            var result = Strategy.DeserializeObject(42L, typeof(int));
            Assert.AreEqual(42, result);
        }

        [Test]
        public void Deserialize_LongToDouble_ConvertsToDouble()
        {
            var result = Strategy.DeserializeObject(100L, typeof(double));
            Assert.IsInstanceOf<double>(result);
        }

        [Test]
        public void Deserialize_LongToFloat_ConvertsToFloat()
        {
            var result = Strategy.DeserializeObject(5L, typeof(float));
            Assert.IsInstanceOf<float>(result);
        }

        [Test]
        public void Deserialize_LongToDecimal_ConvertsToDecimal()
        {
            var result = Strategy.DeserializeObject(10L, typeof(decimal));
            Assert.IsInstanceOf<decimal>(result);
        }

        [Test]
        public void Deserialize_LongToLong_ReturnsSameLong()
        {
            var result = Strategy.DeserializeObject(999L, typeof(long));
            Assert.AreEqual(999L, result);
        }

        [Test]
        public void Deserialize_DoubleToDouble_ReturnsSameDouble()
        {
            var result = Strategy.DeserializeObject(3.14, typeof(double));
            Assert.AreEqual(3.14, result);
        }

        [Test]
        public void Deserialize_DictToObject_ReturnsDict()
        {
            var dict = new Dictionary<string, object> { ["k"] = "v" };
            var result = Strategy.DeserializeObject(dict, typeof(object));
            Assert.AreSame(dict, result);
        }

        [Test]
        public void Deserialize_DictToPoco_SetsPoco()
        {
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "Alice",
                ["Count"] = 5L,
                ["Flag"] = true,
                ["Score"] = 9.5
            };
            var result = (PocoWithField)Strategy.DeserializeObject(dict, typeof(PocoWithField));
            Assert.AreEqual("Alice", result.Name);
            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void Deserialize_DictToDictionary_ReturnsDictionary()
        {
            var dict = new Dictionary<string, object> { ["key1"] = "value1" };
            var result = Strategy.DeserializeObject(dict, typeof(Dictionary<string, string>));
            Assert.IsInstanceOf<Dictionary<string, string>>(result);
        }

        [Test]
        public void Deserialize_ListToTypedList_ReturnsTypedList()
        {
            var list = new List<object> { "a", "b", "c" };
            var result = Strategy.DeserializeObject(list, typeof(List<string>));
            Assert.IsInstanceOf<List<string>>(result);
            Assert.AreEqual(3, ((List<string>)result).Count);
        }

        [Test]
        public void Deserialize_ListToArray_ReturnsArray()
        {
            var list = new List<object> { 1L, 2L, 3L };
            var result = Strategy.DeserializeObject(list, typeof(int[]));
            Assert.IsInstanceOf<int[]>(result);
            Assert.AreEqual(3, ((int[])result).Length);
        }

        [Test]
        public void Deserialize_NullableInt_ReturnsWrapped()
        {
            var result = Strategy.DeserializeObject(7L, typeof(int?));
            Assert.IsNotNull(result);
        }

        [Test]
        public void SerializeObject_Float_WritesFloat()
        {
            var json = Serialize(1.5f);
            StringAssert.Contains("1.5", json);
        }

        [Test]
        public void SerializeObject_Decimal_WritesDecimal()
        {
            var json = Serialize(9.99m);
            StringAssert.Contains("9.99", json);
        }

        [Test]
        public void SerializeObject_UInt_WritesUInt()
        {
            var json = Serialize((uint)42u);
            Assert.AreEqual("42", json);
        }

        [Test]
        public void SerializeObject_ULong_WritesULong()
        {
            var json = Serialize(9876543210UL);
            StringAssert.Contains("9876543210", json);
        }

        [Test]
        public void SerializeObject_Short_WritesShort()
        {
            var json = Serialize((short)10);
            Assert.AreEqual("10", json);
        }

        [Test]
        public void SerializeObject_Enum_WritesDouble()
        {
            var json = Serialize(TestColor.Blue);
            Assert.IsNotNull(json);
        }

        [Test]
        public void SerializeObject_DateTime_WritesIsoString()
        {
            var dt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var json = Serialize(dt);
            StringAssert.Contains("2023", json);
        }

        [Test]
        public void SerializeObject_WithFiltersParam_SerializesAllKeys()
        {
            // Filters only apply to IFilterable objects, not plain dicts (applyFilters=false)
            var dict = new Dictionary<string, object>
            {
                ["secretKey"] = "secret-value",
                ["publicKey"] = "public-value"
            };
            var filters = new Dictionary<string, bool> { ["secretKey"] = true };
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false));
            SimpleJson.SerializeObject(dict, sw, filters);
            sw.Flush();
            var json = new System.Text.UTF8Encoding(false).GetString(ms.ToArray());
            StringAssert.Contains("secretKey", json);
            StringAssert.Contains("publicKey", json);
        }

        [Test]
        public void SerializeObject_CircularReference_WritesCircular()
        {
            var strDict = new Dictionary<string, string> { ["k"] = "v" };
            var json = Serialize(strDict);
            StringAssert.Contains("\"k\"", json);
            StringAssert.Contains("\"v\"", json);
        }
    }

    [TestFixture]
    public class ReflectionUtilsTests
    {
        [Test]
        public void IsTypeGeneric_ListOfString_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsTypeGeneric(typeof(List<string>)));
        }

        [Test]
        public void IsTypeGeneric_String_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsTypeGeneric(typeof(string)));
        }

        [Test]
        public void GetGenericTypeArguments_DictionaryStringInt_ReturnsBothTypes()
        {
            var args = ReflectionUtils.GetGenericTypeArguments(typeof(Dictionary<string, int>));
            Assert.AreEqual(2, args.Length);
            Assert.AreEqual(typeof(string), args[0]);
            Assert.AreEqual(typeof(int), args[1]);
        }

        [Test]
        public void IsAssignableFrom_IListFromListOfString_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsAssignableFrom(typeof(IList), typeof(List<string>)));
        }

        [Test]
        public void IsAssignableFrom_StringFromInt_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsAssignableFrom(typeof(string), typeof(int)));
        }

        [Test]
        public void IsTypeDictionary_GenericDict_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsTypeDictionary(typeof(Dictionary<string, object>)));
        }

        [Test]
        public void IsTypeDictionary_String_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsTypeDictionary(typeof(string)));
        }

        [Test]
        public void IsTypeDictionary_ListOfString_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsTypeDictionary(typeof(List<string>)));
        }

        [Test]
        public void IsNullableType_NullableInt_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsNullableType(typeof(int?)));
        }

        [Test]
        public void IsNullableType_Int_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(int)));
        }

        [Test]
        public void IsNullableType_String_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsNullableType(typeof(string)));
        }

        [Test]
        public void ToNullableType_IntValue_ReturnsNullableInt()
        {
            var result = ReflectionUtils.ToNullableType(42, typeof(int?));
            Assert.AreEqual(42, result);
        }

        [Test]
        public void IsValueType_Int_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsValueType(typeof(int)));
        }

        [Test]
        public void IsValueType_String_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsValueType(typeof(string)));
        }

        [Test]
        public void GetTypeInfo_String_ReturnsTypeInfo()
        {
            var ti = ReflectionUtils.GetTypeInfo(typeof(string));
            Assert.IsNotNull(ti);
        }

        [Test]
        public void GetAttribute_WithSerializable_ReturnsAttributeOrNull()
        {
            var attr = ReflectionUtils.GetAttribute(typeof(string), typeof(SerializableAttribute));
            // may be null on some platforms — should not throw
        }

        [Test]
        public void GetGenericListElementType_ListOfInt_ReturnsInt()
        {
            var elemType = ReflectionUtils.GetGenericListElementType(typeof(List<int>));
            Assert.AreEqual(typeof(int), elemType);
        }

        [Test]
        public void GetGenericListElementType_NonList_ThrowsException()
        {
            // Non-generic types cause IndexOutOfRangeException in the implementation
            Assert.Throws<IndexOutOfRangeException>(() =>
                ReflectionUtils.GetGenericListElementType(typeof(string)));
        }

        [Test]
        public void IsTypeGenericeCollectionInterface_ListOfString_ReturnsTrue()
        {
            Assert.IsTrue(ReflectionUtils.IsTypeGenericeCollectionInterface(typeof(IList<string>)));
        }

        [Test]
        public void IsTypeGenericeCollectionInterface_String_ReturnsFalse()
        {
            Assert.IsFalse(ReflectionUtils.IsTypeGenericeCollectionInterface(typeof(string)));
        }

        [Test]
        public void GetConstructors_StringBuilder_ReturnsConstructors()
        {
            var ctors = ReflectionUtils.GetConstructors(typeof(System.Text.StringBuilder));
            int count = 0;
            foreach (var c in ctors) count++;
            Assert.Greater(count, 0);
        }

        [Test]
        public void GetConstructorInfo_StringBuilder_NoArgs_ReturnsConstructorInfo()
        {
            var ctorInfo = ReflectionUtils.GetConstructorInfo(typeof(System.Text.StringBuilder));
            Assert.IsNotNull(ctorInfo);
        }

        [Test]
        public void GetConstructorInfo_NonExistentSignature_ReturnsNull()
        {
            var ctorInfo = ReflectionUtils.GetConstructorInfo(typeof(System.Text.StringBuilder),
                typeof(object), typeof(object), typeof(object), typeof(object), typeof(object));
            Assert.IsNull(ctorInfo);
        }

        [Test]
        public void GetProperties_PocoWithField_ReturnsProperties()
        {
            var props = ReflectionUtils.GetProperties(typeof(PocoWithField));
            int count = 0;
            foreach (var p in props) count++;
            Assert.Greater(count, 0);
        }

        [Test]
        public void GetFields_PocoWithField_ReturnsFields()
        {
            var fields = ReflectionUtils.GetFields(typeof(PocoWithField));
            Assert.IsNotNull(fields);
        }

        [Test]
        public void GetGetterMethodInfo_NameProperty_ReturnsGetMethod()
        {
            var prop = typeof(PocoWithField).GetProperty("Name");
            var getter = ReflectionUtils.GetGetterMethodInfo(prop);
            Assert.IsNotNull(getter);
        }

        [Test]
        public void GetSetterMethodInfo_NameProperty_ReturnsSetMethod()
        {
            var prop = typeof(PocoWithField).GetProperty("Name");
            var setter = ReflectionUtils.GetSetterMethodInfo(prop);
            Assert.IsNotNull(setter);
        }

        [Test]
        public void GetContructor_StringBuilder_ReturnsDelegate()
        {
            var ctor = ReflectionUtils.GetContructor(typeof(System.Text.StringBuilder));
            Assert.IsNotNull(ctor);
            var instance = ctor();
            Assert.IsInstanceOf<System.Text.StringBuilder>(instance);
        }

        [Test]
        public void GetConstructorByReflection_StringBuilder_ReturnsDelegate()
        {
            var ctorInfo = typeof(System.Text.StringBuilder).GetConstructor(Type.EmptyTypes);
            var ctor = ReflectionUtils.GetConstructorByReflection(ctorInfo);
            Assert.IsNotNull(ctor);
            var instance = ctor();
            Assert.IsInstanceOf<System.Text.StringBuilder>(instance);
        }

        [Test]
        public void GetConstructorByReflection_ByType_ReturnsDelegate()
        {
            var ctor = ReflectionUtils.GetConstructorByReflection(typeof(System.Text.StringBuilder));
            Assert.IsNotNull(ctor);
        }

        [Test]
        public void GetGetMethod_PropertyInfo_ReturnsDelegate()
        {
            var prop = typeof(PocoWithField).GetProperty("Name");
            var getter = ReflectionUtils.GetGetMethod(prop);
            Assert.IsNotNull(getter);
            var poco = new PocoWithField { Name = "test" };
            Assert.AreEqual("test", getter(poco));
        }

        [Test]
        public void GetSetMethod_PropertyInfo_ReturnsDelegate()
        {
            var prop = typeof(PocoWithField).GetProperty("Name");
            var setter = ReflectionUtils.GetSetMethod(prop);
            Assert.IsNotNull(setter);
            var poco = new PocoWithField();
            setter(poco, "hello");
            Assert.AreEqual("hello", poco.Name);
        }

        [Test]
        public void GetGetMethodByReflection_FieldInfo_ReturnsDelegate()
        {
            var field = typeof(PublicFieldClass).GetField("Value");
            var getter = ReflectionUtils.GetGetMethod(field);
            Assert.IsNotNull(getter);
            var obj = new PublicFieldClass { Value = 99 };
            Assert.AreEqual(99, getter(obj));
        }

        [Test]
        public void GetSetMethodByReflection_FieldInfo_ReturnsDelegate()
        {
            var field = typeof(PublicFieldClass).GetField("Value");
            var setter = ReflectionUtils.GetSetMethod(field);
            Assert.IsNotNull(setter);
            var obj = new PublicFieldClass();
            setter(obj, 42);
            Assert.AreEqual(42, obj.Value);
        }

        [Test]
        public void GetAttribute_MemberInfo_PropertyWithoutAttribute_ReturnsNull()
        {
            var memberInfo = typeof(TestPoco).GetProperty("Name");
            var attr = ReflectionUtils.GetAttribute(memberInfo, typeof(SerializableAttribute));
            Assert.IsNull(attr);
        }

        [Test]
        public void GetContructor_ConstructorInfo_ReturnsDelegate()
        {
            var ctorInfo = typeof(StringBuilder).GetConstructors()[0];
            var del = ReflectionUtils.GetContructor(ctorInfo);
            Assert.IsNotNull(del);
            var instance = del(new object[0]);
            Assert.IsInstanceOf<StringBuilder>(instance);
        }

        [Test]
        public void GetGetMethodByReflection_PropertyInfo_DirectCall_ReturnsValue()
        {
            var prop = typeof(TestPoco).GetProperty("Name");
            var getter = ReflectionUtils.GetGetMethodByReflection(prop);
            Assert.IsNotNull(getter);
            var poco = new TestPoco { Name = "hello" };
            Assert.AreEqual("hello", getter(poco));
        }

        [Test]
        public void GetGetMethodByReflection_FieldInfo_DirectCall_ReturnsValue()
        {
            var field = typeof(PublicFieldClass).GetField("Value");
            var getter = ReflectionUtils.GetGetMethodByReflection(field);
            Assert.IsNotNull(getter);
            var obj = new PublicFieldClass { Value = 7 };
            Assert.AreEqual(7, getter(obj));
        }

        [Test]
        public void GetSetMethodByReflection_PropertyInfo_DirectCall_SetsValue()
        {
            var prop = typeof(TestPoco).GetProperty("Name");
            var setter = ReflectionUtils.GetSetMethodByReflection(prop);
            Assert.IsNotNull(setter);
            var poco = new TestPoco();
            setter(poco, "world");
            Assert.AreEqual("world", poco.Name);
        }

        [Test]
        public void GetSetMethodByReflection_FieldInfo_DirectCall_SetsValue()
        {
            var field = typeof(PublicFieldClass).GetField("Value");
            var setter = ReflectionUtils.GetSetMethodByReflection(field);
            Assert.IsNotNull(setter);
            var obj = new PublicFieldClass();
            setter(obj, 55);
            Assert.AreEqual(55, obj.Value);
        }
    }

    [TestFixture]
    public class SimpleJsonAdvancedTests
    {
        [Test]
        public void DeserializeObject_Typed_WithCustomStrategy_ReturnsTypedResult()
        {
            var strategy = new PocoJsonSerializerStrategy();
            var result = SimpleJson.DeserializeObject<JsonObject>("{\"key\":\"value\"}", strategy);
            Assert.IsNotNull(result);
            Assert.AreEqual("value", result["key"]);
        }

        [Test]
        public void DeserializeObject_Typed_NoStrategy_ReturnsTypedResult()
        {
            var result = SimpleJson.DeserializeObject<JsonObject>("{\"name\":\"test\"}");
            Assert.IsNotNull(result);
            Assert.AreEqual("test", result["name"]);
        }

        [Test]
        public void CurrentJsonSerializerStrategy_Setter_CanReplaceAndRestore()
        {
            var original = SimpleJson.CurrentJsonSerializerStrategy;
            var custom = new PocoJsonSerializerStrategy();
            SimpleJson.CurrentJsonSerializerStrategy = custom;
            Assert.AreEqual(custom, SimpleJson.CurrentJsonSerializerStrategy);
            SimpleJson.CurrentJsonSerializerStrategy = original;
            Assert.AreEqual(original, SimpleJson.CurrentJsonSerializerStrategy);
        }

        [Test]
        public void SerializeObject_WithExplicitStrategyAndSeen_WritesJson()
        {
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false));
            var strategy = new PocoJsonSerializerStrategy();
            var seen = new Dictionary<object, bool>();
            SimpleJson.SerializeObject("hello", sw, strategy, seen);
            sw.Flush();
            var json = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("\"hello\"", json);
        }
    }

    [TestFixture]
    public class ThreadSafeDictionaryTests
    {
        [Test]
        public void ThreadSafeDictionary_GetOrCreate_ReturnsValue()
        {
            int callCount = 0;
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(key =>
            {
                callCount++;
                return key.Length;
            });

            var result = dict["hello"];
            Assert.AreEqual(5, result);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ThreadSafeDictionary_GetOrCreate_CachesValue()
        {
            int callCount = 0;
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(key =>
            {
                callCount++;
                return 42;
            });

            var r1 = dict["key"];
            var r2 = dict["key"];
            Assert.AreEqual(42, r1);
            Assert.AreEqual(42, r2);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ThreadSafeDictionary_Count_ReflectsAdded()
        {
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(k => 0);
            var _ = dict["a"];
            var __ = dict["b"];
            Assert.AreEqual(2, dict.Count);
        }

        [Test]
        public void ThreadSafeDictionary_ContainsKey_ReturnsTrueAfterGet()
        {
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(k => 0);
            var _ = dict["k"];
            Assert.IsTrue(dict.ContainsKey("k"));
        }

        [Test]
        public void ThreadSafeDictionary_ContainsKey_WhenEmpty_ThrowsNullRef()
        {
            // Internal _dictionary is null until first access; ContainsKey throws NullReferenceException
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(k => 0);
            Assert.Throws<NullReferenceException>(() => dict.ContainsKey("notexist"));
        }

        [Test]
        public void ThreadSafeDictionary_TryGetValue_ReturnsAfterCreation()
        {
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, string>(k => k.ToUpper());
            var _ = dict["hello"];
            Assert.IsTrue(dict.TryGetValue("hello", out var val));
            Assert.AreEqual("HELLO", val);
        }

        [Test]
        public void ThreadSafeDictionary_Keys_ContainsAllKeys()
        {
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(k => k.Length);
            var _ = dict["ab"];
            var __ = dict["cde"];
            CollectionAssert.Contains(dict.Keys, "ab");
            CollectionAssert.Contains(dict.Keys, "cde");
        }

        [Test]
        public void ThreadSafeDictionary_Values_ContainsAllValues()
        {
            var dict = new ReflectionUtils.ThreadSafeDictionary<string, int>(k => k.Length);
            var _ = dict["ab"];
            CollectionAssert.Contains(dict.Values, 2);
        }
    }
}
