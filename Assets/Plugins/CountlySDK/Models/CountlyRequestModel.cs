using System;
using Plugins.CountlySDK.Persistance;

namespace Plugins.CountlySDK.Models
{
    public class CountlyRequestModel : IModel
    {
        public CountlyRequestModel(bool isRequestGetType, string requestUrl, string requestData,
            DateTime requestDateTime)
        {
            IsRequestGetType = isRequestGetType;
            RequestUrl = requestUrl;
            RequestData = requestData;
            RequestDateTime = requestDateTime;
        }

        public bool IsRequestGetType { get; set; }
        public string RequestUrl { get; set; }
        public string RequestData { get; set; }
        public DateTime RequestDateTime { get; set; }
        public long Id { get; set; }

        public override string ToString()
        {
            return $"{nameof(IsRequestGetType)}: {IsRequestGetType}, {nameof(RequestUrl)}: {RequestUrl}, {nameof(RequestData)}: {RequestData}, {nameof(RequestDateTime)}: {RequestDateTime}, {nameof(Id)}: {Id}";
        }
    }
}
