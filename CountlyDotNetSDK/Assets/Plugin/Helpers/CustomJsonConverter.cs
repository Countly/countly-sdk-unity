using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Plugin.Helpers
{
    class CustomJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, List<Helpers.KeyValuePair<string, object>>>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var customValue = value as Dictionary<string, List<KeyValuePair<string, object>>>;
            if (customValue == null || customValue.Count == 0) return;

            var list = customValue.First().Value as List<KeyValuePair<string, object>>;
            if (list == null || list.Count == 0) return;

            writer.WriteStartObject();
            foreach (var item in list)
            {
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
