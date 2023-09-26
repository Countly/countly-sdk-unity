using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;

namespace Plugins.CountlySDK.Helpers
{
    // Providing methods for converting data between different models and JSON representations.
    public static class Converter
    {
        // <summary>
        // Converts an EventEntity object to a CountlyEventModel object.
        // </summary>
        // <returns>
        // CountlyEventModel object representing the converted data, or null if the input entity is null or invalid.
        // </returns>
        public static CountlyEventModel ConvertEventEntityToEventModel(EventEntity entity, CountlyLogHelper L)
        {
            // Check if the input EventEntity is null
            if (entity == null) {
                L.Warning("[Converter] 'ConvertEventEntityToEventModel': EventEntity variable is null");
                return null;
            }

            // Check if the JSON in the EventEntity is valid
            if (!IsValidJson(entity.Json)) {
                L.Warning("[Converter] 'ConvertEventEntityToEventModel': EventEntity contains invalid JSON");
                return null;
            }

            try {
                // Deserialize the JSON into a CountlyEventModel with settings that ignore additional properties
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                CountlyEventModel model = JsonConvert.DeserializeObject<CountlyEventModel>(entity.Json, settings);

                // Assign the Id property from the entity
                model.Id = entity.Id;

                return model;

            } catch (JsonSerializationException ex) {
                // Handle JSON serialization error
                L.Warning($"[Converter] 'ConvertEventEntityToEventModel': JSON serialization error: {ex.Message}");
                return null;
            } catch (JsonException ex) {
                // Handle JSON deserialization error
                L.Warning($"[Converter] 'ConvertEventEntityToEventModel': JSON deserialization error: {ex.Message}");
                return null;
            } catch (FormatException ex) {
                L.Warning($"[Converter] 'ConvertEventEntityToEventModel': JSON deserialization error: {ex.Message}");
                return null;
            }
        }

        // Function to check if a provided JSON string is valid or not
        private static bool IsValidJson(string json)
        {
            try {
                JToken.Parse(json);
                return true;
            } catch (JsonException) {
                return false;
            }
        }

        // <summary>
        // Converts a CountlyEventModel object to an EventEntity object.
        // </summary>
        // <returns>EventEntity object representing the converted data.</returns>
        public static EventEntity ConvertEventModelToEventEntity(CountlyEventModel model, long id)
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            model.Id = id;

            return new EventEntity {
                Id = id,
                Json = json
            };
        }

        // <summary>
        // Converts a SegmentEntity object to a SegmentModel object.
        // </summary>
        // <returns>SegmentModel object representing the converted data.</returns>
        public static SegmentModel ConvertSegmentEntityToSegmentModel(SegmentEntity entity)
        {
            SegmentModel model = JsonConvert.DeserializeObject<SegmentModel>(entity.Json);
            model.Id = entity.Id;
            return model;
        }

        // <summary>
        // Converts a SegmentModel object to a SegmentEntity object.
        // </summary>
        // <returns>SegmentEntity object representing the converted data.</returns>
        public static SegmentEntity ConvertSegmentModelToSegmentEntity(SegmentModel model, long id)
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            model.Id = id;

            return new SegmentEntity {
                Id = id,
                Json = json
            };
        }

        // <summary>
        // Converts a RequestEntity object to a CountlyRequestModel object.
        // </summary>
        // <returns>CountlyRequestModel object representing the converted data.</returns>
        public static CountlyRequestModel ConvertRequestEntityToRequestModel(RequestEntity entity)
        {
            CountlyRequestModel model = JsonConvert.DeserializeObject<CountlyRequestModel>(entity.Json);
            model.Id = entity.Id;
            return model;
        }

        // <summary>
        // Converts a CountlyRequestModel object to a RequestEntity object.
        // </summary>
        // <returns>RequestEntity object representing the converted data.</returns>
        public static RequestEntity ConvertRequestModelToRequestEntity(CountlyRequestModel model, long id)
        {
            string json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            model.Id = id;

            return new RequestEntity {
                Id = id,
                Json = json
            };
        }

        // <summary>
        // Converts a JSON string to a Dictionary of string and object.
        // </summary>
        // <returns>Dictionary of string and object representing the converted JSON data, or null if the input JSON string is null.</returns>
        public static Dictionary<string, object> ConvertJsonToDictionary(string json, CountlyLogHelper L)
        {
            if (json == null) {
                L.Warning("[Converter] 'ConvertJsonToDictionary': Provided Json is null");
                return null;
            }
            // Check if the JSON in is valid
            if (!IsValidJson(json)) {
                L.Warning("[Converter] 'ConvertJsonToDictionary': Invalid Json is provided");
                return null;
            }

            try {
                // Deserialize the JSON into a JObject
                JObject jsonObject = JObject.Parse(json);

                // Recursively convert the JObject to a Dictionary<string, object> with type conversion
                return ConvertJObjectToDictionary(jsonObject);
            } catch (JsonReaderException ex) {
                return null;
            }
        }

        // Recursively converts a JSON object represented as a JObject into a Dictionary<string, object>.
        // Returns a Dictionary<string, object> representation of the JSON object.
        private static Dictionary<string, object> ConvertJObjectToDictionary(JObject jsonObject)
        {
            // Create a new Dictionary to store the converted data.
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            // Iterate through the properties of the JSON object.
            foreach (JProperty property in jsonObject.Properties()) {
                // Get the name of the property.
                string propertyName = property.Name;

                // Get the JSON token associated with the property.
                JToken token = property.Value;

                // Check the type of the token.
                if (token.Type == JTokenType.Object) {
                    // If it's an object, recursively convert it to a Dictionary and store it.
                    dictionary[propertyName] = ConvertJObjectToDictionary((JObject)token);
                } else if (token.Type == JTokenType.Array) {
                    // If it's an array, convert it to a List<object>.
                    List<object> list = new List<object>();

                    // Iterate through the items in the array.
                    foreach (JToken item in (JArray)token) {
                        if (item.Type == JTokenType.Object) {
                            // If an item is an object within the array, recursively convert it.
                            list.Add(ConvertJObjectToDictionary((JObject)item));
                        } else {
                            // Otherwise, convert the item directly and add it to the list.
                            list.Add(item.ToObject<object>());
                        }
                    }

                    // Store the List<object> in the dictionary.
                    dictionary[propertyName] = list;
                } else {
                    // If it's neither an object nor an array, convert the token directly and store it.
                    dictionary[propertyName] = token.ToObject<object>();
                }
            }

            // Return the final Dictionary representation of the JSON object.
            return dictionary;
        }
    }
}
