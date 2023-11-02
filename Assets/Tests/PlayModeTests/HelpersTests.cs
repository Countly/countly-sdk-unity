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
        [SetUp]
        public void SetUp()
        {
            TestUtility.TestCleanup();
        }

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

        // 'MetricHelper' class in CountlySDK.Helpers
        // Provide a Dictionary<string,string> to override default values
        // If a dictionary is provided MetricHelper should return overriden value
        public void MetricHelper_base(Dictionary<string, string> overridenMetrics, MetricHelper expectedMetrics)
        {
            CountlyConfiguration config = TestUtility.createConfigWithOverridenMetrics(overridenMetrics);

            MetricHelper configMetricHelper = config.GetMetricHelper();

            Assert.AreEqual(expectedMetrics.OS, configMetricHelper.OS);
            Assert.AreEqual(expectedMetrics.OSVersion, configMetricHelper.OSVersion);
            Assert.AreEqual(expectedMetrics.AppVersion, configMetricHelper.AppVersion);
            Assert.AreEqual(expectedMetrics.Density, configMetricHelper.Density);
            Assert.AreEqual(expectedMetrics.Resolution, configMetricHelper.Resolution);
            Assert.AreEqual(expectedMetrics.Browser, configMetricHelper.Browser);
            Assert.AreEqual(expectedMetrics.BrowserVersion, configMetricHelper.BrowserVersion);
            Assert.AreEqual(expectedMetrics.Carrier, configMetricHelper.Carrier);
            Assert.AreEqual(expectedMetrics.Device, configMetricHelper.Device);
            Assert.AreEqual(expectedMetrics.Locale, configMetricHelper.Locale);
            Assert.AreEqual(expectedMetrics.Store, configMetricHelper.Store);
        }

        // Providing a null dictionary to MetricHelper
        // MetricHelper should return default values
        [Test]
        public void MetricHelper_nullDictionary()
        {
            MetricHelper expectedMetrics = new MetricHelper();

            expectedMetrics.overridenMetrics = null;

            MetricHelper_base(null, expectedMetrics);
        }

        // Providing a dictionary with valid keys to MetricHelper
        // It should return overriden values instead of default
        [Test]
        public void MetricHelper_overridenMetrics()
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>
            {
                { "OS", "NotWindows" },
                { "OSVersion", "NineThousand" }
            };

            MetricHelper expectedMetrics = new MetricHelper();
            expectedMetrics.overridenMetrics = overrides;

            MetricHelper_base(overrides, expectedMetrics);
        }

        // Providing a dictionary with valid key and setting it after creating MetricHelper
        // It should return the overriden value after setting it.
        [Test]
        public void MetricHelper_OverrideRuntime()
        {
            CountlyConfiguration config = TestUtility.createBaseConfig();
            MetricHelper metricHelper = new MetricHelper();
            MetricHelper configMetricHelper = config.GetMetricHelper();

            Assert.AreEqual(metricHelper.Carrier, configMetricHelper.Carrier);

            Dictionary<string, string> overrides = new Dictionary<string, string>
            {
                { "Carrier", "CountlyMobile" }
            };

            metricHelper.overridenMetrics = overrides;

            Assert.AreNotEqual(metricHelper.Carrier, configMetricHelper.Carrier);
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
