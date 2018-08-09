using Assets.Plugin.Scripts.Development;
using Helpers;
using System;
using UnityEngine;

namespace Assets.Plugin.Models
{
    [Serializable]
    class CountlyResponseModel
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
                        Debug.Log(reqModel.RequestUrl);
                        ProcessRequest(reqModel);
                        isProcessed = true;
                    }
                    catch (Exception ex)
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

        static string ProcessRequest(CountlyRequestModel model)
        {
            if (model.IsRequestGetType)
            {
                return CountlyHelper.Get(model.RequestUrl);
            }
            else
            {
                return CountlyHelper.Post(model.RequestUrl, model.RequestData);
            }
        }
    }
}
