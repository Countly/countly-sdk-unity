using Assets.Plugin.Models;
using Assets.Plugin.Scripts.Development;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Security.Cryptography;

namespace Helpers
{
    class CountlyHelper
    {
        #region Fields

        internal static readonly string OperationSystem = SystemInfo.operatingSystem;

        #endregion

        #region Methods

        internal static string GenerateUniqueDeviceID() => Guid.NewGuid().ToString();

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
            var baseParams = new Dictionary<string, object>();

            baseParams.Add("app_key", Countly.AppKey);
            baseParams.Add("device_id", Countly.DeviceId);

            foreach (var item in TimeMetricModel.GetTimeMetricModel())
                baseParams.Add(item.Key, item.Value);

            if (!string.IsNullOrEmpty(Countly.CountryCode))
                baseParams.Add("country_code", Countly.CountryCode);
            if (!string.IsNullOrEmpty(Countly.City))
                baseParams.Add("city", Countly.City);
            if (!string.IsNullOrEmpty(Countly.Location))
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
                baseParams.Add(WWW.EscapeURL(item.Key), Convert.ToString(item.Value));

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
        /// <param name="usePost"></param>
        /// <returns></returns>
        internal static string GetResponse(Dictionary<string, object> queryParams, bool usePost = false)
        {
            var data = BuildPostRequest(queryParams);
            if (Countly.PostRequestEnabled || usePost || data.Length > 1800)
            {
                return Post(GetBaseUrl(), data);
            }
            else
            {
                return Get(BuildGetRequest(queryParams));
            }
        }

        /// <summary>
        /// Uses GetAsync/PostAsync method to make request to the Countly server and returns the response.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="usePost"></param>
        /// <returns></returns>
        internal static async Task<string> GetResponseAsync(Dictionary<string, object> queryParams, bool usePost = false)
        {
            var data = BuildPostRequest(queryParams);
            if (Countly.PostRequestEnabled || usePost || data.Length > 1800)
            {
                return await PostAsync(GetBaseUrl(), data);
            }
            else
            {
                return await GetAsync(BuildGetRequest(queryParams));
            }
        }

        /// <summary>
        /// Makes a GET request to the Counlty server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static string Get(string url)
        {
            string responseString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = reader.ReadToEnd();
                }
            }
            //catch (WebException webex)
            //{
            //    responseString = webex.Message;
            //}
            catch (Exception ex)
            {
                var requestModel = new CountlyRequestModel(true, url, null, DateTime.UtcNow);
                if (!Countly.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();

                responseString = ex.Message;
            }

            //Log to Unity Console
            if (Countly.EnableConsoleErrorLogging)
            {
                Debug.Log(responseString);
            }
            return responseString;
        }

        /// <summary>
        /// Makes an Asynchronous GET request to the Countly server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static async Task<string> GetAsync(string url)
        {
            string responseString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = await reader.ReadToEndAsync();
                }
            }
            //catch (WebException webex)
            //{
            //    responseString = webex.Message;
            //}
            catch (Exception ex)
            {
                responseString = ex.Message;
            }

            //Log to Unity Console
            if (Countly.EnableConsoleErrorLogging)
            {
                Debug.Log(responseString);
            }
            return responseString;
        }

        /// <summary>
        /// Makes a POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static string Post(string uri, string data, string contentType = "application/json")
        {
            string responseString = string.Empty;
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = "POST";

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (Stream requestBody = request.GetRequestStream())
                {
                    requestBody.Write(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = reader.ReadToEnd();
                }
            }
            //catch (WebException webex)
            //{
            //    responseString = webex.Message;
            //}
            catch (Exception ex)
            {
                var requestModel = new CountlyRequestModel(false, uri, data, DateTime.UtcNow);
                if(!Countly.TotalRequests.Contains(requestModel))
                    requestModel.AddRequestToQueue();

                responseString = ex.Message;
            }

            //Log to Unity Console
            if (Countly.EnableConsoleErrorLogging)
            {
                Debug.Log(responseString);
            }
            return responseString;
        }

        /// <summary>
        /// Makes an Asynchronous POST request to the Countly server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static async Task<string> PostAsync(string uri, string data, string contentType = "application/json")
        {
            string responseString = string.Empty;
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = "POST";

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (Stream requestBody = request.GetRequestStream())
                {
                    await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = await reader.ReadToEndAsync();
                }
            }
            //catch (WebException webex)
            //{
            //    responseString = webex.Message;
            //}
            catch (Exception ex)
            {
                responseString = ex.Message;
            }

            //Log to Unity Console
            if (Countly.EnableConsoleErrorLogging)
            {
                Debug.Log(responseString);
            }
            return responseString;
        }

        /// <summary>
        /// Validates the picture format. The Countly server supports a specific set of formats only.
        /// </summary>
        /// <param name="pictureUrl"></param>
        /// <returns></returns>
        internal static bool IsPictureValid(string pictureUrl)
        {
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

        #region Unused Code

        //public static void GetRequest(string uri)
        //{
        //    using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        //    {
        //        uwr.SendWebRequest();
        //        var progress = uwr.downloadProgress;
        //        while (progress < 1)
        //        {
        //            progress = uwr.downloadProgress;
        //        }
        //        if (uwr.isNetworkError)
        //        {
        //            Debug.Log("Error While Sending: " + uwr.error);
        //        }
        //        else
        //        {
        //            Debug.Log("Received: " + uwr.downloadHandler.text);
        //        }
        //    }
        //}

        //public static void PostRequest(string uri, string postData)
        //{
        //    //if (!Countly.ConsentGranted)
        //    //{
        //    //    //return;
        //    //    //What do we do here?
        //    //}
        //    using (UnityWebRequest uwr = new UnityWebRequest(uri, "POST"))
        //    {
        //        byte[] jsonToSend = new UTF8Encoding().GetBytes(postData);
        //        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        //        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //        uwr.SetRequestHeader("Content-Type", "application/json");

        //        //Send the request then wait here until it returns
        //        uwr.SendWebRequest();
        //        var progress = uwr.downloadProgress;
        //        while (progress < 1)
        //        {
        //            progress = uwr.downloadProgress;
        //        }

        //        if (uwr.isNetworkError)
        //        {
        //            Debug.Log("Error While Sending: " + uwr.error);
        //        }
        //        else
        //        {
        //            Debug.Log("Received: " + uwr.downloadHandler.text);
        //        }
        //    }
        //}

        #endregion

        #endregion
    }
}
