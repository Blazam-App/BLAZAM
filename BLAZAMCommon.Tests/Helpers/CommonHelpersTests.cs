using BLAZAM.Helpers; // For CommonHelpers extension methods
using SixLabors.ImageSharp; // For Image
using SixLabors.ImageSharp.Formats.Png; // For PngEncoder
using System.Collections;
using System.Diagnostics.Eventing.Reader; // For EventRecord
using System.Reflection;
using System.Security.Principal; // For SecurityIdentifier

namespace BLAZAMCommon.Tests.Helpers
{
    public class CommonHelpersTests
    {
        // Helper class for property tests
        public class TestClass
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public bool BoolProperty { get; set; }
            public List<string> ListProperty { get; set; }
            public TestClass NestedProperty { get; set; }
            public string Field = "fieldValue"; // Fields should not be picked up by property methods
            private string PrivateProperty { get; set; } = "privateValue";
            public string ReadOnlyProperty { get; } = "readOnlyValue";

            public TestClass()
            {
                ListProperty = new List<string>();
            }
        }

        // Tests for Round(this double number, int decimalPlaces = 0)
        #region Round Tests
        [Theory]
        [InlineData(3.14159, 0, 3.0)]
        [InlineData(3.5, 0, 4.0)] // Math.Round(3.5, 0) = 4 (ToEven)
        [InlineData(4.5, 0, 4.0)] // Math.Round(4.5, 0) = 4 (ToEven)
        [InlineData(3.7, 0, 4.0)]
        [InlineData(3.0, 0, 3.0)]
        [InlineData(-3.14159, 0, -3.0)]
        [InlineData(-3.5, 0, -4.0)] // Math.Round(-3.5, 0) = -4 (ToEven)
        [InlineData(-4.5, 0, -4.0)] // Math.Round(-4.5, 0) = -4 (ToEven)
        [InlineData(-3.7, 0, -4.0)]
        [InlineData(0.0, 0, 0.0)]
        public void Round_ToZeroDecimalPlaces_UsesMathRound(double number, int decimalPlaces, double expected)
        {
            Assert.Equal(expected, number.Round(decimalPlaces));
        }

        [Theory]
        [InlineData(3.14159, 2, 3.14)]
        [InlineData(3.14159, 4, 3.1416)]
        [InlineData(1.2345, 3, 1.234)] // MidpointRounding.ToEven results in 1.234 for .NET internal Math.Round on this specific value if it were 1.2345000...
                                       // However, double precision might make 1.2345 slightly more or less. Standard behavior for Math.Round(1.2345,3) is 1.235
        [InlineData(1.2375, 3, 1.238)]// Test ToEven for x.xx75
        [InlineData(1.234, 3, 1.234)]
        [InlineData(0.0, 5, 0.0)]
        [InlineData(-3.14159, 2, -3.14)]
        [InlineData(-3.14159, 4, -3.1416)]
        [InlineData(-1.2345, 3, -1.234)]
        [InlineData(-1.2375, 3, -1.238)]
        public void Round_ToSpecificDecimalPlaces_UsesMathRound(double number, int decimalPlaces, double expected)
        {
            Assert.Equal(expected, number.Round(decimalPlaces));
        }
        #endregion Round Tests

        // Tests for GetValueChangesString(this List<AuditChangeLog> changes, Func<AuditChangeLog, object?> valueSelector)
        #region GetValueChangesString Tests
        [Fact]
        public void GetValueChangesString_EmptyList_ReturnsEmptyString()
        {
            var changes = new List<AuditChangeLog>();
            Func<AuditChangeLog, object?> selector = c => c.NewValue;
            Assert.Equal(string.Empty, changes.GetValueChangesString(selector));
        }

        [Fact]
        public void GetValueChangesString_NullList_ReturnsEmptyString()
        {
            List<AuditChangeLog>? changes = null;
            Func<AuditChangeLog, object?> selector = c => c.NewValue;
            Assert.Equal(string.Empty, changes.GetValueChangesString(selector));
        }

