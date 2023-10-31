using System;
using Newtonsoft.Json;
using Plugins.CountlySDK.Helpers;
using UnityEngine;

namespace Plugins.CountlySDK.Models
{
    [Serializable]
    internal class CountlyMetricModel
    {
        private MetricHelper metricHelper;

        public CountlyMetricModel(MetricHelper metricHelper)
        {
            this.metricHelper = metricHelper;
        }

        [JsonProperty("_os")] public string OS => metricHelper.OS;

        [JsonProperty("_os_version")] public string OSVersion => metricHelper.OSVersion;

        [JsonProperty("_device")] public string Device => metricHelper.Device;

        [JsonProperty("_resolution")] public string Resolution => metricHelper.Resolution;

        [JsonProperty("_carrier")] public string Carrier => metricHelper.Carrier;

        [JsonProperty("_app_version")] public string AppVersion => metricHelper.AppVersion;

        [JsonProperty("_density")] public string Density => metricHelper.Density;

        [JsonProperty("_store")] public string Store => metricHelper.Store;

        [JsonProperty("_browser")] public string Browser => metricHelper.Browser;

        [JsonProperty("_browser_version")] public string BrowserVersion => metricHelper.BrowserVersion;

        [JsonProperty("_locale")] public string Locale => metricHelper.Locale;

    }
}
