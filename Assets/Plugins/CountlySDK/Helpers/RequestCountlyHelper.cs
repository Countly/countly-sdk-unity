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

        private void AddRequestToQueue(CountlyRequestModel request)
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
            Log.Verbose("[RequestCountlyHelper] Process request, request: " + model);

            if (_config.EnablePost || model.RequestData.Length > 1800) {
                return await Task.Run(() => PostAsync(_countlyUtils.ServerInputUrl, model.RequestData));
            } else {
                return await Task.Run(() => GetAsync(_countlyUtils + model.RequestUrl));

            }
        }

        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal string BuildGetRequest()
        {
            Dictionary<string, object> queryParams = Json
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
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(requestStringBuilder + _config.Salt));
                    string hex = _countlyUtils.GetStringFromBytes(bytes);
                    Log.Debug("BuildGetRequest: query = " + requestStringBuilder + ", checksum256 = " + hex);

                    requestStringBuilder.AppendFormat("&checksum256={0}", hex);
                }
            }

            return requestStringBuilder.ToString();
        }

        /// <summary>
        ///  An internal function to add a request to request queue.
        /// </summary>
        internal void AddToRequestQueue(Dictionary<string, object> queryParams)
        {
            string data = JsonConvert.SerializeObject(queryParams);
            CountlyRequestModel requestModel = new CountlyRequestModel(null,  data);

            AddRequestToQueue(requestModel);
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
                BuildGetRequest();
                byte[] dataBytes = Encoding.ASCII.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/x-www-form-urlencoded";
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
