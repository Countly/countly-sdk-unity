using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;

namespace Plugins.CountlySDK.Helpers
{
    public static class Converter
    {
        public static CountlyEventModel ConvertEventEntityToEventModel(EventEntity entity)
        {
            CountlyEventModel model = JsonConvert.DeserializeObject<CountlyEventModel>(entity.Json);
            model.Id = entity.Id;
            return model;
        }

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

        public static SegmentModel ConvertSegmentEntityToSegmentModel(SegmentEntity entity)
        {
            SegmentModel model = JsonConvert.DeserializeObject<SegmentModel>(entity.Json);
            model.Id = entity.Id;
            return model;
        }

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

        public static CountlyRequestModel ConvertRequestEntityToRequestModel(RequestEntity entity)
        {
            CountlyRequestModel model = JsonConvert.DeserializeObject<CountlyRequestModel>(entity.Json);
            model.Id = entity.Id;
            return model;
        }

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

        public static Dictionary<string, object> ConvertJsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}