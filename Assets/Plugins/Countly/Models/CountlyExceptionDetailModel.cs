using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Models
{
    [Serializable]
    internal class CountlyExceptionDetailModel
    {
        //device metrics
        [JsonProperty("_os")]
        public string OS { get; set; }
        [JsonProperty("_os_version")]
        public string OSVersion { get; set; }
        [JsonProperty("_manufacture")]
        public string Manufacture { get; set; }

        [JsonProperty("_device")]
        public string Device { get; set; }
        [JsonProperty("_resolution")]
        public string Resolution { get; set; }
        [JsonProperty("_app_version")]
        public string AppVersion { get; set; }
        [JsonProperty("_cpu")]
        public string Cpu { get; set; }
        [JsonProperty("_opengl")]
        public string Opengl { get; set; }

        //state of device
        [JsonProperty("_ram_current")]
        public string RamCurrent { get; set; }
        [JsonProperty("_ram_total")]
        public string RamTotal { get; set; }
        [JsonProperty("_disk_current")]
        public string DiskCurrent { get; set; }
        [JsonProperty("_disk_total")]
        public string DiskTotal { get; set; }
        [JsonProperty("_bat")]
        public string Battery { get; set; }
        [JsonProperty("_orientation")]
        public string Orientation { get; set; }

        //bools
        [JsonProperty("_root")]
        public string Root { get; set; }
        [JsonProperty("_online")]
        public string Online { get; set; }
        [JsonProperty("_muted")]
        public string Muted { get; set; }
        [JsonProperty("_background")]
        public string Background { get; set; }
        [JsonProperty("_name")]
        public string Name { get; set; }
        [JsonProperty("_error")]
        public string Error { get; set; }
        [JsonProperty("_nonfatal")]
        public bool Nonfatal { get; set; }
        [JsonProperty("_logs")]
        public string Logs { get; set; }
        [JsonProperty("_run")]
        public string Run { get; set; }
        [JsonProperty("_custom")]
        public Dictionary<string, object> Custom { get; set; }

        static CountlyExceptionDetailModel() { }
        private CountlyExceptionDetailModel() { }

        public static readonly CountlyExceptionDetailModel ExceptionDetailModel
            = new CountlyExceptionDetailModel
            {
                OS = Constants.UnityPlatform,
                OSVersion = SystemInfo.operatingSystem,
                Device = SystemInfo.deviceName,
                Resolution = Screen.currentResolution.ToString(),
                AppVersion = Application.version,
                Cpu = SystemInfo.processorType,
                Opengl = SystemInfo.graphicsDeviceVersion,
                RamTotal = SystemInfo.systemMemorySize.ToString(),
                Battery = SystemInfo.batteryLevel.ToString(),
                Orientation = Screen.orientation.ToString(),
                Online = (Application.internetReachability > 0).ToString(),

                //Not found
                RamCurrent = null,
                DiskCurrent = null,
                DiskTotal = null,
                Muted = null,
                Background = null,
                Root = null,
                Logs = null,
                Run = null,
            };
    }
}
