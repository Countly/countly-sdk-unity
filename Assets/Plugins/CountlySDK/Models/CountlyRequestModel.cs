using System;
using Plugins.CountlySDK.Persistance;

namespace Plugins.CountlySDK.Models
{
    public class CountlyRequestModel : IModel
    {
        public CountlyRequestModel(string requestUrl, string requestData)
        {
            RequestUrl = requestUrl;
            RequestData = requestData;
        }


        public string RequestUrl { get; set; }
        public string RequestData { get; set; }
        public long Id { get; set; }

        public override string ToString()
        {
            return $"{nameof(RequestUrl)}: {RequestUrl}, {nameof(RequestData)}: {RequestData}, {nameof(Id)}: {Id}";
        }
    }
}
