using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Plugins.CountlySDK.Helpers
{
    class CustomJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, List<KeyValuePair<string, object>>>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Dictionary<string, List<KeyValuePair<string, object>>> customValue = value as Dictionary<string, List<KeyValuePair<string, object>>>;
            if (customValue == null || customValue.Count == 0) {
                return;
            }

            List<KeyValuePair<string, object>> list = customValue.First().Value as List<KeyValuePair<string, object>>;
            if (list == null || list.Count == 0) {
                return;
            }

            writer.WriteStartObject();
            foreach (KeyValuePair<string, object> item in list) {
                writer.WritePropertyName(item.Key);
                serializer.Serialize(writer, item.Value);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
