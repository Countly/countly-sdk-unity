using System.Collections.Generic;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using NUnit.Framework;
using Assets.Tests.PlayModeTests;
using UnityEngine;
using Plugins.CountlySDK.Services;
using Plugins.CountlySDK.Enums;

namespace Tests
{
    public class UtilsTests
    {
        // 'GetAppKeyAndDeviceIdParams' method in the CountlyUtils.
        // Retrieves a dictionary containing the "app_key" and "device_id" parameters.
        // It should return a dictionary which contains correct "app_key" and "device_id"
        [Test]
        public void GetAppKeyAndDeviceIdParams()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

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
            Countly.Instance.Init(TestUtility.createBaseConfig());
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
            Countly.Instance.Init(TestUtility.createBaseConfig());
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            bool isValid = utils.IsPictureValid(pictureUrl);
            return isValid;
        }

        // 'GetUniqueDeviceId' method in CountlyUtils
        // We add "CLY_" preface to the SDK Generated device id
        // If developer provided a device id which is not whitespace, empty or null, SDK should not generate one with a preface
        public void GetUniqueDeviceId_base(string deviceId)
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.DeviceId = deviceId;
            Countly.Instance.Init(configuration);

            string currentDeviceID = Countly.Instance.Device.DeviceId;
            Assert.IsNotNull(currentDeviceID);

            if (!string.IsNullOrEmpty(deviceId) && !string.IsNullOrWhiteSpace(deviceId)) {
                Assert.AreEqual(currentDeviceID, deviceId);
                Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
                Assert.AreNotEqual(currentDeviceID, CountlyUtils.GetUniqueDeviceId());
                Assert.IsFalse(currentDeviceID.Contains("CLY_")); //developer provided 
            } else {
                Assert.AreEqual(currentDeviceID, CountlyUtils.GetUniqueDeviceId());
                Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
                Assert.IsTrue(currentDeviceID.Contains("CLY_")); //sdk generated
            }
        }

        // Developer is providing a device id which is not whitespace, empty or null
        // Therefore developer provided device id should be used.
        [Test]
        public void GetUniqueDeviceId_UserProvidedDeviceId()
        {
            GetUniqueDeviceId_base("device_id");
        }

        // Developer is providing a device id which is empty
        // Therefore SDK should generate a device id with "CLY_" preface
        [Test]
        public void GetUniqueDeviceId_EmptyDeviceId()
        {
            GetUniqueDeviceId_base("");
        }

        // Developer is providing a device id which is null
        // Therefore SDK should generate a device id with "CLY_" preface
        [Test]
        public void GetUniqueDeviceId_NullDeviceId()
        {
            GetUniqueDeviceId_base(null);
        }

        // Developer is providing a device id which is whitespace
        // Therefore SDK should generate a device id with "CLY_" preface
        [Test]
        public void GetUniqueDeviceId_WhitespaceDeviceId()
        {
            GetUniqueDeviceId_base(" ");
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}