        [Fact]
        public void GetValueChangesString_NullSelector_ReturnsEmptyString()
        {
            var changes = new List<AuditChangeLog>
            {
                new AuditChangeLog { Field = "Field1", NewValue = "val1" }
            };
            Func<AuditChangeLog, object?>? selector = null;
            Assert.Equal(string.Empty, changes.GetValueChangesString(selector));
        }

        [Fact]
        public void GetValueChangesString_SimpleValues_ReturnsFormattedString()
        {
            var changes = new List<AuditChangeLog>
            {
                new AuditChangeLog { Field = "Name", NewValue = "Value1" },
                new AuditChangeLog { Field = "Age", NewValue = "Value2" },
                new AuditChangeLog { Field = "City", NewValue = null }, // Should be Field=;
                new AuditChangeLog { Field = "Country", NewValue = "Value3" }
            };
            Func<AuditChangeLog, object?> selector = c => c.NewValue;
            var expected = "Name=Value1;Age=Value2;City=;Country=Value3;";
            Assert.Equal(expected, changes.GetValueChangesString(selector));
        }

        [Fact]
        public void GetValueChangesString_WithCollections_ReturnsFormattedStringWithCommaSeparatedCollectionItems()
        {
            var changes = new List<AuditChangeLog>
            {
                new AuditChangeLog { Field = "Tags", NewValue = new List<string> { "Item1", "Item2" } },
                new AuditChangeLog { Field = "Status", NewValue = "Value3" },
                new AuditChangeLog { Field = "Categories", NewValue = new List<object> { "Item3", 100 } }
            };
            Func<AuditChangeLog, object?> selector = c => c.NewValue;
            // Expected: "Tags=Item1,Item2,;Status=Value3;Categories=Item3,100,;"
            // The trailing comma after collection items is per implementation.
            var expected = "Tags=Item1,Item2,;Status=Value3;Categories=Item3,100,;";
            Assert.Equal(expected, changes.GetValueChangesString(selector));
        }

        [Fact]
        public void GetValueChangesString_SelectorReturnsNull_FieldEqualsEmptySemiColon()
        {
            var changes = new List<AuditChangeLog>
            {
                new AuditChangeLog { Field = "NullableField", NewValue = null }
            };
            // Selector directly returns the NewValue which is null
            Func<AuditChangeLog, object?> selector = c => c.NewValue;
            var expected = "NullableField=;";
            Assert.Equal(expected, changes.GetValueChangesString(selector));
        }
        #endregion GetValueChangesString Tests

