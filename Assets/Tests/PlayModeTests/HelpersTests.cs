using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using NUnit.Framework;
using Plugins.CountlySDK.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace Assets.Tests.PlayModeTests
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


        // 'MetricHelper' default values
        // no override provided
        // all values should be valid and not "null"
        [Test]
        public void MetricHelper_defaultValues()
        {
            MetricHelper defaultMetricHelper = new MetricHelper();

            Assert.NotNull(defaultMetricHelper.OS);
            Assert.NotNull(defaultMetricHelper.OSVersion);
            Assert.NotNull(defaultMetricHelper.AppVersion);
            Assert.NotNull(defaultMetricHelper.Density);
            Assert.NotNull(defaultMetricHelper.Resolution);
            Assert.NotNull(defaultMetricHelper.Device);
            Assert.NotNull(defaultMetricHelper.Locale);
        }

        // 'MetricHelper' override functionality
        // Providing a null dictionary to MetricHelper
        // MetricHelper should return default values
        [Test]
        public void MetricHelper_nullOverride()
        {
            MetricHelper expectedMetrics = new MetricHelper();
            expectedMetrics.overridenMetrics = null;

            MetricHelper defaultMetricHelper = new MetricHelper();

            Assert.AreEqual(expectedMetrics.OS, defaultMetricHelper.OS);
            Assert.AreEqual(expectedMetrics.OSVersion, defaultMetricHelper.OSVersion);
            Assert.AreEqual(expectedMetrics.AppVersion, defaultMetricHelper.AppVersion);
            Assert.AreEqual(expectedMetrics.Density, defaultMetricHelper.Density);
            Assert.AreEqual(expectedMetrics.Resolution, defaultMetricHelper.Resolution);
            Assert.AreEqual(expectedMetrics.Device, defaultMetricHelper.Device);
            Assert.AreEqual(expectedMetrics.Locale, defaultMetricHelper.Locale);
        }

        // 'MetricHelper' override functionality
        // Providing an empty dictionary to MetricHelper
        // MetricHelper should return default values
        [Test]
        public void MetricHelper_emptyOverride()
        {
            MetricHelper expectedMetrics = new MetricHelper();
            expectedMetrics.overridenMetrics = new Dictionary<string, string>();
            MetricHelper defaultMetricHelper = new MetricHelper();

            Assert.AreEqual(expectedMetrics.OS, defaultMetricHelper.OS);
            Assert.AreEqual(expectedMetrics.OSVersion, defaultMetricHelper.OSVersion);
            Assert.AreEqual(expectedMetrics.AppVersion, defaultMetricHelper.AppVersion);
            Assert.AreEqual(expectedMetrics.Density, defaultMetricHelper.Density);
            Assert.AreEqual(expectedMetrics.Resolution, defaultMetricHelper.Resolution);
            Assert.AreEqual(expectedMetrics.Device, defaultMetricHelper.Device);
            Assert.AreEqual(expectedMetrics.Locale, defaultMetricHelper.Locale);
        }

        // 'MetricHelper' override functionality
        // Providing a dictionary with valid keys to MetricHelper
        // It should return overriden values instead of default
        [Test]
        public void MetricHelper_overridenMetrics()
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>
            {
                { "_os", "NotWindows" },
                { "_os_version", "NineThousand" },
                { "_device", "Calculator"},
                { "_resolution", "144p"},
                { "_app_version","1" },
                { "_density", "unknown"},
                { "_locale", "English"}
            };

            MetricHelper expectedMetrics = new MetricHelper();
            expectedMetrics.overridenMetrics = overrides;

            Assert.AreEqual(expectedMetrics.OS, "NotWindows");
            Assert.AreEqual(expectedMetrics.OSVersion, "NineThousand");
            Assert.AreEqual(expectedMetrics.AppVersion, "1");
            Assert.AreEqual(expectedMetrics.Density, "unknown");
            Assert.AreEqual(expectedMetrics.Resolution, "144p");
            Assert.AreEqual(expectedMetrics.Device, "Calculator");
            Assert.AreEqual(expectedMetrics.Locale, "English");
        }

        // 'buildMetricJSON' function in MetricHelper
        // Providing a dictionary with a custom metric to MetricHelper
        // Custom metric should be in the JSON built by MetricHelper
        [Test]
        public void MetricHelper_customMetric()
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>
            {
                { "CustomMetric1", "Kobe" },
                { "CustomMetric2", "Jordan" },
                { "_os", "NotWindows"},
                { "_os_version", "NineThousand" },
                { "_device", "Calculator"},
                { "_resolution", "144p"},
                { "_app_version","1" },
                { "_density", "unknown"},
                { "_locale", "English"}
            };

            MetricHelper expectedMetrics = new MetricHelper(overrides);
            string metricJSON = expectedMetrics.buildMetricJSON();

            Dictionary<string, object> metrics = Converter.ConvertJsonToDictionary(metricJSON, null);

            Assert.IsTrue(metrics.ContainsKey("CustomMetric1"));
            Assert.IsTrue(metrics.ContainsKey("CustomMetric2"));

            Assert.AreEqual(metrics["CustomMetric1"], "Kobe");
            Assert.AreEqual(metrics["CustomMetric2"], "Jordan");
            Assert.AreEqual(metrics["_os"], "NotWindows");
            Assert.AreEqual(metrics["_os_version"], "NineThousand");
            Assert.AreEqual(metrics["_app_version"], "1");
            Assert.AreEqual(metrics["_density"], "unknown");
            Assert.AreEqual(metrics["_resolution"], "144p");
            Assert.AreEqual(metrics["_device"], "Calculator");
            Assert.AreEqual(metrics["_locale"], "English");
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
