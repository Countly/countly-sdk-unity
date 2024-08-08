using System.Collections.Generic;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using NUnit.Framework;
using Plugins.CountlySDK.Enums;
using System;

namespace Assets.Tests.PlayModeTests
{
    public class UtilsTests
    {
        // 'GetAppKeyAndDeviceIdParams' method in the CountlyUtils.
        // Retrieves a dictionary containing the "app_key" and "device_id" parameters.
        // It should return a dictionary which contains correct "app_key" and "device_id"
        [Test]
        public void GetAppKeyAndDeviceIdParams()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            Dictionary<string, object> userParams = utils.GetAppKeyAndDeviceIdParams();

            Assert.IsNotNull(userParams);
            Assert.AreEqual(userParams.Count, userParams.Count);
            Assert.AreEqual(userParams["app_key"], userParams["app_key"]);
            Assert.AreEqual(userParams["device_id"], userParams["device_id"]);
        }

        // 'GetStringFromBytes' method in CountlyUtils
        // We provide a byte array to method to convert it into a string
        // Method should convert the provided array into string
        public void GetStringFromBytes_base(byte[] byteArray, string expected)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            string hexString = utils.GetStringFromBytes(byteArray);
            Assert.AreEqual(expected, hexString);
        }

        // 'GetStringFromBytes' method in CountlyUtils
        // We provide a valid byte array to method to convert it into a string
        // Method should convert the valid array.
        [Test]
        public void GetStringFromBytes_ValidArray()
        {
            byte[] byteArray = { 0x48, 0x65, 0x6c, 0x6c, 0x6f };
            string expectedResult = "48656c6c6f";

            GetStringFromBytes_base(byteArray, expectedResult);
        }

        // 'GetStringFromBytes' method in CountlyUtils
        // We provide an empty byte array to method to convert it into a string
        // Method should convert the empty array.
        [Test]
        public void GetStringFromBytes_EmptyArray()
        {
            byte[] byteArray = { };
            string expectedResult = "";

            GetStringFromBytes_base(byteArray, expectedResult);
        }

        // 'GetStringFromBytes' method in CountlyUtils
        // We provide an null byte array to method to convert it into a string
        // Method should return an empty string 
        [Test]
        public void GetStringFromBytes_NullArray()
        {
            byte[] byteArray = null;
            string expectedResult = "";

            GetStringFromBytes_base(byteArray, expectedResult);
        }

        // 'IsPictureValid' method in CountlyUtils
        // We provide URLs with different extensions, empty URL, null URL and invalid URL
        // 'IsPictureValid' should return true if provided URL is valid, else false
        [TestCase("", ExpectedResult = true)] // Empty URL should be considered valid
        [TestCase("http://example.com/pic.png", ExpectedResult = true)] // Valid URL with .png extension
        [TestCase("http://example.com/pic.jpg", ExpectedResult = true)] // Valid URL with .jpg extension
        [TestCase("http://example.com/pic.jpeg", ExpectedResult = true)] // Valid URL with .jpeg extension
        [TestCase("http://example.com/pic.gif", ExpectedResult = true)] // Valid URL with .gif extension
        [TestCase("http://example.com/pic.bmp", ExpectedResult = false)] // Invalid URL with unsupported extension
        [TestCase(null, ExpectedResult = true)] // Null URL should be considered valid
        [TestCase("http://example.com/pic.jpg?query=123", ExpectedResult = true)] // Valid URL with query string
        [TestCase("invalid_url", ExpectedResult = false)] // Invalid URL without extension
        public bool TestIsPictureValid(string pictureUrl)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            bool isValid = utils.IsPictureValid(pictureUrl);
            return isValid;
        }

        // The SDK device ID acquisition method
        // During the first init we are providing a device ID value so the SDK does not generate a device ID
        // The device ID that the SDK internally acquires should have no prefix
        [Test]
        public void GetUniqueDeviceId_UserProvidedDeviceId()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.DeviceId = "device_id";
            Countly.Instance.Init(configuration);

            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
            Assert.IsFalse(Countly.Instance.Device.DeviceId.Contains("CLY_")); //developer provided
        }

        // The SDK device ID acquisition method
        // During the first init we do not provide a device ID value so the SDK does generates a device ID
        // The device ID that the SDK internally acquires should have a "CLY_" prefix
        [Test]
        public void GetUniqueDeviceId_NullDeviceId()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.DeviceId = null;
            Countly.Instance.Init(configuration);

            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
            Assert.IsTrue(Countly.Instance.Device.DeviceId.StartsWith("CLY_")); //sdk generated
        }

        // "CountlyUtils.GetUniqueDeviceId()"
        // Generate a value
        // That value should start with the "CLY_" prefix and it should be more then just the prefix
        [Test]
        public void GetUniqueDeviceId()
        {
            string generatedValue = CountlyUtils.GetUniqueDeviceId();
            Assert.IsTrue(generatedValue.StartsWith("CLY_"));
            Assert.IsTrue(generatedValue.Length > 4);
        }

        // 'RemoveUnsupportedDataTypes' in CountlyUtils
        // Removes unsuppored data types and returns true if something is removed
        // It should remove data if provided Dictionary contains something other than string, int, double, bool
        public void RemoveUnsupportedDataTypes_base(Dictionary<string, object> data, bool isRemovingExpected)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            if (isRemovingExpected) {
                Assert.IsTrue(utils.RemoveUnsupportedDataTypes(data, null));
            } else {
                Assert.IsFalse(utils.RemoveUnsupportedDataTypes(data, null));
            }
        }

        // 'RemoveUnsupportedDataTypes' in CountlyUtils
        // Removes unsuppored data types and returns true if something is removed
        // It should return false since provided data is null and nothing can be removed
        [Test]
        public void RemoveUnsupportedDataTypes_Null()
        {
            RemoveUnsupportedDataTypes_base(null, false);
        }

        // 'RemoveUnsupportedDataTypes' in CountlyUtils
        // Removes unsuppored data types and returns true if something is removed
        // It should return true since provided data contains unsupported data type
        [Test]
        public void RemoveUnsupportedDataTypes_UnsupportedDataType()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 },
                { "key3", 3.14 },
                { "key4", true },
                { "key5", 1234567890123456789L},
                { "key6", new object() } // Unsupported data type
            };

            RemoveUnsupportedDataTypes_base(data, true);
        }

        // 'RemoveUnsupportedDataTypes' in CountlyUtils
        // Removes unsuppored data types and returns true if something is removed
        // It should return false since provided data does not contain unsupported data type
        [Test]
        public void RemoveUnsupportedDataTypes_SupportedDataType()
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 },
                { "key3", 3.14 },
                { "key4", true },
                { "key5", 1234567890123456789L}
            };

            RemoveUnsupportedDataTypes_base(data, false);
        }

        // 'CopyDictionaryToDestination' in CountlyUtils
        // We put all items in a Dictionary into another Dictionary
        // All items should be placed into the destination without any problem
        [Test]
        public void CopyDictionaryToDestination_ValidDictionaries()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            Dictionary<string, object> destination = new Dictionary<string, object>
            {
                { "key1", "value1" }
            };

            Dictionary<string, object> source = new Dictionary<string, object>
            {
                { "key2", 42 },
                { "key3", 3.14 },
                { "key4", true },
                { "key5", 1234567890123456789L }
            };

            utils.CopyDictionaryToDestination(destination, source, TestUtility.CreateLogHelper());

            // validating that no changes have happened on source
            Assert.AreEqual(4, source.Count);
            Assert.AreEqual(source["key2"], 42);
            Assert.AreEqual(source["key3"], 3.14);
            Assert.AreEqual(source["key4"], true);
            Assert.AreEqual(source["key5"], 1234567890123456789L);

            // validating the destination
            Assert.AreEqual(5, destination.Count);
            Assert.AreEqual("value1", destination["key1"]);
            Assert.AreEqual(source["key2"], destination["key2"]);
            Assert.AreEqual(source["key3"], destination["key3"]);
            Assert.AreEqual(source["key4"], destination["key4"]);
            Assert.AreEqual(source["key5"], destination["key5"]);
        }

        // 'CopyDictionaryToDestination' in CountlyUtils
        // We pass empty and null Dictionaries to put into another Dictionary
        // Nothing should break, removed or added into the destination
        [TestCase(true)]
        [TestCase(false)]
        public void CopyDictionaryToDestination_NullOrEmptySource(bool isNull)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            Dictionary<string, object> destination = new Dictionary<string, object>
            {
                { "key1", "value1" }
            };

            Dictionary<string, object> source = new Dictionary<string, object>();

            if (isNull) {
                source = null;
            }

            utils.CopyDictionaryToDestination(destination, source, TestUtility.CreateLogHelper());

            // validating that no changes have happened on source
            if (isNull) {
                Assert.IsNull(source);
            } else {
                Assert.IsEmpty(source);
            }      

            // validating the destination
            Assert.AreEqual("value1", destination["key1"]);
            Assert.AreEqual(1, destination.Count);
        }

        // 'CopyDictionaryToDestination' in CountlyUtils
        // We pass valid Dictionary into an empty destination
        // All KeyValuePairs should be added into destination
        [TestCase(true)]
        [TestCase(false)]
        public void CopyDictionaryToDestination_ValidDictionary_EmptyAndNullDestination(bool isNull)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            Dictionary<string, object> destination = new Dictionary<string, object>();
            if (isNull) {
                destination = null;
            }

            Dictionary<string, object> source = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 },
                { "key3", 3.14 },
                { "key4", true },
                { "key5", 1234567890123456789L }
            };

            utils.CopyDictionaryToDestination(destination, source, TestUtility.CreateLogHelper());

            // validating that no changes have happened on source
            Assert.AreEqual(5, source.Count);
            Assert.AreEqual(source["key1"], "value1");
            Assert.AreEqual(source["key2"], 42);
            Assert.AreEqual(source["key3"], 3.14);
            Assert.AreEqual(source["key4"], true);
            Assert.AreEqual(source["key5"], 1234567890123456789L);

            // validating the destination
            if (isNull) {
                Assert.IsNull(destination);
            } else {
                Assert.AreEqual(source.Count, destination.Count);
                foreach (var kvp in source) {
                    Assert.IsTrue(destination.ContainsKey(kvp.Key));
                    Assert.AreEqual(kvp.Value, destination[kvp.Key]);
                }
            }
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}

