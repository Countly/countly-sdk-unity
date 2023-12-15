using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using System.Text.RegularExpressions;

namespace Assets.Tests.PlayModeTests
{
    /// Utility class for creating Countly configurations, clearing SDK queues, and managing log helpers.
    public class TestUtility
    {
        readonly static string SERVER_URL = "https://xyz.com/";
        readonly static string APP_KEY = "772c091355076ead703f987fee94490";
        readonly static string DEVICE_ID = "test_user";

        /// <summary>
        /// Creates a basic Countly configuration with predefined server URL, app key, and device ID.
        /// </summary>
        /// <returns>CountlyConfiguration object representing the basic configuration.</returns>
        public static CountlyConfiguration createBaseConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = SERVER_URL,
                AppKey = APP_KEY,
                DeviceId = DEVICE_ID,
            };

            return configuration;
        }

        public static CountlyConfiguration createBaseConfigConsent(Consents[] givenConsent)
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = SERVER_URL,
                AppKey = APP_KEY,
                DeviceId = DEVICE_ID,
            };

            configuration.RequiresConsent = true;
            configuration.GiveConsent(givenConsent);

            return configuration;
        }

        /// Clears the queues of the Countly SDK.
        public static void ClearSDKQueues(Countly CountlyInstance)
        {
            CountlyInstance.Views._eventService._eventRepo.Clear();
            CountlyInstance.CrashReports._requestCountlyHelper._requestRepo.Clear();
        }

        /// <summary>
        /// Creates a CountlyLogHelper instance for testing purposes, based on the provided configuration and enables or disables logging.
        /// </summary>
        /// <returns>CountlyLogHelper instance with the specified logging configuration.</returns>
        public static CountlyLogHelper CreateLogHelper(bool enableLogging)
        {
            CountlyConfiguration config = createBaseConfig();
            config.EnableConsoleLogging = enableLogging;
            CountlyLogHelper logHelper = new CountlyLogHelper(config);

            return logHelper;
        }

        /// <summary>
        /// Creates an EventEntity for testing purposes, with provided id and json.
        /// </summary>
        /// <returns> EventEntity instance with the specified id and json configuration.</returns>
        public static EventEntity CreateEventEntity(int id, string json)
        {
            EventEntity entity = new EventEntity {
                Id = id,
                Json = json
            };
            return entity;
        }

        public static CountlyEventModel CreateEventModel(string key, int? count = null, double? sum = null, double? dur = null, Dictionary<string, object> segmentation = null, long? timestamp = null, int? dow = null, int? hour = null)
        {
            CountlyEventModel expected = new CountlyEventModel();

            expected.Key = key;

            if (count != null) {
                expected.Count = (int)count;
            }
            if (sum != null) {
                expected.Sum = (double)sum;
            }
            if (dur != null) {
                expected.Duration = (double)dur;
            }
            if (segmentation != null) {
                expected.Segmentation = new SegmentModel(segmentation);
            }
            if (timestamp != null) {
                expected.Timestamp = (long)timestamp;
            }
            if (dow != null) {
                expected.DayOfWeek = (int)dow;
            }
            if (hour != null) {
                expected.Hour = (int)hour;
            }

            return expected;
        }

        public static string CreateEventEntityJSONString(string? key = null, int? count = null, double? sum = null, double? dur = null, string segmentation = null, long? timestamp = null, int? dow = null, int? hour = null, string? customData = null)
        {
            JObject jobj = new JObject();

            if (key != null) {
                jobj.Add("Key", key);
            }

            if (count != null) {
                jobj.Add("Count", count);
            }

            if (sum != null) {
                jobj.Add("Sum", sum);
            }

            if (dur != null) {
                jobj.Add("dur", dur);
            }

            if (timestamp != null) {
                jobj.Add("timestamp", timestamp);
            }

            if (dow != null) {
                jobj.Add("dow", dow);
            }

            if (hour != null) {
                jobj.Add("hour", hour);
            }

            string returnS = jobj.ToString();

            if (segmentation != null) {
                returnS = returnS.Insert(returnS.Length - 1, ", \"Segmentation\":" + segmentation);
            }

            if (customData != null) {
                returnS = returnS.Insert(returnS.Length - 1, "," + customData);
            }

            return returnS;
        }

        public static bool IsBase64String(string value)
        {
            string base64Pattern = @"^[A-Za-z0-9+/]*={0,2}$";
            return Regex.IsMatch(value, base64Pattern);
        }

        public static void TestCleanup()
        {
            Countly.Instance.ClearStorage();
            UnityEngine.Object.DestroyImmediate(Countly.Instance);
        }
    }
}
