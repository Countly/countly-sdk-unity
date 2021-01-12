
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Services
{
    internal interface IBaseService
    {
        void DeviceIdChanged(string deviceId, bool merged);
        void ConsentChanged(Dictionary<Features, bool> updatedConsents);
    }

}
