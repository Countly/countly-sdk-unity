using Assets.Plugin.Models;
using Assets.Plugin.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Helpers
{
    public class CountlyHelper
    {
        #region Fields

        #endregion

        #region Methods

        public static string GenerateUniqueDeviceID() => Guid.NewGuid().ToString();

        /// <summary>
        /// Build request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        public static string BuildRequest(Dictionary<string, object> queryParams)
        {
            StringBuilder request = new StringBuilder();

            //"i" added to the countly server url
            request.AppendFormat(Countly.ServerUrl[Countly.ServerUrl.Length - 1] == '/' ? "{0}i?" : "{0}/i?", Countly.ServerUrl);
            request.AppendFormat("app_key={0}", Countly.AppKey);
            request.AppendFormat("&device_id={0}", Countly.DeviceId);

            foreach (var item in TimeMetricModel.GetTimeMetricModel())
            {
                request.AppendFormat("&{0}={1}", item.Key, item.Value);
            }
            foreach (var item in queryParams)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                    request.AppendFormat("&{0}={1}", item.Key, item.Value);
            }
            return request.ToString();
        }

        public static void GetResponse(string uri, string postData = null)
        {
            if (Countly.PostRequestEnabled)
            {
                Post(uri, postData);
            }
            else
            {
                Get(uri);
            }
        }

        public static async Task<string> GetResponseAsync(string uri, string postData = null)
        {
            if (Countly.PostRequestEnabled)
            {
                return await PostAsync(uri, postData);
            }
            else
            {
                return await GetAsync(uri);
            }
        }

        public static string Get(string uri)
        {
            string responseString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = reader.ReadToEnd();
                }
            }
            catch (WebException webex)
            {
                responseString = webex.Message;
            }
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

        public static async Task<string> GetAsync(string uri)
        {
            string responseString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                //Allow untrusted connection
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = await reader.ReadToEndAsync();
                }
            }
            catch (WebException webex)
            {
                responseString = webex.Message;
            }
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

        public static string Post(string uri, string data, string contentType = "application/json", string method = "POST")
        {
            string responseString = string.Empty;
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = method;

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
            catch (WebException webex)
            {
                responseString = webex.Message;
            }
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

        public static async Task<string> PostAsync(string uri, string data, string contentType = "application/json", string method = "POST")
        {
            string responseString = string.Empty;
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = method;

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
            catch (WebException webex)
            {
                responseString = webex.Message;
            }
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
