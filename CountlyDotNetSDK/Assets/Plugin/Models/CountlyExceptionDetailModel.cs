using System;
using UnityEngine;

namespace CountlyModels
{
    [Serializable]
    public class CountlyExceptionDetailModel
    {
        //device metrics
        public string _os;
        public string _os_version;
        public string _manufacture;

        public string _device;
        public string _resolution;
        public string _app_version;
        public string _cpu;
        public string _opengl;

        //state of device
        public string _ram_current;
        public string _ram_total;
        public string _disk_current;
        public string _disk_total;
        public string _bat;
        public string _orientation;

        //bools
        public string _root;
        public string _online;
        public string _muted;
        public string _background;

        public string _name;
        public string _error;
        public string _nonfatal;
        public string _logs;
        public string _run;

        static CountlyExceptionDetailModel() { }
        private CountlyExceptionDetailModel() { }

        public static readonly CountlyExceptionDetailModel ExceptionDetailModel
            = new CountlyExceptionDetailModel
            {
                _os = SystemInfo.operatingSystem,
                _os_version = SystemInfo.operatingSystem,
                _device = SystemInfo.deviceName,
                _resolution = Screen.currentResolution.ToString(),
                //_app_version = ??
                _cpu = SystemInfo.processorType,
                _opengl = SystemInfo.graphicsDeviceVersion,
                _ram_current = SystemInfo.systemMemorySize.ToString(),
                _ram_total = SystemInfo.systemMemorySize.ToString(),
                //_disk_current = ??
                //_disk_total = ??
                _bat = SystemInfo.batteryLevel.ToString(),
                _orientation = Screen.orientation.ToString(),
                _online = (Application.internetReachability > 0).ToString(),
                //_muted = ??
                //_background = ??
                _nonfatal = true.ToString(),
            };
    }
}
