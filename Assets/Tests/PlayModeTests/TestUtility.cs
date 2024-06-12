﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.CountlySDK.Enums;
using System.Text.RegularExpressions;
using UnityEngine;
using NUnit.Framework;

namespace Assets.Tests.PlayModeTests
{
    /// <summary>
    /// Utility class for creating Countly configurations, clearing SDK queues, and managing log helpers.
    /// </summary>
    public class TestUtility
    {
        readonly static string SERVER_URL = "https://xyz.com/";
        readonly static string APP_KEY = "772c091355076ead703f987fee94490";
        readonly static string DEVICE_ID = "test_user";
        /// <summary>
        /// Creates a basic Countly configuration with predefined server URL, app key, and device ID.
        /// </summary>
        /// <returns>CountlyConfiguration object representing the basic configuration.</returns>
        public static CountlyConfiguration CreateBaseConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(SERVER_URL, APP_KEY)
                .SetDeviceId(DEVICE_ID);
            return configuration;
        }

        /// <summary>
        /// Creates a Countly configuration with setting consent requirement to true, predefined server URL, app key, and device ID.
        /// </summary>
        /// <returns>CountlyConfiguration object representing the basic configuration.</returns>
        public static CountlyConfiguration CreateBaseConfigConsent(Consents[] givenConsent)
        {
            CountlyConfiguration configuration = CreateBaseConfig()
                .SetRequiresConsent(true);

            configuration.GiveConsent(givenConsent);
            return configuration;
        }

        /// <summary>
        /// Creates a Countly configuration with setting consent requirement to true
        /// </summary>
        /// <returns>CountlyConfiguration object with no given consent</returns>
        public static CountlyConfiguration CreateNoConsentConfig()
        {
            CountlyConfiguration configuration = CreateBaseConfig()
                .SetRequiresConsent(true);

            return configuration;
        }

        /// <summary>
        /// Creates a Countly configuration for testing View functionality by setting related fields
        /// </summary>
        /// <returns>CountlyConfiguration object representing the basic configuration.</returns>
        public static CountlyConfiguration CreateViewConfig(CustomIdProvider customIdProvider)
        {
            CountlyConfiguration configuration = CreateBaseConfig()
                .SetRequiresConsent(true);
            configuration.SafeViewIDGenerator = customIdProvider;
            configuration.GiveConsent(new Consents[] { Consents.Views });

            return configuration;
        }

        /// <summary>
        /// Clears the queues of the Countly SDK.
        /// </summary>
        /// <param name="CountlyInstance"></param>
        public static void ClearSDKQueues(Countly CountlyInstance)
        {
            CountlyInstance.Events._eventRepo.Clear();
            CountlyInstance.CrashReports._requestCountlyHelper._requestRepo.Clear();
        }

