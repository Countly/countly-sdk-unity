using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using NUnit.Framework;
using Plugins.CountlySDK.Helpers;
using System.Collections.Generic;
using Assets.Tests.PlayModeTests;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using static UnityEngine.Networking.UnityWebRequest;
using Newtonsoft.Json.Linq;
using UnityEditor.UIElements;
using System.Text;
using System.Net.NetworkInformation;
using Plugins.CountlySDK;

namespace Tests
{
    public class HelpersTests
    {
        // 'ConvertEventEntityToEventModel' method in Converter.
        // We convert an EventEntity into CountlyEventModel.
        // With a valid EventEntity provided, conversion should be successful.
        public void EventEntityToEventModel_base(EventEntity entity, CountlyEventModel expected)
        {
            // Convert the EventEntity to a CountlyEventModel using the Converter.
            CountlyEventModel model = Converter.ConvertEventEntityToEventModel(entity, TestUtility.CreateLogHelper(true));

            // If the conversion resulted in a null CountlyEventModel, assert that the expected value is also null.
            if (expected == null) {
                Assert.IsNull(model);
            }
            // Check if the conversion resulted in a non-null CountlyEventModel.
            else {
                // Assert that individual properties of the converted CountlyEventModel match the expected values.
                Assert.IsNotNull(model);
                Assert.AreEqual(expected.Id, model.Id);
                Assert.AreEqual(expected.Key, model.Key);
                Assert.AreEqual(expected.Count, model.Count);
                Assert.AreEqual(expected.Sum, model.Sum);
                Assert.AreEqual(expected.Segmentation, model.Segmentation);
                Assert.AreEqual(expected.Timestamp, model.Timestamp);
                Assert.AreEqual(expected.Hour, model.Hour);
                Assert.AreEqual(expected.DayOfWeek, model.DayOfWeek);
            }
        }

        // 'ConvertEventEntityToEventModel' method in Converter.
        // Conversion of a valid EventEntity with JSON data to a CountlyEventModel.
        // Since a valid entity is provided, conversion should be successful.
        [Test]
        public void EventEntityToEventModel_withValidEntity()
        {
            string json = @"
            {
                ""Class"": ""Fighter"",
                ""Level"": 20,
                ""OtherClass"": null,
                ""IsMultiClass"": false,
                ""Languages"": [""Elven"", ""Common""],
                ""Character"": {
                    ""Name"": ""John"",
                    ""Race"": ""Human"",
                }
            }";

            Dictionary<string, object> segm = new Dictionary<string, object>
            {
                { "Class", "Fighter" },
                { "Level", 20 },
                { "IsMultiClass", false },
                { "OtherClass", null},
                { "Languages", new JArray { "Elven", "Common" } },
                { "Character", new JObject
                    {
                        { "Name", "John" },
                        { "Race", "Human" },
                    }
                }
            };
            // Create a valid EventEntity with JSON data and an expected CountlyEventModel.
            EventEntity entity = TestUtility.CreateEventEntity(0, TestUtility.CreateEventEntityJSONString(key: "SampleEvent", count: 5, sum: 10, dow: 2, hour: 13, timestamp: 123456, segmentation: json));

            CountlyEventModel expected = TestUtility.CreateEventModel(key: "SampleEvent", count: 5, sum: 10, dow: 2, hour: 13, timestamp: 123456, segmentation: segm);

            EventEntityToEventModel_base(entity, expected);
        }

        // 'ConvertEventEntityToEventModel' method in Converter.
        // Handling of an EventEntity with empty JSON data.
        // Since created EventEntity is containing an empty json, Converter should return null.
        [Test]
        public void EventEntityToEventModel_withEmptyJson()
        {
            // Create an EventEntity with empty JSON data and an expected null CountlyEventModel.
            EventEntity entity = TestUtility.CreateEventEntity(1, "");
            CountlyEventModel expected = null;

            EventEntityToEventModel_base(entity, expected);
        }

        // 'ConvertEventEntityToEventModel' method in Converter.
        // Handling of an EventEntity with invalid JSON data.
        // Since created EventEntity is containing non-existing variables, Converter should return null.
        [Test]
        public void EventEntityToEventModel_withNonExistingVariable()
        {
            // Create an EventEntity with invalid JSON data and an expected null CountlyEventModel.
            EventEntity entity = TestUtility.CreateEventEntity(0, TestUtility.CreateEventEntityJSONString(key: "Example Key", count: 5, sum: 10, customData: "\"This Doesn't Exist\": 5"));
            CountlyEventModel expected = TestUtility.CreateEventModel(key: "Example Key", sum: 10, count: 5);
            EventEntityToEventModel_base(entity, expected);
        }

