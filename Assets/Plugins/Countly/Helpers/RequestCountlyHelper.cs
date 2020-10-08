using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.Countly.Models;
using Plugins.Countly.Persistance.Repositories;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.Countly.Helpers
{
    public class RequestCountlyHelper
    {
        private readonly CountlyConfigModel _config;
        private readonly ICountlyUtils _countlyUtils;
        private readonly RequestRepository _requestRepo;

        internal RequestCountlyHelper(CountlyConfigModel config, ICountlyUtils countlyUtils, RequestRepository requestRepo)
        {
            _config = config;
            _countlyUtils = countlyUtils;
            _requestRepo = requestRepo;
        }

        private void AddRequestToQueue(CountlyRequestModel request)
        {
            if (_requestRepo.Count == _config.StoredRequestLimit)
                _requestRepo.Dequeue();

            _requestRepo.Enqueue(request);
        }

//        private CountlyRequestModel GetRequestFromQueue()
//        {
//            return _totalRequests.Peek();
//        }

        internal void ProcessQueue()
        {
            var requests = _requestRepo.Models.ToArray();
            Debug.Log("[Countly RequestCountlyHelper] Process queue, requests: " + requests.Length);
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
                        if (isProcessed) _requestRepo.Dequeue();
                    }                   
                }
            }
        }

        private void ProcessRequest(CountlyRequestModel model)
        {
            if (model.IsRequestGetType)
            {
                Task.Run(() => GetAsync(model.RequestUrl, false));
            }
            else
            {
                Task.Run(() => PostAsync(model.RequestUrl, model.RequestData, false));   
            }
        }


        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        private string BuildGetRequest(Dictionary<string, object> queryParams)
        {
            var requestStringBuilder = new StringBuilder();
            //Metrics added to each request
            foreach (var item in _countlyUtils.GetBaseParams())
            {
                requestStringBuilder.AppendFormat((item.Key != "app_key" ? "&" : string.Empty) + "{0}={1}",
                    UnityWebRequest.EscapeURL(item.Key), UnityWebRequest.EscapeURL(Convert.ToString(item.Value))); 
            }


            //Query params supplied for creating request
            foreach (var item in queryParams)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                {
                    requestStringBuilder.AppendFormat("&{0}={1}", UnityWebRequest.EscapeURL(item.Key),
                        UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
                }
            }


            if (!string.IsNullOrEmpty(_config.Salt))
            {
                // Create a SHA256   
                using (var sha256Hash = SHA256.Create())
                {
                    var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(requestStringBuilder + _config.Salt));
                    requestStringBuilder.Insert(0, _countlyUtils.GetBaseInputUrl());
                    return requestStringBuilder.AppendFormat("&checksum256={0}", _countlyUtils.GetStringFromBytes(data)).ToString();
                }
            }
            
            requestStringBuilder.Insert(0, _countlyUtils.GetBaseInputUrl());
            return requestStringBuilder.ToString();
        }

        /// <summary>
        ///     Serializes the post data (base params required for a request, and the supplied queryParams) in a string.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        private string BuildPostRequest(Dictionary<string, object> queryParams)
        {
            var baseParams = _countlyUtils.GetBaseParams();
            foreach (var item in queryParams)
            {
                baseParams.Add(UnityWebRequest.EscapeURL(item.Key), item.Value);
            }
                

            var data = JsonConvert.SerializeObject(baseParams);
            if (!string.IsNullOrEmpty(_config.Salt))
            {
                // Create a SHA256   
                using (var sha256Hash = SHA256.Create())
                {
                    var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data + _config.Salt));
                    baseParams.Add("checksum256", bytes);
                    return JsonConvert.SerializeObject(baseParams);
                }
            }

            return data;
        }

        /// <summary>
        ///     Uses Get/Post method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal CountlyResponse GetResponse(Dictionary<string, object> queryParams, bool addToRequestQueue = false)
        {
            var data = BuildPostRequest(queryParams);
            if (_config.EnablePost || data.Length > 1800)
            {
                return Post(_countlyUtils.GetBaseInputUrl(), data, addToRequestQueue);
            }
            return Get(BuildGetRequest(queryParams), addToRequestQueue);
        }

        /// <summary>
        ///     Uses GetAsync/PostAsync method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        internal Task<CountlyResponse> GetResponseAsync(Dictionary<string, object> queryParams,
            bool addToRequestQueue = false)
        {
            var data = BuildPostRequest(queryParams);
            if (_config.EnablePost || data.Length > 1800)
            {
                return Task.Run(
                    () => PostAsync(_countlyUtils.GetBaseInputUrl(), BuildPostRequest(queryParams), addToRequestQueue));    
            }
            return Task.Run(() => GetAsync(BuildGetRequest(queryParams), addToRequestQueue));
        }

        /// <summary>
        ///     Makes a GET request to the Counlty server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private CountlyResponse Get(string url, bool addToRequestQueue = true)
        {
            var countlyResponse = new CountlyResponse();
            if (!addToRequestQueue)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var res = reader.ReadToEnd();
                        countlyResponse.IsSuccess = !string.IsNullOrEmpty(res);
                        countlyResponse.Data = res;
                    }
                }
                catch (Exception ex)
                {
                    addToRequestQueue = true;
                    countlyResponse.ErrorMessage = ex.Message; 
                }
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(true, url, null, DateTime.UtcNow);
                AddRequestToQueue(requestModel);

                if (_config.EnableConsoleErrorLogging)
                {
                    Debug.Log("[Countly] RequestCountlyHelper: Added to Request Queue");
                }
            }

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] RequestCountlyHelper request: " + url + " response: " + countlyResponse.ToString());
            }

            return countlyResponse;
        }

        /// <summary>
        ///     Makes an Asynchronous GET request to the Countly server.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="addToRequestQueue"></param>
        /// <returns></returns>
        internal async Task<CountlyResponse> GetAsync(string url, bool addToRequestQueue = true)
        {
            var countlyResponse = new CountlyResponse();

            if (!addToRequestQueue)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var res = await reader.ReadToEndAsync();
                        countlyResponse.IsSuccess = !string.IsNullOrEmpty(res);
                        countlyResponse.Data = res;
                    }
                }
                catch (Exception ex)
                {
                    addToRequestQueue = true;
                    countlyResponse.ErrorMessage = ex.Message;
                }
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(true, url, null, DateTime.UtcNow);
                AddRequestToQueue(requestModel);

                if (_config.EnableConsoleErrorLogging)
                {
                    Debug.Log("[Countly] RequestCountlyHelper: Added to Request Queue");
                }
            }

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] RequestCountlyHelper request: " + url + " response: " + countlyResponse.ToString());
            }

            return countlyResponse;
        }

        /// <summary>
        ///     Makes a POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private CountlyResponse Post(string uri, string data, bool addToRequestQueue = true)
        {
            var countlyResponse = new CountlyResponse();
            if (!addToRequestQueue)
            {
                try
                {
                    var dataBytes = Encoding.UTF8.GetBytes(data);

                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.ContentLength = dataBytes.Length;
                    request.ContentType = "application/json";
                    request.Method = "POST";

                    using (var requestBody = request.GetRequestStream())
                    {
                        requestBody.Write(dataBytes, 0, dataBytes.Length);
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var res = reader.ReadToEnd();
                        countlyResponse.IsSuccess = !string.IsNullOrEmpty(res);
                        countlyResponse.Data = res;
                    }
                }
                catch (Exception ex)
                {
                    addToRequestQueue = true;
                    countlyResponse.ErrorMessage = ex.Message;
                }
            }
                
            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(false, uri, data, DateTime.UtcNow);
                AddRequestToQueue(requestModel);

                if (_config.EnableConsoleErrorLogging)
                {
                    Debug.Log("[Countly] RequestCountlyHelper: Added to Request Queue");
                }
            }

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] RequestCountlyHelper request: " + uri + " response: " + countlyResponse.ToString());
            }

            return countlyResponse;
        }

        /// <summary>
        ///     Makes an Asynchronous POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="addToRequestQueue"></param>
        /// <returns></returns>
        private async Task<CountlyResponse> PostAsync(string uri, string data, bool addToRequestQueue = true)
        {
            var countlyResponse = new CountlyResponse();

            if (!addToRequestQueue)
            {
                try
                {
                    var dataBytes = Encoding.UTF8.GetBytes(data);

                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.ContentLength = dataBytes.Length;
                    request.ContentType = "application/json";
                    request.Method = "POST";


                    using (var requestBody = request.GetRequestStream())
                    {
                        await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
                    }

                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var res = await reader.ReadToEndAsync();
                        countlyResponse.IsSuccess = !string.IsNullOrEmpty(res);
                        countlyResponse.Data = res;
                    }
                }
                catch (Exception ex)
                {
                    addToRequestQueue = true;
                    countlyResponse.ErrorMessage = ex.Message;
                }
            }
            
            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(false, uri, data, DateTime.UtcNow);
                AddRequestToQueue(requestModel);

                if (_config.EnableConsoleErrorLogging)
                {
                    Debug.Log("[Countly] RequestCountlyHelper: Added to Request Queue");
                }
            }

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] RequestCountlyHelper request: " + uri + " response: " + countlyResponse.ToString());
            }

            return countlyResponse;
        }

//        internal static void InvokeMethod(Type type, string methodName, object[] payLoadData)
//        {
//            Testing tt = new Testing();
//            MethodInfo info = type.GetMethod(methodName);
//            info.Invoke(tt, new object[] { payLoadData });
//        }
    }
}