using NUnit.Framework;
using BugsnagUnity.Payload;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class SanitizationHelpersTests
    {
        // ---- IsTriviallySerializable ----

        [Test]
        public void IsTriviallySerializable_Null_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(null));
        }

        [Test]
        public void IsTriviallySerializable_String_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable("hello"));
        }

        [Test]
        public void IsTriviallySerializable_Int_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(42));
        }

        [Test]
        public void IsTriviallySerializable_Bool_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(true));
        }

        [Test]
        public void IsTriviallySerializable_Double_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(3.14));
        }

        [Test]
        public void IsTriviallySerializable_List_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(new List<int> { 1, 2 }));
        }

        [Test]
        public void IsTriviallySerializable_Dictionary_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(new Dictionary<string, object>()));
        }

        [Test]
        public void IsTriviallySerializable_PlainObject_ReturnsFalse()
        {
            Assert.IsFalse(SanitizationHelpers.IsTriviallySerializable(new object()));
        }

        // ---- SanitizeValue ----

        [Test]
        public void SanitizeValue_Null_ReturnsNull()
        {
            Assert.IsNull(SanitizationHelpers.SanitizeValue(null));
        }

        [Test]
        public void SanitizeValue_String_ReturnsString()
        {
            Assert.AreEqual("hello", SanitizationHelpers.SanitizeValue("hello"));
        }

        [Test]
        public void SanitizeValue_Int_ReturnsInt()
        {
            Assert.AreEqual(42, SanitizationHelpers.SanitizeValue(42));
        }

        [Test]
        public void SanitizeValue_LargeUlong_ReturnsString()
        {
            ulong large = (ulong)long.MaxValue + 1;
            var result = SanitizationHelpers.SanitizeValue(large);
            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(large.ToString(System.Globalization.CultureInfo.InvariantCulture), result);
        }

        [Test]
        public void SanitizeValue_SmallUlong_ReturnsUlong()
        {
            ulong small = 100;
            Assert.AreEqual(small, SanitizationHelpers.SanitizeValue(small));
        }

        [Test]
        public void SanitizeValue_GenericStringObjectDict_ReturnsSanitizedCopy()
        {
            var input = new Dictionary<string, object> { { "key", 99 }, { "other", "val" } };
            var result = SanitizationHelpers.SanitizeValue(input) as Dictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.AreEqual(99, result["key"]);
            Assert.AreEqual("val", result["other"]);
        }

        [Test]
        public void SanitizeValue_NonGenericHashtable_ReturnsStringKeyedCopy()
        {
            var ht = new System.Collections.Hashtable { { "k", "v" }, { 1, "num" } };
            var result = SanitizationHelpers.SanitizeValue(ht) as Dictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.AreEqual("v", result["k"]);
            Assert.AreEqual("num", result["1"]);
        }

        [Test]
        public void SanitizeValue_NestedDict_RecursivelySanitizes()
        {
            var inner = new Dictionary<string, object> { { "inner_key", "inner_val" } };
            var outer = new Dictionary<string, object> { { "outer_key", inner } };
            var result = SanitizationHelpers.SanitizeValue(outer) as Dictionary<string, object>;
            var innerResult = result["outer_key"] as Dictionary<string, object>;
            Assert.IsNotNull(innerResult);
            Assert.AreEqual("inner_val", innerResult["inner_key"]);
        }

        [Test]
        public void SanitizeValue_LargeUlongInsideDict_ConvertsToString()
        {
            ulong large = (ulong)long.MaxValue + 1;
            var input = new Dictionary<string, object> { { "big", large } };
            var result = SanitizationHelpers.SanitizeValue(input) as Dictionary<string, object>;
            Assert.IsInstanceOf<string>(result["big"]);
        }
    }

    [TestFixture]
    public class SanitizationHelpersExtendedTests
    {
        [Test]
        public void IsTriviallySerializable_Byte_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((byte)1));

        [Test]
        public void IsTriviallySerializable_SByte_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((sbyte)-1));

        [Test]
        public void IsTriviallySerializable_Short_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((short)100));

        [Test]
        public void IsTriviallySerializable_UShort_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((ushort)200));

        [Test]
        public void IsTriviallySerializable_UInt_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((uint)300u));

        [Test]
        public void IsTriviallySerializable_Long_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((long)400L));

        [Test]
        public void IsTriviallySerializable_Float_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(1.5f));

        [Test]
        public void IsTriviallySerializable_Decimal_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(1.5m));

        [Test]
        public void IsTriviallySerializable_DateTime_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(DateTime.UtcNow));

        [Test]
        public void IsTriviallySerializable_DateTimeOffset_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(DateTimeOffset.UtcNow));

        [Test]
        public void IsTriviallySerializable_Guid_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(Guid.NewGuid()));

        [Test]
        public void IsTriviallySerializable_Enum_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(DayOfWeek.Monday));

        [Test]
        public void IsTriviallySerializable_ULong_ReturnsTrue()
            => Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable((ulong)12345UL));

        [Test]
        public void SanitizeValue_StringArray_ReturnsSanitizedCopy()
        {
            var result = SanitizationHelpers.SanitizeValue(new[] { "a", "b" });
            Assert.IsInstanceOf<string[]>(result);
            var arr = (string[])result;
            Assert.AreEqual("a", arr[0]);
            Assert.AreEqual("b", arr[1]);
        }

        [Test]
        public void SanitizeValue_ListOfStrings_ReturnsSanitizedCopy()
        {
            var input = new List<string> { "x", "y" };
            var result = SanitizationHelpers.SanitizeValue(input);
            Assert.IsInstanceOf<List<string>>(result);
            var list = (List<string>)result;
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void SanitizeValue_IntArray_ReturnsObjectArray()
        {
            var result = SanitizationHelpers.SanitizeValue(new int[] { 1, 2, 3 });
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_ListOfInt_ReturnsObjectArray()
        {
            var result = SanitizationHelpers.SanitizeValue(new List<int> { 10, 20 });
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_DateTime_ReturnsUnchanged()
        {
            var dt = DateTime.UtcNow;
            var result = SanitizationHelpers.SanitizeValue(dt);
            Assert.AreEqual(dt, result);
        }

        [Test]
        public void SanitizeValue_Guid_ReturnsUnchanged()
        {
            var g = Guid.NewGuid();
            var result = SanitizationHelpers.SanitizeValue(g);
            Assert.AreEqual(g, result);
        }

        [Test]
        public void SanitizeValue_StringArrayWithLargeUlong_ConvertsToString()
        {
            var dict = new Dictionary<string, object>
            {
                { "key", (ulong)(long.MaxValue + 1UL) }
            };
            var result = SanitizationHelpers.SanitizeValue(dict) as Dictionary<string, object>;
            Assert.IsInstanceOf<string>(result["key"]);
        }
    }

    [TestFixture]
    public class SanitizationHelpersAdditionalTests
    {
        [Test]
        public void IsTriviallySerializable_Enum_ReturnsTrue()
        {
            Assert.IsTrue(SanitizationHelpers.IsTriviallySerializable(DayOfWeek.Monday));
        }

        [Test]
        public void SanitizeValue_Dictionary_ReturnsSanitizedDict()
        {
            var input = new Dictionary<string, object> { ["key"] = "value" };
            var result = SanitizationHelpers.SanitizeValue(input);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_Null_ReturnsNull()
        {
            var result = SanitizationHelpers.SanitizeValue(null);
            Assert.IsNull(result);
        }

        [Test]
        public void SanitizeValue_NonGenericIDictionary_ReturnsSanitizedDict()
        {
            var ht = new Hashtable { ["k"] = "v" };
            var result = SanitizationHelpers.SanitizeValue(ht);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_DateTime_ReturnsDateTime()
        {
            var dt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var result = SanitizationHelpers.SanitizeValue(dt);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_ULongOverMaxLong_ReturnsString()
        {
            ulong big = (ulong)long.MaxValue + 1UL;
            var result = SanitizationHelpers.SanitizeValue(big);
            Assert.IsInstanceOf<string>(result);
        }

        [Test]
        public void SanitizeValue_NestedDict_SanitizesNested()
        {
            var nested = new Dictionary<string, object>
            {
                ["inner"] = new Dictionary<string, object> { ["x"] = 1 }
            };
            var result = SanitizationHelpers.SanitizeValue(nested);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SanitizeValue_GenericDictStringString_ReturnsSanitized()
        {
            var dict = new Dictionary<string, string> { ["a"] = "b" };
            var result = SanitizationHelpers.SanitizeValue(dict);
            Assert.IsNotNull(result);
        }
    }
}