        /// <summary>
        /// Creates a CountlyLogHelper instance for testing purposes, based on the provided configuration and enables or disables logging.
        /// </summary>
        /// <returns>CountlyLogHelper instance with the specified logging configuration.</returns>
        public static CountlyLogHelper CreateLogHelper()
        {
            CountlyConfiguration config = CreateBaseConfig()
                .EnableLogging();
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

        /// <summary>
        /// Creates a CountlyEventModel for testing purposes
        /// </summary>
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

        /// <summary>
        /// Creates a JSON string representing an event entity with optional parameters.
        /// </summary>
        /// <param name="key">The key of the event.</param>
        /// <param name="count">The count associated with the event.</param>
        /// <param name="sum">The sum associated with the event.</param>
        /// <param name="dur">The duration associated with the event.</param>
        /// <param name="segmentation">The segmentation associated with the event.</param>
        /// <param name="timestamp">The timestamp associated with the event.</param>
        /// <param name="dow">The day of the week associated with the event.</param>
        /// <param name="hour">The hour associated with the event.</param>
        /// <param name="customData">Custom data associated with the event.</param>
        /// <returns>A JSON string representing the event entity.</returns>
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

        /// <summary>
        /// Checks if the input string is in Base64 format.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is in Base64 format; otherwise, false.</returns>
        public static bool IsBase64String(string value)
        {
            string base64Pattern = @"^[A-Za-z0-9+/]*={0,2}$";
            return Regex.IsMatch(value, base64Pattern);
        }

        /// <summary>
        /// Cleans up test environment by clearing Countly storage and destroying its instance.
        /// </summary>
        public static void TestCleanup()
        {
            _ = Countly.Instance.Session?.EndSessionAsync();
            Countly.Instance.Session?.StopSessionTimer();
            Countly.Instance.ClearStorage();
            UnityEngine.Object.DestroyImmediate(Countly.Instance);
        }

        public static Dictionary<string, object> TestSegmentation()
        {
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("long", 1234567890123456789L);
            segmentation.Add("bool", true);

            return segmentation;
        }

        public static Dictionary<string, object> BaseViewTestSegmentation(string viewName, bool isVisit, bool isStart)
        {
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("name", viewName);
            if (isVisit) {
                segmentation.Add("visit", 1);
            }
            if (isStart) {
                segmentation.Add("start", 1);
            }
            segmentation.Add("segment", Application.platform.ToString().ToLower());

            return segmentation;
        }

        public static Dictionary<string, object> TestTimeMetrics()
        {
            return TimeMetricModel.GetTimeMetricModel();
        }

        // Validates the properties of a CountlyEventModel object for view events or view action events
        public static void ViewEventValidator(CountlyEventModel eventModel, int? expectedCount, double? expectedSum,
            int? expectedDuration, Dictionary<string, object>? expectedSegmentation,
            string? expectedEventId, string? expectedPreviousViewId, string? expectedCurrentViewId,
            string? expectedPreviousEventId, Dictionary<string, object>? expectedTimeMetrics, bool isAction = false)
        {
            if (isAction) {
                Assert.AreEqual(eventModel.Key, "[CLY]_action");
            } else {
                Assert.AreEqual(eventModel.Key, "[CLY]_view");
            }

            Assert.AreEqual(eventModel.Count, expectedCount);
            Assert.AreEqual(eventModel.Sum, expectedSum);
            Assert.AreEqual(eventModel.Duration, expectedDuration);

            if (expectedSegmentation != null) {
                foreach (var kvp in expectedSegmentation) {
                    Assert.IsTrue(eventModel.Segmentation.ContainsKey(kvp.Key));
                    Assert.AreEqual(eventModel.Segmentation[kvp.Key], kvp.Value);
                }
            }

            Assert.AreEqual(eventModel.EventID, expectedEventId);
            Assert.AreEqual(eventModel.PreviousViewID, expectedPreviousViewId);
            Assert.AreEqual(eventModel.CurrentViewID, expectedCurrentViewId);
            Assert.AreEqual(eventModel.PreviousEventID, expectedPreviousEventId);

            // Check time metrics if provided
            if (expectedTimeMetrics != null) {
                foreach (var kvp in expectedTimeMetrics) {
                    if (kvp.Key == "timestamp") {
                        Assert.IsTrue(Mathf.Abs(eventModel.Timestamp - (long)kvp.Value) < 6000);
                    } else if (kvp.Key == "hour") {
                        Assert.AreEqual(eventModel.Hour, kvp.Value);
                    } else if (kvp.Key == "dow") {
                        Assert.AreEqual(eventModel.DayOfWeek, kvp.Value);
                    }
                }
            }
        }

        public static void ValidateRQEQSize(Countly cly, int expRQSize, int expEQSize)
        {
            CountlyRequestModel[] requests = cly.RequestHelper._requestRepo.Models.ToArray();
            Assert.AreEqual(expRQSize, requests.Length);

            CountlyEventModel[] events = cly.Events._eventRepo.Models.ToArray();
            Assert.AreEqual(expEQSize, events.Length);
        }
    }
}