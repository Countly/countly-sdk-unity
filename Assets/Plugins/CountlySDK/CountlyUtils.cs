using System;
using System.Collections.Generic;
using System.Text;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK
{
    public class CountlyUtils
    {
        private static readonly StringBuilder Builder = new StringBuilder();

        private readonly Countly _countly;

        internal string ServerInputUrl { get; private set; }

        internal string ServerOutputUrl { get; private set; }

        public CountlyUtils(Countly countly)
        {
            _countly = countly;

            ServerInputUrl = _countly.Configuration.ServerUrl + "/i?";
            ServerOutputUrl = _countly.Configuration.ServerUrl + "/o/sdk?";
        }

        public string GetUniqueDeviceId()
        {
            return UnityEngine.SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        ///     Gets the least set of paramas required to be sent along with each request.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetBaseParams()
        {
            Dictionary<string, object> baseParams = new Dictionary<string, object>
            {
                {"app_key", _countly.Configuration.AppKey},
                {"device_id", _countly.Device.DeviceId},
                {"sdk_name", Constants.SdkName},
                {"sdk_version", Constants.SdkVersion}
            };

            foreach (KeyValuePair<string, object> item in TimeMetricModel.GetTimeMetricModel()) {
                baseParams.Add(item.Key, item.Value);
            }

            return baseParams;
        }

        /// <summary>
        ///     Gets the least set of app key and device id required to be sent along with remote config request,
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetAppKeyAndDeviceIdParams()
        {
            return new Dictionary<string, object>
            {
                {"app_key", _countly.Configuration.AppKey},
                {"device_id", _countly.Device.DeviceId}
            };
        }

        public bool IsNullEmptyOrWhitespace(string input)
        {
            return string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        ///     Validates the picture format. The Countly server supports a specific set of formats only.
        /// </summary>
        /// <param name="pictureUrl"></param>
        /// <returns></returns>
        public bool IsPictureValid(string pictureUrl)
        {
            if (!string.IsNullOrEmpty(pictureUrl) && pictureUrl.Contains("?")) {
                pictureUrl = pictureUrl.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[0];
            }

            return string.IsNullOrEmpty(pictureUrl)
                   || pictureUrl.EndsWith(".png")
                   || pictureUrl.EndsWith(".jpg")
                   || pictureUrl.EndsWith(".jpeg")
                   || pictureUrl.EndsWith(".gif");
        }

        public string GetStringFromBytes(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++) {
                Builder.Append(bytes[i].ToString("x2"));
            }

            return Builder.ToString();
        }
    }
}