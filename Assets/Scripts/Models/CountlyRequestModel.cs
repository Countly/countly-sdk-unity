using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Assets.Scripts.Models
{
    [Serializable]
    class CountlyApiResponseModel
    {
        public string Result { get; set; }
    }

    public class CountlyRequestModel
    {
        bool IsRequestGetType { get; set; }
        string RequestUrl { get; set; }
        string RequestData { get; set; }
        DateTime RequestDateTime { get; set; }

        internal static Queue<CountlyRequestModel> TotalRequests { get; private set; }

        internal CountlyRequestModel(bool isRequestGetType, string requestUrl, string requestData, DateTime requestDateTime)
        {
            IsRequestGetType = isRequestGetType;
            RequestUrl = requestUrl;
            RequestData = requestData;
            RequestDateTime = requestDateTime;
        }

        internal static void InitializeRequestCollection()
        {
            TotalRequests = new Queue<CountlyRequestModel>();
        }

        internal void AddRequestToQueue()
        {
            if (TotalRequests.Count == Countly.StoredRequestLimit)
                RemoveRequestFromQueue();

            TotalRequests.Enqueue(this);
        }

        CountlyRequestModel RemoveRequestFromQueue()
        {
            return TotalRequests.Dequeue();
        }

        CountlyRequestModel GetRequestFromQueue()
        {
            return TotalRequests.Peek();
        }

        internal static void ProcessQueue()
        {
            var requests = TotalRequests.ToArray();
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