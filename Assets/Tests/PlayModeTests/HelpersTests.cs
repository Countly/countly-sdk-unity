using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using NUnit.Framework;
using Plugins.CountlySDK.Helpers;
using System.Collections.Generic;

namespace Tests
{
    public class HelpersTests
    {
        /// 'ConvertEventEntityToEventModel' method in Converter.
        /// We convert an EventEntity to CountlyEventModel.
        /// Conversion should be successful and EventModel then be used.
        [Test]
        public void EventEntityToEventModel()
        {
            // Create new EventEntity
            EventEntity entity = new EventEntity {
                Id = 1,
                Json = "{\"Id\": 1, \"Key\": \"SampleEvent\", \"Count\": 5, \"Sum\": 10, \"Duration\": 3.6, \"Segmentation\": { \"key1\": \"value1\", \"key2\": \"value2\" }}"
            };

            // Convert EventEntity to EventModel and assign it 
            CountlyEventModel model = null;
            model = Converter.ConvertEventEntityToEventModel(entity);

            // Verify if conversion is correct
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("SampleEvent", model.Key);
            Assert.AreEqual(5, model.Count);
            Assert.AreEqual(10, model.Sum);
            Assert.IsTrue(model.Segmentation.ContainsValue("value1"));
            Assert.IsTrue(model.Segmentation.ContainsValue("value2"));
        }

        /// 'ConvertJsonToDictionary' method in Converter.
        /// We convert a Json to a Dictionary.
        /// Conversion should be successful and Dictionary then be used
        [Test]
        public void JsonToDictionary()
        {
            // Create new EventEntity
            EventEntity entity = new EventEntity {
                Id = 1,
                Json = "{\"Id\": 2, \"Key\": \"NewEvent\", \"Count\": 7, \"Sum\": 12, \"Duration\": 5.7}"
            };

            // Convert Json into Dictionary
            Dictionary<string, object> result = Converter.ConvertJsonToDictionary(entity.Json);

            // Verify if conversion is correct
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.ContainsKey("Id"));
            Assert.IsTrue(result.ContainsKey("Key"));
            Assert.IsTrue(result.ContainsKey("Count"));
            Assert.IsTrue(result.ContainsKey("Sum"));
            Assert.IsTrue(result.ContainsKey("Duration"));
            Assert.AreEqual(2, result["Id"]);
            Assert.AreEqual("NewEvent", result["Key"]);
            Assert.AreEqual(7, result["Count"]);
            Assert.AreEqual(12, result["Sum"]);
            Assert.AreEqual(5.7, result["Duration"]);
        }
    }
}