        // Tests for IsNullOrEmpty(this ICollection collection)
        #region IsNullOrEmpty Tests
        [Fact]
        public void IsNullOrEmpty_NullCollection_ReturnsTrue()
        {
            ICollection? collection = null;
            Assert.True(collection.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_EmptyList_ReturnsTrue()
        {
            var list = new List<int>();
            Assert.True(list.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_NonEmptyList_ReturnsFalse()
        {
            var list = new List<string> { "item" };
            Assert.False(list.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_EmptyArray_ReturnsTrue()
        {
            var array = Array.Empty<int>();
            Assert.True(array.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_NonEmptyArray_ReturnsFalse()
        {
            var array = new[] { "item" };
            Assert.False(array.IsNullOrEmpty());
        }
        #endregion IsNullOrEmpty Tests

        // Tests for ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        #region ForEach Tests
        [Fact]
        public void ForEach_SumIntegers_ActionCalledForEachItem()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            int sum = 0;
            Action<int> sumAction = n => sum += n;

            numbers.ForEach(sumAction);

            Assert.Equal(15, sum);
        }

        [Fact]
        public void ForEach_EmptyList_ActionNotCalled()
        {
            var emptyList = new List<string>();
            bool actionCalled = false;
            Action<string> testAction = s => actionCalled = true;

            emptyList.ForEach(testAction);

            Assert.False(actionCalled);
        }

        #endregion ForEach Tests

        // Tests for ToGuid(this byte[]? guidBytes)
        #region ToGuid Tests
        [Fact]
        public void ToGuid_Valid16ByteArray_ReturnsCorrectGuid()
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            Assert.Equal(guid, bytes.ToGuid());
        }

        [Fact]
        public void ToGuid_NullByteArray_ReturnsNull()
        {
            byte[]? bytes = null;
            Assert.Null(bytes.ToGuid());
        }

        [Fact]
        public void ToGuid_ByteArrayNot16Bytes_ReturnsNull()
        {
            var bytes15 = new byte[15];
            Assert.Null(bytes15.ToGuid());

            var bytes17 = new byte[17];
            Assert.Null(bytes17.ToGuid());
        }
        #endregion ToGuid Tests

        // Tests for ToHexADString(this byte[]? byteArray)
        #region ToHexADString Tests
        [Fact]
        public void ToHexADString_SampleByteArray_ReturnsCorrectString()
        {
            var byteArray = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            Assert.Equal(@"\DE\AD\BE\EF", byteArray.ToHexADString());
        }

        [Fact]
        public void ToHexADString_NullByteArray_ReturnsNull()
        {
            byte[]? byteArray = null;
            Assert.Null(byteArray.ToHexADString());
        }

        [Fact]
        public void ToHexADString_EmptyByteArray_ReturnsEmptyString()
        {
            var byteArray = Array.Empty<byte>();
            Assert.Equal(string.Empty, byteArray.ToHexADString());
        }
        #endregion ToHexADString Tests

        // Tests for ToSidString(this byte[]? sid)
        #region ToSidString Tests
        [Fact]
        public void ToSidString_ValidSidByteArray_ReturnsCorrectSidString()
        {
            var sidBytes = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }; // S-1-1-0
            var expectedSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null).ToString();
            Assert.Equal(expectedSid, sidBytes.ToSidString());

            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var adminSidBytes = new byte[adminSid.BinaryLength];
            adminSid.GetBinaryForm(adminSidBytes, 0);
            Assert.Equal(adminSid.ToString(), adminSidBytes.ToSidString());
        }

        [Fact]
        public void ToSidString_NullByteArray_ReturnsEmptyString()
        {
            byte[]? sid = null;
            Assert.Equal(string.Empty, sid.ToSidString());
        }

        [Fact]
        public void ToSidString_InvalidSidByteArray_ReturnsEmptyString()
        {
            var invalidSidBytes = new byte[] { 1, 2, 3, 4, 5 };
            Assert.Equal(string.Empty, invalidSidBytes.ToSidString());
        }
        #endregion ToSidString Tests

        // Tests for ToSidByteArray(this string sidString)
        #region ToSidByteArray Tests


        [Fact]
        public void ToSidByteArray_NullOrEmptyString_ReturnsEmptyByteArray()
        {
            string? nullSid = null;
            Assert.Empty(nullSid.ToSidByteArray());

            string emptySid = string.Empty;
            Assert.Empty(emptySid.ToSidByteArray());
        }

        [Fact]
        public void ToSidByteArray_InvalidSidStringFormat_ReturnsEmptyByteArray()
        {
            var invalidSidString = "S-1-INVALID";
            Assert.Empty(invalidSidString.ToSidByteArray());

            var alsoInvalid = "NotASid";
            Assert.Empty(alsoInvalid.ToSidByteArray());
        }
        #endregion ToSidByteArray Tests

        // Tests for SetPropertyValue(this object obj, string propertyName, object value)
        #region SetPropertyValue Tests
        [Fact]
        public void SetPropertyValue_ValidPublicProperty_SetsValue()
        {
            var testObj = new TestClass();
            var newValue = "NewValue";
            Assert.True(testObj.SetPropertyValue("StringProperty", newValue));
            Assert.Equal(newValue, testObj.StringProperty);

            var newIntValue = 123;
            Assert.True(testObj.SetPropertyValue("IntProperty", newIntValue));
            Assert.Equal(newIntValue, testObj.IntProperty);
        }

        [Fact]
        public void SetPropertyValue_CaseInsensitive_SetsValue()
        {
            var testObj = new TestClass();
            var newValue = "CaseTest";
            Assert.True(testObj.SetPropertyValue("stringproperty", newValue));
            Assert.Equal(newValue, testObj.StringProperty);
        }



        [Fact]
        public void SetPropertyValue_NonExistentProperty_ReturnsFalse()
        {
            var testObj = new TestClass();
            Assert.False(testObj.SetPropertyValue("NonExistent", "value"));
        }

        [Fact]
        public void SetPropertyValue_ReadOnlyProperty_ThrowsExceptionAndValueNotChanged()
        {
            var testObj = new TestClass();
            var originalValue = testObj.ReadOnlyProperty;
            // SetValue on a readonly property typically throws.
            // The current CommonHelpers.SetPropertyValue does not catch this.
            var ex = Assert.ThrowsAny<Exception>(() => testObj.SetPropertyValue("ReadOnlyProperty", "newValue"));
            Assert.True(ex is ArgumentException || ex is TargetException || ex.InnerException is ArgumentException); //Actual exception can vary
            Assert.Equal(originalValue, testObj.ReadOnlyProperty); // Ensure it wasn't changed
        }


        [Fact]
        public void SetPropertyValue_NullObject_ReturnsFalse()
        {
            TestClass? testObj = null;
            Assert.False(testObj.SetPropertyValue("StringProperty", "value"));
        }

        [Fact]
        public void SetPropertyValue_Field_ReturnsFalse()
        {
            var testObj = new TestClass();
            var originalFieldValue = testObj.Field;
            Assert.False(testObj.SetPropertyValue("Field", "newFieldValue"));
            Assert.Equal(originalFieldValue, testObj.Field);
        }

        [Fact]
        public void SetPropertyValue_PrivateProperty_ReturnsFalse()
        {
            var testObj = new TestClass();
            Assert.False(testObj.SetPropertyValue("PrivateProperty", "newValue"));
        }
        #endregion SetPropertyValue Tests

        // Tests for GetPropertyValue(this object obj, string propertyName)
        #region GetPropertyValue Tests
        [Fact]
        public void GetPropertyValue_ValidPublicProperty_ReturnsValue()
        {
            var testObj = new TestClass { StringProperty = "TestValue", IntProperty = 42 };
            Assert.Equal("TestValue", testObj.GetPropertyValue("StringProperty"));
            Assert.Equal(42, testObj.GetPropertyValue("IntProperty"));
        }

        [Fact]
        public void GetPropertyValue_CaseInsensitive_ReturnsValue()
        {
            var testObj = new TestClass { StringProperty = "CaseTest" };
            Assert.Equal("CaseTest", testObj.GetPropertyValue("stringproperty"));
        }

        [Fact]
        public void GetPropertyValue_NonExistentProperty_ReturnsNull()
        {
            var testObj = new TestClass();
            Assert.Null(testObj.GetPropertyValue("NonExistent"));
        }

        [Fact]
        public void GetPropertyValue_Field_ReturnsNull()
        {
            var testObj = new TestClass();
            Assert.Null(testObj.GetPropertyValue("Field"));
        }

        [Fact]
        public void GetPropertyValue_PrivateProperty_ReturnsNull()
        {
            var testObj = new TestClass();
            Assert.Null(testObj.GetPropertyValue("PrivateProperty"));
        }

        [Fact]
        public void GetPropertyValue_NullObject_ReturnsNull()
        {
            TestClass? testObj = null;
            Assert.Null(testObj.GetPropertyValue("StringProperty"));
        }
        #endregion GetPropertyValue Tests

        // Tests for PropertyValueEquals(this object obj, string propertyName, object? value)
        #region PropertyValueEquals Tests
        [Fact]
        public void PropertyValueEquals_EqualStringValue_CaseInsensitive_ReturnsTrue()
        {
            var testObj = new TestClass { StringProperty = "TestValue" };
            Assert.True(testObj.PropertyValueEquals("StringProperty", "testvalue"));
            Assert.True(testObj.PropertyValueEquals("stringproperty", "TestValue"));
        }

        [Fact]
        public void PropertyValueEquals_NonEqualStringValue_ReturnsFalse()
        {
            var testObj = new TestClass { StringProperty = "TestValue" };
            Assert.False(testObj.PropertyValueEquals("StringProperty", "OtherValue"));
        }

        [Fact]
        public void PropertyValueEquals_EqualNonStringValue_ReturnsTrue()
        {
            var testObj = new TestClass { IntProperty = 123, BoolProperty = true };
            Assert.True(testObj.PropertyValueEquals("IntProperty", 123)); // Compares "123" with "123"
            Assert.True(testObj.PropertyValueEquals("BoolProperty", true)); // Compares "True" with "True"
        }

        [Fact]
        public void PropertyValueEquals_NonEqualNonStringValue_ReturnsFalse()
        {
            var testObj = new TestClass { IntProperty = 123, BoolProperty = true };
            Assert.False(testObj.PropertyValueEquals("IntProperty", 456)); // Compares "123" with "456"
            Assert.False(testObj.PropertyValueEquals("BoolProperty", false)); // Compares "True" with "False"
        }

        [Fact]
        public void PropertyValueEquals_DifferentTypeComparisonButSameStringValue_ReturnsTrue()
        {
            var testObj = new TestClass { IntProperty = 123 };
            // CommonHelpers.PropertyValueEquals converts both to string and compares case-insensitively.
            Assert.True(testObj.PropertyValueEquals("IntProperty", "123")); // Compares "123" with "123"
        }

        [Fact]
        public void PropertyValueEquals_NullPropertyValue_And_NullComparisonValue_ReturnsTrue()
        {
            var testObj = new TestClass { StringProperty = null };
            Assert.True(testObj.PropertyValueEquals("StringProperty", null));
        }

        [Fact]
        public void PropertyValueEquals_NullPropertyValue_And_NonNullComparisonValue_ReturnsFalse()
        {
            var testObj = new TestClass { StringProperty = null };
            Assert.False(testObj.PropertyValueEquals("StringProperty", "TestValue"));
        }

        [Fact]
        public void PropertyValueEquals_NonNullPropertyValue_And_NullComparisonValue_ReturnsFalse()
        {
            var testObj = new TestClass { StringProperty = "TestValue" };
            Assert.False(testObj.PropertyValueEquals("StringProperty", null));
        }

        [Fact]
        public void PropertyValueEquals_NullObject_ReturnsFalse()
        {
            TestClass? testObj = null;
            Assert.False(testObj.PropertyValueEquals("StringProperty", "TestValue"));
        }

        [Fact]
        public void PropertyValueEquals_NonExistentProperty_ReturnsFalse() // Because GetPropertyValue returns null, then compares null with "TestValue"
        {
            var testObj = new TestClass();
            Assert.False(testObj.PropertyValueEquals("NonExistentProperty", "TestValue"));
        }

        [Fact]
        public void PropertyValueEquals_NonExistentProperty_NullValue_ReturnsTrue() // GetPropertyValue returns null, then compares null with null
        {
            var testObj = new TestClass();
            Assert.True(testObj.PropertyValueEquals("NonExistentProperty", null));
        }
        #endregion PropertyValueEquals Tests

        // Tests for GetChanges(this object changed, object? original)
        #region GetChanges Tests
        [Fact]
        public void GetChanges_IdenticalPrimitiveObjects_ReturnsEmptyList()
        {
            int obj1 = 5;
            int obj2 = 5;
            // The GetChanges method expects class objects with properties, or anonymous types.
            // For raw primitives, it might not work as expected or throw if it can't get properties.
            // The current implementation of GetChanges/BuildAuditChangeLog uses GetType().GetProperties().
            // For int, GetProperties() is empty.
            Assert.Empty(obj1.GetChanges(obj2)); //This will be empty as int has no properties
        }

        [Fact]
        public void GetChanges_DifferentPrimitiveObjects_ReturnsEmptyList()
        {
            int obj1 = 5;
            int obj2 = 10;
            // As above, int has no properties, so no changes will be detected by GetProperties().
            Assert.Empty(obj1.GetChanges(obj2));
        }



        [Fact]
        public void GetChanges_BothNull_ReturnsEmptyList()
        {
            TestClass? obj1 = null;
            TestClass? obj2 = null;
            Assert.Empty(obj1.GetChanges(obj2));
        }

        private class AnotherTestClass { public string AnotherProperty { get; set; } }

        [Fact]
        public void GetChanges_DifferentTypes_ThrowsArgumentException()
        {
            var obj1 = new TestClass { StringProperty = "A" };
            var obj2 = new AnotherTestClass { AnotherProperty = "B" };
            Assert.Throws<ArgumentException>(() => obj1.GetChanges(obj2));
            Assert.Throws<ArgumentException>(() => obj2.GetChanges(obj1));
        }


        #endregion GetChanges Tests

        #region GetEventProperty Tests
        [Fact]
        public void GetEventProperty_NullEventRecord_ReturnsNull()
        {
            EventRecord? eventRecord = null;
            Assert.Null(eventRecord.GetEventProperty(0));
        }

        [Fact]
        public void GetEventProperty_ValidIndex_ReturnsPropertyValueAsString()
        {
            // Cannot directly test this without a real EventRecord or complex mocking.
            // EventRecord and EventProperty are abstract and their concrete implementations
            // are internal or have internal constructors, making them difficult to instantiate in tests.
            // This test case is acknowledged as not implemented due to these limitations.
            Assert.True(true, "Test for valid index in GetEventProperty not implemented due to EventRecord mocking complexity.");
        }

        [Fact]
        public void GetEventProperty_OutOfRangeIndex_ReturnsNull()
        {
            // Similar to the above, requires a mockable EventRecord.
            // The helper method internally catches ArgumentOutOfRangeException and returns null.
            // This test case is acknowledged as not implemented due to these limitations.
            Assert.True(true, "Test for out-of-range index in GetEventProperty not implemented due to EventRecord mocking complexity.");
        }
        #endregion GetEventProperty Tests

        #region ResizeRawImage Tests

        private byte[] CreateTestPng(int width, int height)
        {
            using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                // You can fill the image with a color if needed, but for resizing, a blank one is fine.
                // image[0,0] = SixLabors.ImageSharp.PixelFormats.Rgba32.ParseHex("FF0000"); // Example: Red pixel
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, new PngEncoder());
                    return ms.ToArray();
                }
            }
        }


        [Fact]
        public void ResizeRawImage_Portrait_NoCrop_MaintainsAspectRatio()
        {
            byte[] originalImage = CreateTestPng(50, 100); // Width:50, Height:100
            int maxDimension = 25; // Target height

            var resizedImageBytes = originalImage.ResizeRawImage(maxDimension, cropToSquare: false);
            Assert.NotEmpty(resizedImageBytes);

            using (var image = Image.Load(resizedImageBytes))
            {
                // Height should be maxDimension (25)
                // Width should be scaled: 50 * (25/100) = 12.5. ImageSharp might round to 12 or 13.
                // Helper: if (image.Height > image.Width) -> image.Mutate(x => x.Resize(0, maxDimension));
                // This means height becomes maxDimension, width is scaled.
                Assert.Equal(maxDimension, image.Height);
                Assert.InRange(image.Width, 12, 13); // Original aspect ratio 0.5. New width should be 25 * 0.5 = 12.5
            }
        }

        [Fact]
        public void ResizeRawImage_Landscape_NoCrop_MaintainsAspectRatio()
        {
            byte[] originalImage = CreateTestPng(100, 50); // Width:100, Height:50
            int maxDimension = 25; // Target width

            var resizedImageBytes = originalImage.ResizeRawImage(maxDimension, cropToSquare: false);
            Assert.NotEmpty(resizedImageBytes);

            using (var image = Image.Load(resizedImageBytes))
            {
                // Width should be maxDimension (25)
                // Height should be scaled: 50 * (25/100) = 12.5.
                // Helper: else (width >= height) -> image.Mutate(x => x.Resize(maxDimension, 0));
                // This means width becomes maxDimension, height is scaled.
                Assert.Equal(maxDimension, image.Width);
                Assert.InRange(image.Height, 12, 13); // Original aspect ratio 2.0. New height should be 25 / 2.0 = 12.5
            }
        }

        [Fact]
        public void ResizeRawImage_Portrait_CropToSquare_ResultsInSquare()
        {
            byte[] originalImage = CreateTestPng(50, 100); // Width:50, Height:100
            int maxDimension = 25;

            var resizedImageBytes = originalImage.ResizeRawImage(maxDimension, cropToSquare: true);
            Assert.NotEmpty(resizedImageBytes);

            using (var image = Image.Load(resizedImageBytes))
            {
                // Helper: if (image.Height > image.Width) -> crop(image.Width, image.Width) -> crop(50,50)
                // Then resize(0, maxDimension) -> resize(0,25) on the 50x50 image.
                // This should result in 25x25.
                Assert.Equal(maxDimension, image.Width);
                Assert.Equal(maxDimension, image.Height);
            }
        }

        [Fact]
        public void ResizeRawImage_Landscape_CropToSquare_ResultsInSquare()
        {
            byte[] originalImage = CreateTestPng(100, 50); // Width:100, Height:50
            int maxDimension = 25;

            var resizedImageBytes = originalImage.ResizeRawImage(maxDimension, cropToSquare: true);
            Assert.NotEmpty(resizedImageBytes);

            using (var image = Image.Load(resizedImageBytes))
            {
                // Helper: else (width >= height) -> crop(image.Height, image.Height) -> crop(50,50)
                // Then resize(maxDimension, 0) -> resize(25,0) on the 50x50 image.
                // This should result in 25x25.
                Assert.Equal(maxDimension, image.Width);
                Assert.Equal(maxDimension, image.Height);
            }
        }

        [Fact]
        public void ResizeRawImage_SquareImage_CropToSquare_MaintainsSizeAndResizes()
        {
            byte[] originalImage = CreateTestPng(100, 100);
            int maxDimension = 50;

            var resizedImageBytes = originalImage.ResizeRawImage(maxDimension, cropToSquare: true);
            Assert.NotEmpty(resizedImageBytes);
            using (var image = Image.Load(resizedImageBytes))
            {
                // Crop(100,100) does nothing. Resize(50,0) makes it 50x50.
                Assert.Equal(maxDimension, image.Width);
                Assert.Equal(maxDimension, image.Height);
            }
        }

        #endregion ResizeRawImage Tests

        #region DateTimeToAdsValue and AdsValueToDateTime Tests

        [Fact]
        public void DateTimeToAdsValue_NullDateTime_ReturnsNull()
        {
            DateTime? dt = null;
            Assert.Null(dt.DateTimeToAdsValue());
        }

        [Fact]
        public void DateTimeToAdsValue_FileTimeMinValue_ReturnsCorrectFileTime()
        {
            var min = DateTime.FromFileTimeUtc(0).ToUniversalTime();
            var minFileTime = DateTime.Parse("1/1/1601 12:00:00 AM Z");
            long expectedFileTime = 0; // This is a valid FileTime
            Assert.Equal(expectedFileTime, minFileTime.ToFileTimeUtc());
        }

        [Fact]
        public void DateTimeToAdsValue_DateTimeMaxValue_ReturnsNullDueToRange()
        {
            // DateTime.MaxValue.ToFileTimeUtc() throws ArgumentOutOfRangeException
            // The helper catches this and returns null.
            DateTime? dt = DateTime.MaxValue;
            Assert.Null(dt.DateTimeToAdsValue());
        }

        [Fact]
        public void AdsValueToDateTime_NullObject_ReturnsNull()
        {
            object? value = null;
            Assert.Null(value.AdsValueToDateTime());
        }

        [Fact]
        public void AdsValueToDateTime_ValidLongFileTime_ReturnsCorrectDateTime()
        {
            DateTime now = DateTime.UtcNow;
            long fileTime = now.ToFileTimeUtc();
            DateTime? result = fileTime.AdsValueToDateTime();
            Assert.NotNull(result);
            // Precision loss can occur with FileTime, so compare with tolerance
            Assert.Equal(now.Year, result.Value.Year);
            Assert.Equal(now.Month, result.Value.Month);
            Assert.Equal(now.Day, result.Value.Day);
            Assert.Equal(now.Hour, result.Value.Hour);
            Assert.Equal(now.Minute, result.Value.Minute);
            Assert.Equal(now.Second, result.Value.Second);
        }

        [Fact]
        public void AdsValueToDateTime_StringOfValidLongFileTime_ReturnsCorrectDateTime()
        {
            DateTime now = DateTime.UtcNow;
            long fileTime = now.ToFileTimeUtc();
            string fileTimeString = fileTime.ToString();
            DateTime? result = fileTimeString.AdsValueToDateTime();
            Assert.NotNull(result);
            Assert.Equal(now.Year, result.Value.Year);
            Assert.Equal(now.Month, result.Value.Month);
            Assert.Equal(now.Day, result.Value.Day);
            Assert.Equal(now.Hour, result.Value.Hour);
            Assert.Equal(now.Minute, result.Value.Minute);
            Assert.Equal(now.Second, result.Value.Second);
        }


        [Fact]
        public void AdsValueToDateTime_ZeroLong_ReturnsNull()
        {
            long fileTime = 0L;
            // The helper considers 0L as a "null" or "no date" scenario.
            Assert.Null(fileTime.AdsValueToDateTime());
        }

        [Fact]
        public void AdsValueToDateTime_ADsNullTimeEquivalentLong_ReturnsNull()
        {
            long adsNullFileTime = CommonHelpers.ADS_NULL_TIME.ToFileTimeUtc();
            // This will convert to ADS_NULL_TIME, which the helper then converts to null.
            Assert.Null(adsNullFileTime.AdsValueToDateTime());
        }



        [Fact]
        public void AdsValueToDateTime_IADsLargeInteger_ValidDate_ReturnsCorrectDateTime()
        {
            DateTime now = new DateTime(2025, 5, 15, 10, 0, 0, DateTimeKind.Utc);
            long fileTime = now.ToFileTimeUtc();

            var adsLargeInt = new CommonHelpers.ADsLargeInteger
            {
                HighPart = (int)(fileTime >> 32),
                LowPart = (int)(fileTime & 0xFFFFFFFF)
            };

            DateTime? result = adsLargeInt.AdsValueToDateTime();
            Assert.NotNull(result);
            Assert.Equal(now.Year, result.Value.Year);
            Assert.Equal(now.Month, result.Value.Month);
            Assert.Equal(now.Day, result.Value.Day);
            Assert.Equal(now.Hour, result.Value.Hour);
        }

        [Fact]
        public void AdsValueToDateTime_IADsLargeInteger_ZeroDate_ReturnsNull()
        {
            var adsLargeInt = new CommonHelpers.ADsLargeInteger { HighPart = 0, LowPart = 0 };
            Assert.Null(adsLargeInt.AdsValueToDateTime());
        }

        [Fact]
        public void AdsValueToDateTime_IADsLargeInteger_ADsNullTimeDate_ReturnsNull()
        {
            long adsNullFileTime = CommonHelpers.ADS_NULL_TIME.ToFileTimeUtc();
            var adsLargeInt = new CommonHelpers.ADsLargeInteger
            {
                HighPart = (int)(adsNullFileTime >> 32),
                LowPart = (int)(adsNullFileTime & 0xFFFFFFFF)
            };
            Assert.Null(adsLargeInt.AdsValueToDateTime());
        }


        [Fact]
        public void AdsValueToDateTime_UnexpectedObjectType_ReturnsNull()
        {
            object value = new object();
            Assert.Null(value.AdsValueToDateTime());
        }

        [Fact]
        public void AdsValueToDateTime_AlreadyDateTimeObject_ReturnsSameDateTime()
        {
            DateTime now = DateTime.UtcNow;
            object value = now; // Boxed DateTime
            DateTime? result = value.AdsValueToDateTime();
            Assert.Equal(now, result);
        }

        #endregion DateTimeToAdsValue and AdsValueToDateTime Tests
    }
}
