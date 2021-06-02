using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Repositories;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.CountlySDK.Helpers
{
    public class RequestCountlyHelper
    {
        private bool isQueueBeingProcess = false;

        private readonly CountlyLogHelper Log;
        private readonly CountlyUtils _countlyUtils;
        private readonly CountlyConfiguration _config;
        internal readonly RequestRepository _requestRepo;

        internal RequestCountlyHelper(CountlyConfiguration config, CountlyLogHelper log, CountlyUtils countlyUtils, RequestRepository requestRepo)
        {
            Log = log;
            _config = config;
            _countlyUtils = countlyUtils;
            _requestRepo = requestRepo;
        }

        private async Task AddRequestToQueue(CountlyRequestModel request)
        {

            Log.Verbose("[RequestCountlyHelper] AddRequestToQueue: " + request.ToString());

            if (_config.EnableTestMode) {
                return;
            }

            if (_requestRepo.Count == _config.StoredRequestLimit) {

                Log.Warning("[RequestCountlyHelper] Request Queue is full. Dropping the oldest request.");

                _requestRepo.Dequeue();
            }

            _requestRepo.Enqueue(request);

            await ProcessQueue();
        }

        internal async Task ProcessQueue()
        {
            if (isQueueBeingProcess) {
                return;
            }

            isQueueBeingProcess = true;
            CountlyRequestModel[] requests = _requestRepo.Models.ToArray();

            Log.Verbose("[RequestCountlyHelper] Process queue, requests: " + requests.Length);

            foreach (CountlyRequestModel reqModel in requests) {
                CountlyResponse response = await ProcessRequest(reqModel);

                if (!response.IsSuccess) {
                    Log.Verbose("[RequestCountlyHelper] ProcessQueue: Request fail, " + response.ToString());
                    break;
                }

                _requestRepo.Dequeue();
            }
            isQueueBeingProcess = false;
        }

        private async Task<CountlyResponse> ProcessRequest(CountlyRequestModel model)
        {
            Log.Verbose("[RequestCountlyHelper] Process request, request: " + model.ToString());

            if (model.IsRequestGetType) {
                return await Task.Run(() => GetAsync(model.RequestUrl));
            } else {
                return await Task.Run(() => PostAsync(model.RequestUrl, model.RequestData));
            }
        }

        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal string BuildGetRequest(Dictionary<string, object> queryParams)
        {
            StringBuilder requestStringBuilder = new StringBuilder();
            //Metrics added to each request
            foreach (KeyValuePair<string, object> item in _countlyUtils.GetBaseParams()) {
                requestStringBuilder.AppendFormat((item.Key != "app_key" ? "&" : string.Empty) + "{0}={1}",
                    UnityWebRequest.EscapeURL(item.Key), UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
            }


            //Query params supplied for creating request
            foreach (KeyValuePair<string, object> item in queryParams) {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null) {
                    requestStringBuilder.AppendFormat("&{0}={1}", UnityWebRequest.EscapeURL(item.Key),
                        UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
                }
            }


            if (!string.IsNullOrEmpty(_config.Salt)) {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create()) {
                    byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(requestStringBuilder + _config.Salt));
                    requestStringBuilder.Insert(0, _countlyUtils.ServerInputUrl);
                    return requestStringBuilder.AppendFormat("&checksum256={0}", _countlyUtils.GetStringFromBytes(data)).ToString();
                }
            }

            requestStringBuilder.Insert(0, _countlyUtils.ServerInputUrl);
            return requestStringBuilder.ToString();
        }

        /// <summary>
        ///     Serializes the post data (base params required for a request, and the supplied queryParams) in a string.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal string BuildPostRequest(Dictionary<string, object> queryParams)
        {
            Dictionary<string, object> baseParams = _countlyUtils.GetBaseParams();
            foreach (KeyValuePair<string, object> item in queryParams) {
                baseParams.Add(UnityWebRequest.EscapeURL(item.Key), item.Value);
            }


            string data = JsonConvert.SerializeObject(baseParams);
            if (!string.IsNullOrEmpty(_config.Salt)) {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create()) {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data + _config.Salt));
                    baseParams.Add("checksum256", bytes);
                    return JsonConvert.SerializeObject(baseParams);
                }
            }

            return data;
        }

        /// <summary>
        ///     Uses GetAsync/PostAsync method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        internal async Task GetResponseAsync(Dictionary<string, object> queryParams)
        {
            CountlyRequestModel requestModel;
            string data = BuildPostRequest(queryParams);
            if (_config.EnablePost || data.Length > 1800) {
                requestModel = new CountlyRequestModel(false, _countlyUtils.ServerInputUrl, BuildPostRequest(queryParams), DateTime.UtcNow);
            } else {
                requestModel = new CountlyRequestModel(true, BuildGetRequest(queryParams), null, DateTime.UtcNow);
            }

            await AddRequestToQueue(requestModel);
        }

        /// <summary>
        ///     Makes an Asynchronous GET request to the Countly server.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="addToRequestQueue"></param>
        /// <returns></returns>
        internal async Task<CountlyResponse> GetAsync(string url)
        {
            CountlyResponse countlyResponse = new CountlyResponse();

            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync()) {
                    int code = (int)response.StatusCode;
                    using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream)) {
                        string res = await reader.ReadToEndAsync();
                        countlyResponse.StatusCode = code;
                        countlyResponse.IsSuccess = true;
                        countlyResponse.Data = res;
                    }

                }
            } catch (WebException ex) {
                countlyResponse.ErrorMessage = ex.Message;
                if (ex.Response != null) {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    int code = (int)response.StatusCode;
                    using (Stream stream = ex.Response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream)) {
                        string res = await reader.ReadToEndAsync();
                        countlyResponse.StatusCode = code;
                        countlyResponse.IsSuccess = false;
                        countlyResponse.Data = res;
                    }
                }
               
            }

            Log.Verbose("[RequestCountlyHelper] GetAsync request: " + url + " response: " + countlyResponse.ToString());

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
        private async Task<CountlyResponse> PostAsync(string uri, string data)
        {
            CountlyResponse countlyResponse = new CountlyResponse();

            try {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/json";
                request.Method = "POST";


                using (Stream requestBody = request.GetRequestStream()) {
                    await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync()) {
                    int code = (int)response.StatusCode;
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream)) {
                        string res = await reader.ReadToEndAsync();
                        countlyResponse.StatusCode = code;
                        countlyResponse.IsSuccess = true;
                        countlyResponse.Data = res;
                    }
                }
            } catch (WebException ex) {
                countlyResponse.ErrorMessage = ex.Message;
                if (ex.Response != null) {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    int code = (int)response.StatusCode;
                    using (Stream stream = ex.Response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream)) {
                        string res = await reader.ReadToEndAsync();
                        countlyResponse.StatusCode = code;
                        countlyResponse.IsSuccess = false;
                        countlyResponse.Data = res;
                    }
                }

            }

            Log.Verbose("[RequestCountlyHelper] PostAsync request: " + uri + " response: " + countlyResponse.ToString());

            return countlyResponse;
        }

    }
}