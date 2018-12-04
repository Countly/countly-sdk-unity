using Assets.Scripts.Main.Development;
using Assets.Scripts.Main.Testing;
using Assets.Scripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    class CountlyHelper
    {
        #region Fields

        internal static readonly string OperationSystem = SystemInfo.operatingSystem;

        #endregion

        #region Methods

        internal static string GetUniqueDeviceID() => SystemInfo.deviceUniqueIdentifier;

        /// <summary>
        /// Gets the base url to make requests to the Countly server.
        /// </summary>
        /// <returns></returns>
        internal static string GetBaseUrl()
        {
            return string.Format(Countly.ServerUrl[Countly.ServerUrl.Length - 1] == '/' ? "{0}i?" : "{0}/i?", Countly.ServerUrl);
        }

        /// <summary>
        /// Gets the least set of paramas required to be sent along with each request.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, object> GetBaseParams()
        {
            var baseParams = new Dictionary<string, object>
            {
                { "app_key", Countly.AppKey },
                { "device_id", Countly.DeviceId }
            };

            foreach (var item in TimeMetricModel.GetTimeMetricModel())
                baseParams.Add(item.Key, item.Value);

            if (!string.IsNullOrEmpty(Countly.CountryCode))
                baseParams.Add("country_code", Countly.CountryCode);
            if (!string.IsNullOrEmpty(Countly.City))
                baseParams.Add("city", Countly.City);
            if (Countly.Location != null)
                baseParams.Add("location", Countly.Location);

            return baseParams;
        }

        /// <summary>
        /// Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters. 
        /// The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal static string BuildGetRequest(Dictionary<string, object> queryParams)
        {
            StringBuilder request = new StringBuilder();
            //Metrics added to each request
            foreach (var item in GetBaseParams())
            {
                request.AppendFormat((item.Key != "app_key" ? "&" : string.Empty) + "{0}={1}", WWW.EscapeURL(item.Key), WWW.EscapeURL(Convert.ToString(item.Value)));
            }

            //Query params supplied for creating request
            foreach (var item in queryParams)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                    request.AppendFormat("&{0}={1}", WWW.EscapeURL(item.Key), WWW.EscapeURL(Convert.ToString(item.Value)));
            }

            if (!string.IsNullOrEmpty(Countly.Salt))
            {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(request.ToString() + Countly.Salt));
                    request.Insert(0, GetBaseUrl());
                    return request.AppendFormat("&checksum256={0}", GetStringFromBytes(data)).ToString();
                }
            }
            else
            {
                request.Insert(0, GetBaseUrl());
                return request.ToString();
            }
        }

        /// <summary>
        /// Serializes the post data (base params required for a request, and the supplied queryParams) in a string.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal static string BuildPostRequest(Dictionary<string, object> queryParams)
        {
            var baseParams = GetBaseParams();
            foreach (var item in queryParams)
                baseParams.Add(WWW.EscapeURL(item.Key), item.Value);

            var data = JsonConvert.SerializeObject(baseParams);
            if (!string.IsNullOrEmpty(Countly.Salt))
            {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data + Countly.Salt));
                    baseParams.Add("checksum256", bytes);
                    return JsonConvert.SerializeObject(baseParams);
                }
            }
            else
            {
                return data;
            }
        }

        /// <summary>
        /// Uses Get/Post method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        internal static CountlyResponse GetResponse(Dictionary<string, object> queryParams, bool addToRequestQueue = false)
        {
            var data = BuildPostRequest(queryParams);
            if (Countly.PostRequestEnabled || data.Length > 1800)
            {
                return Post(GetBaseUrl(), data, addToRequestQueue);
            }
            else
            {
                return Get(BuildGetRequest(queryParams), addToRequestQueue);
            }
        }

        /// <summary>
        /// Uses GetAsync/PostAsync method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        internal static Task<CountlyResponse> GetResponseAsync(Dictionary<string, object> queryParams,
                                                                bool addToRequestQueue = false)
        {
            var data = BuildPostRequest(queryParams);
            if (Countly.PostRequestEnabled || data.Length > 1800)
            {
                return Task.Run(() => PostAsync(GetBaseUrl(), data, addToRequestQueue));
            }
            else
            {
                return Task.Run(() => GetAsync(BuildGetRequest(queryParams), addToRequestQueue));
            }
        }

        /// <summary>
        /// Makes a GET request to the Counlty server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static CountlyResponse Get(string url, bool addToRequestQueue = false)
        {
            var countlyResponse = new CountlyResponse();
            try
            {
                if (addToRequestQueue)
                {
                    throw new Exception("Request added to queue.");
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var res = JsonConvert.DeserializeObject<CountlyApiResponseModel>(reader.ReadToEnd());
                    countlyResponse.IsSuccess = res != null && res.Result == "Success";
                }
            }
            catch (Exception ex)
            {
                addToRequestQueue = true;
                countlyResponse.ErrorMessage = ex.Message;
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(true, url, null, DateTime.UtcNow);
                if (!CountlyRequestModel.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();
            }

#if UNITY_EDITOR
            //Log to Unity Console
            if (Countly.EnableConsoleErrorLogging)
            {
                Debug.Log(countlyResponse.IsSuccess);
            }
#endif
            return countlyResponse;
        }

        /// <summary>
        /// Makes an Asynchronous GET request to the Countly server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static async Task<CountlyResponse> GetAsync(string url, bool addToRequestQueue = false)
        {
            var countlyResponse = new CountlyResponse();
            try
            {
                if (addToRequestQueue)
                {
                    throw new Exception("Request added to queue.");
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var res = JsonConvert.DeserializeObject<CountlyApiResponseModel>(await reader.ReadToEndAsync());
                    countlyResponse.IsSuccess = res != null && res.Result == "Success";
#if UNITY_EDITOR                    
                    //Log to Unity Console
                    if (Countly.EnableConsoleErrorLogging)
                    {
                        Debug.Log(countlyResponse.IsSuccess);
                    }
#endif

                    return countlyResponse;
                }
            }
            catch (Exception ex)
            {
                addToRequestQueue = true;
                countlyResponse.ErrorMessage = ex.Message;
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(true, url, null, DateTime.UtcNow);
                if (!CountlyRequestModel.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();
            }

            return countlyResponse;
        }

        /// <summary>
        /// Makes a POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static CountlyResponse Post(string uri, string data, bool addToRequestQueue = false)
        {
            var countlyResponse = new CountlyResponse();
            try
            {
                if (addToRequestQueue)
                {
                    throw new Exception("Request added to queue.");
                }

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/json";
                request.Method = "POST";

                using (Stream requestBody = request.GetRequestStream())
                {
                    requestBody.Write(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var res = JsonConvert.DeserializeObject<CountlyApiResponseModel>(reader.ReadToEnd());
                    countlyResponse.IsSuccess = res != null && res.Result == "Success";

#if UNITY_EDITOR
                    //Log to Unity Console
                    if (Countly.EnableConsoleErrorLogging)
                    {
                        Debug.Log(countlyResponse.IsSuccess);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                addToRequestQueue = true;
                countlyResponse.ErrorMessage = ex.Message;
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(false, uri, data, DateTime.UtcNow);
                if (!CountlyRequestModel.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();
            }
            return countlyResponse;
        }

        /// <summary>
        /// Makes an Asynchronous POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static async Task<CountlyResponse> PostAsync(string uri, string data, bool addToRequestQueue = false)
        {
            var countlyResponse = new CountlyResponse();
            try
            {
                if (addToRequestQueue)
                {
                    throw new Exception("Request added to queue.");
                }

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/json";
                request.Method = "POST";


                using (Stream requestBody = request.GetRequestStream())
                {
                    await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var res = JsonConvert.DeserializeObject<CountlyApiResponseModel>(await reader.ReadToEndAsync());
                    countlyResponse.IsSuccess = res != null && res.Result == "Success";
#if UNITY_EDITOR
                    //Log to Unity Console
                    if (Countly.EnableConsoleErrorLogging)
                    {
                        Debug.Log(countlyResponse.IsSuccess);
                    }
#endif
                    return countlyResponse;
                }
            }
            catch (Exception ex)
            {
                addToRequestQueue = true;
                countlyResponse.ErrorMessage = ex.Message;
            }

            if (addToRequestQueue)
            {
                var requestModel = new CountlyRequestModel(false, uri, data, DateTime.UtcNow);
                if (!CountlyRequestModel.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();
            }

            return countlyResponse;
        }

        /// <summary>
        /// Validates the picture format. The Countly server supports a specific set of formats only.
        /// </summary>
        /// <param name="pictureUrl"></param>
        /// <returns></returns>
        internal static bool IsPictureValid(string pictureUrl)
        {
            if (!string.IsNullOrEmpty(pictureUrl) && pictureUrl.Contains("?"))
                pictureUrl = pictureUrl.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[0];

            return string.IsNullOrEmpty(pictureUrl)
                || pictureUrl.EndsWith(".png")
                || pictureUrl.EndsWith(".jpg")
                || pictureUrl.EndsWith(".jpeg")
                || pictureUrl.EndsWith(".gif");
        }

        internal static string GetStringFromBytes(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        internal static void InvokeMethod(Type type, string methodName, object[] payLoadData)
        {
            Testing tt = new Testing();
            MethodInfo info = type.GetMethod(methodName);
            info.Invoke(tt, new object[] { payLoadData });
        }

        internal static bool IsNullEmptyOrWhitespace(string input)
        {
            return string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input);
        }

        #endregion
    }
}
