using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Repositories;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.CountlySDK.Helpers
{
    public class RequestCountlyHelper
    {
        private bool _isQueueBeingProcess;

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

        internal void AddRequestToQueue(CountlyRequestModel request)
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
            if (_isQueueBeingProcess) {
                return;
            }

            _isQueueBeingProcess = true;
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

            _isQueueBeingProcess = false;
        }

        private async Task<CountlyResponse> ProcessRequest(CountlyRequestModel model)
        {
            Log.Verbose("[RequestCountlyHelper] Process request, request: " + model);

            if (_config.EnablePost || model.RequestData.Length > 1800) {
                return await Task.Run(() => PostAsync(_countlyUtils.ServerInputUrl, model.RequestData));
            }
            return await Task.Run(() => GetAsync(_countlyUtils.ServerInputUrl ,model.RequestData));
        }

        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal string BuildRequest(string data)
        {
            Dictionary<string, object> queryParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            StringBuilder requestStringBuilder = new StringBuilder();
            //Query params supplied for creating request
            foreach (KeyValuePair<string, object> item in queryParams) {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null) {
                    requestStringBuilder.AppendFormat("&{0}={1}", UnityWebRequest.EscapeURL(item.Key),
                        UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
                }
            }

            String query = requestStringBuilder.ToString();
            string result = query.Remove(0, 1); //remove extra '&'

            if (!string.IsNullOrEmpty(_config.Salt)) {
                // Create a SHA256
                using (SHA256 sha256Hash = SHA256.Create()) {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(result + _config.Salt));
                    string hex = _countlyUtils.GetStringFromBytes(bytes);
                    Log.Debug("BuildGetRequest: query = " + result + ", checksum256 = " + hex);

                    result += "&checksum256=" + hex;
                }
            }



            return result;
        }

        /// <summary>
        ///  An internal function to add a request to request queue.
        /// </summary>
        internal void AddToRequestQueue(Dictionary<string, object> queryParams)
        {
            //Metrics added to each request
            Dictionary<string, object> requestData = _countlyUtils.GetBaseParams();
            foreach (KeyValuePair<string, object> item in queryParams) {
                requestData.Add(item.Key, item.Value);
            }

            string data = JsonConvert.SerializeObject(requestData);
            CountlyRequestModel requestModel = new CountlyRequestModel(null,  data);

            AddRequestToQueue(requestModel);
        }

        /// <summary>
        ///     Makes an Asynchronous GET request to the Countly server.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="addToRequestQueue"></param>
        /// <returns></returns>
        internal async Task<CountlyResponse> GetAsync(string uri, string data)
        {

            Log.Verbose("[RequestCountlyHelper] GetAsync request: " + uri + " params: " + data);

            CountlyResponse countlyResponse = new CountlyResponse();
            string query = BuildRequest(data);
            string url = uri + query;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + query);
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

            Log.Verbose("[RequestCountlyHelper] GetAsync request: " + url + " params: " + query + " response: " + countlyResponse.ToString());

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
        internal async Task<CountlyResponse> PostAsync(string uri, string data)
        {
            CountlyResponse countlyResponse = new CountlyResponse();

            try {
                string query = BuildRequest(data);
                byte[] dataBytes = Encoding.ASCII.GetBytes(query);

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

            Log.Verbose("[RequestCountlyHelper] PostAsync request: " + uri + " body: " + data + " response: " + countlyResponse.ToString());

            return countlyResponse;
        }

    }
}
