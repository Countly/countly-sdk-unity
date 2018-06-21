using System;
using UnityEngine;

namespace CountlyModels
{
    [Serializable]
    public class CountlyMetricModel
    {
        public string _os;
        public string _os_version;
        public string _device;
        public string _resolution;
        public string _carrier;
        public string _app_version;
        public string _density;
        public string _store;
        public string _browser;
        public string _browser_version;

        static CountlyMetricModel() { }
        private CountlyMetricModel() { }

        public static readonly CountlyMetricModel Metrics =
            new CountlyMetricModel
            {
                _os = SystemInfo.operatingSystem,
                _os_version = SystemInfo.operatingSystem,
                _device = SystemInfo.deviceModel,
                _resolution = Screen.currentResolution.ToString(),
                //_carrier =
                //_app_version =
                _density = Screen.dpi.ToString(),
                //_store =
                //_browser =
                //_browser_version =
            };
    }
}