        // 'ConvertJsonToDictionary' method in Converter.
        // We convert a Json to a Dictionary.
        // Conversion should be successful and Dictionary then be used
        public void JsonToDictionary_base(string Json, Dictionary<string, object> expected)
        {
            // Convert Json into Dictionary
            Dictionary<string, object> result = Converter.ConvertJsonToDictionary(Json, TestUtility.CreateLogHelper(true));
            if (expected == null) {
                Assert.IsNull(result);
            } else {
                Assert.IsNotNull(result);
                Assert.AreEqual(expected, result);
            }
        }

        // 'ConvertJsonToDictionary' method in Converter.
        // Provide a null Json to base function to convert.
        // It should not crash with a null Json and expected result is null
        [Test]
        public void ConvertJsonToDictionary_withNullJson()
        {
            JsonToDictionary_base(null, null);
        }

        // 'ConvertJsonToDictionary' method in Converter.
        // Provide an empty Json to base function to convert.
        // It should not crash with an empty Json and expected result is null
        [Test]
        public void ConvertJsonToDictionary_withEmptyJson()
        {
            string json = "";
            JsonToDictionary_base(json, null);
        }

        // 'ConvertJsonToDictionary' method in Converter.
        // Provide a valid Json to base function to convert.
        // With valid Json conversion should be successful.
        [Test]
        public void ConvertJsonToDictionary_withValidJson()
        {
            string json = @"
            {
                ""Class"": ""Fighter"",
                ""Level"": 20,
                ""OtherClass"": null,
                ""IsMultiClass"": false,
                ""Languages"": [""Elven"", ""Common""],
                ""Character"": {
                    ""Name"": ""John"",
                    ""Race"": ""Human"",
                }
            }";

            Dictionary<string, object> expected = new Dictionary<string, object>
            {
                { "Class", "Fighter" },
                { "Level", 20 },
                { "IsMultiClass", false },
                { "OtherClass", null},
                { "Languages", new JArray { "Elven", "Common" } },
                { "Character", new JObject
                    {
                        { "Name", "John" },
                        { "Race", "Human" },
                    }
                }
            };
            JsonToDictionary_base(json, expected);
        }

        public void MetricHelper_base(MetricHelper expectedMetrics, Dictionary<string, string> metricValues)
        {
            Countly countly = Countly.Instance;
            CountlyConfiguration config = TestUtility.createBaseConfig();
            config.MetricHelper = metricValues;

            countly.Init(config);

            Assert.AreEqual(expectedMetrics.OS, countly.MetricHelper.OS);
            Assert.AreEqual(expectedMetrics.OSVersion, countly.MetricHelper.OSVersion);
            Assert.AreEqual(expectedMetrics.AppVersion, countly.MetricHelper.AppVersion);
            Assert.AreEqual(expectedMetrics.Density, countly.MetricHelper.Density);
            Assert.AreEqual(expectedMetrics.Locale, countly.MetricHelper.Locale);
            Assert.AreEqual(expectedMetrics.Browser, countly.MetricHelper.Browser);
            Assert.AreEqual(expectedMetrics.BrowserVersion, countly.MetricHelper.BrowserVersion);
            Assert.AreEqual(expectedMetrics.Resolution, countly.MetricHelper.Resolution);
            Assert.AreEqual(expectedMetrics.Carrier, countly.MetricHelper.Carrier);
            Assert.AreEqual(expectedMetrics.Device, countly.MetricHelper.Device);
        }

        [Test]
        public void MetricHelper_PassedNullDictionary()
        {
            Dictionary<string, string> metricValues = null;
            MetricHelper metricHelper = new MetricHelper(metricValues);

            MetricHelper_base(metricHelper, metricValues);
        }

        [Test]
        public void MetricHelper_OverridenMetrics()
        {
            Dictionary<string, string> metricValues = new Dictionary<string, string>
            {
                {"OS", "SomeOtherOS"},
                {"OSVersion", "OverNineThousand"},
                {"appVersion", "5"}
            };

            MetricHelper metricHelper = new MetricHelper(metricValues);

            Assert.AreEqual(metricHelper.OS, "SomeOtherOS");
            Assert.AreEqual(metricHelper.OSVersion, "OverNineThousand");
            Assert.AreEqual(metricHelper.AppVersion, "5");

            MetricHelper_base(metricHelper, metricValues);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
