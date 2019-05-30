using System;

namespace Plugins.Countly.Models
{
    [Serializable]
    public class CountlyAuthModel
    {
        public string ServerUrl;
        public string AppKey;
        public string DeviceId;
    }
}