using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using NUnit.Framework;
using Plugins.CountlySDK.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests
{
    public class HelpersTests
    {
        /// 'ConvertEventEntityToEventModel' method in Converter.
        /// We convert an EventEntity to CountlyEventModel.
        /// Conversion should be successful and EventModel then be used.
        public void EventEntityToEventModel_base(EventEntity entity)
        {
            // Convert EventEntity to EventModel and assign it 
            CountlyEventModel model = null;
            model = Converter.ConvertEventEntityToEventModel(entity);

            if (entity != null) {
                // Verify if conversion is correct with Valid Entity
                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.Id);
                Assert.AreEqual("SampleEvent", model.Key);
                Assert.AreEqual(5, model.Count);
                Assert.AreEqual(10, model.Sum);
                Assert.IsTrue(model.Segmentation.ContainsValue("value1"));
                Assert.IsTrue(model.Segmentation.ContainsValue("value2"));
            }
            if (entity == null) {
                // Verify if model and entity is null
                Assert.IsNull(model);
                Assert.IsNull(entity);
            }
        }

        // Passes a valid EventEntity into the base function.
        // Since a valid EventEntity is passed conversion should be successful.
        [Test]
        public void EventEntityToEventModel_withValidEntity()
        {
            // Create new EventEntity
            EventEntity entity = new EventEntity {
                Id = 1,
                Json = "{\"Id\": 1, \"Key\": \"SampleEvent\", \"Count\": 5, \"Sum\": 10, \"Duration\": 3.6, \"Segmentation\": { \"key1\": \"value1\", \"key2\": \"value2\" }}"
            };
            EventEntityToEventModel_base(entity);
        }

        // Passes a null EventEntity into the base function.
        // It should not crash with a null EventEntity 
        [Test]
        public void EventEntityToEventModel_withNullEntity()
        {
            // Create null EventEntity
            EventEntity entity = new EventEntity { };
            entity = null;

            EventEntityToEventModel_base(entity);
        }

        /// 'ConvertJsonToDictionary' method in Converter.
        /// We convert a Json to a Dictionary.
        /// Conversion should be successful and Dictionary then be used
        public void JsonToDictionary_base(string Json)
        {
            // Convert Json into Dictionary
            Dictionary<string, object> result = Converter.ConvertJsonToDictionary(Json);

            // Verify if conversion is correct with valid Json
            if (Json != null) {
                Assert.NotNull(result);

                // Now, you can add assertions to verify the contents of the dictionary
                Assert.NotNull(result); // Ensure that the result is not null

                // Verify specific values in the dictionary
                Assert.AreEqual("John", result["Name"]);
                Assert.AreEqual(30, result["Age"]);
                Assert.AreEqual("New York", result["City"]);
                Assert.AreEqual(false, result["IsStudent"]);

                if (result.ContainsKey("Languages")) {
                    // Deserialize the "Languages" field as an array of strings
                    string[] languages = JsonConvert.DeserializeObject<string[]>(result["Languages"].ToString());

                    Assert.NotNull(languages);
                    Assert.AreEqual(2, languages.Length);
                    Assert.AreEqual("English", languages[0]);
                    Assert.AreEqual("Spanish", languages[1]);
                }
            }
            if (Json == null) {
                Assert.IsNull(result);
                Assert.IsNull(Json);
            }
        }

        // Provide a valid Json to base function to convert.
        // With valid Json conversion should be successful.
        [Test]
        public void ConvertJsonToDictionary_withValidJson()
        {
            string json = "{\"Name\": \"John\", \"Age\": 30, \"City\": \"New York\", \"IsStudent\": false, \"Languages\": [\"English\", \"Spanish\"]}";

            JsonToDictionary_base(json);
        }

        // Provide a null Json to base function to convert.
        // It should not crash with a null Json
        [Test]
        public void ConvertJsonToDictionary_withNullJson()
        {
            string json = null;

            JsonToDictionary_base(json);
        }
    }
}

