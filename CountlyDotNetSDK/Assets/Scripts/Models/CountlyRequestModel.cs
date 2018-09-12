using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    [Serializable]
    class CountlyApiResponseModel
    {
        public string Result { get; set; }
    }

    class CountlyRequestModel
    {
        bool IsRequestGetType { get; set; }
        string RequestUrl { get; set; }
        string RequestData { get; set; }
        DateTime RequestDateTime { get; set; }

        internal CountlyRequestModel(bool isRequestGetType, string requestUrl, string requestData, DateTime requestDateTime)
        {
            IsRequestGetType = isRequestGetType;
            RequestUrl = requestUrl;
            RequestData = requestData;
            RequestDateTime = requestDateTime;
        }

        internal void AddRequestToQueue()
        {
            if (Countly.TotalRequests.Count == Countly.StoredRequestLimit)
                RemoveRequestFromQueue();

            Countly.TotalRequests.Enqueue(this);
        }

        CountlyRequestModel RemoveRequestFromQueue()
        {
            return Countly.TotalRequests.Dequeue();
        }

        CountlyRequestModel GetRequestFromQueue()
        {
            return Countly.TotalRequests.Peek();
        }

        internal static void ProcessQueue()
        {
            var requests = Countly.TotalRequests.ToArray();
            foreach (var reqModel in requests)
            {
                var isProcessed = false;
                var retryCount = 0;
                while (!isProcessed && retryCount < 3)
                {
                    try
                    {
                        ProcessRequest(reqModel);
                        isProcessed = true;
                    }
                    catch
                    {
                        retryCount++;
                        isProcessed = false;
                    }
                    finally
                    {
                        if (isProcessed)
                        {
                            reqModel.RemoveRequestFromQueue();
                        }
                    }
                }
            }
        }

        static void ProcessRequest(CountlyRequestModel model)
        {
            if (model.IsRequestGetType)
            {
                Task.Run(() => CountlyHelper.GetAsync(model.RequestUrl));
            }
            else
            {
                Task.Run(() => CountlyHelper.PostAsync(model.RequestUrl, model.RequestData));
            }
        }
    }
}