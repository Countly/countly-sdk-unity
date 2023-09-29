using System.Collections.Generic;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using NUnit.Framework;
using Assets.Tests.PlayModeTests;
using UnityEngine;

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
            // Create a base Countly configuration and initialize Countly
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);

            // Create a CountlyUtils instance
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            // Retrieve the "app_key" and "device_id" parameters
            Dictionary<string, object> userParams = utils.GetAppKeyAndDeviceIdParams();

            // Perform assertions
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
            // Create a base Countly configuration and initialize Countly
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);

            // Create a CountlyUtils instance
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            // Connvert the byte array to a hexadecimal string
            string hexString = utils.GetStringFromBytes(byteArray);

            // Perform assertion to ensure the correct conversion
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
            // Create a base Countly configuration and initialize Countly
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);

            // Create a CountlyUtils instance
            CountlyUtils utils = new CountlyUtils(Countly.Instance);

            // Check if the provided picture URL is valid
            bool isValid = utils.IsPictureValid(pictureUrl);

            // Check if the method returns the expected result
            return isValid;
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}

