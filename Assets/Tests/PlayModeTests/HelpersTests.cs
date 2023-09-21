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
                Json = "{\"Id\": 1, \"Key\": \"SampleEvent\"}"
            };

            // Convert EventEntity to EventModel and assign it 
            CountlyEventModel model = null;
            model = Converter.ConvertEventEntityToEventModel(entity);

            // Verify if conversion is correct
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("SampleEvent", model.Key);
        }

        /// 'ConvertJsonToDictionary' method in Converter.
        /// We convert a Json to a Dictionary.
        /// Conversion should be successful and Dictionary then be used
        [Test]
        public void JsonToDictionary()
        {
            // Create Json string
            string json = "{\"Id\": 0, \"Name\": \"John\"}";

            // Convert Json into Dictionary
            Dictionary<string, object> result = Converter.ConvertJsonToDictionary(json);

            // Verify if conversion is correct
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("Id"));
            Assert.AreEqual(0, result["Id"]);
            Assert.IsTrue(result.ContainsKey("Name"));
            Assert.AreEqual("John", result["Name"]);
        }
    }
}

