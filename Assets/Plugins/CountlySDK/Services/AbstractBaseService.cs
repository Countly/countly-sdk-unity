
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Services
{
    public abstract class AbstractBaseService
    {
        internal virtual void DeviceIdChanged(string deviceId, bool merged) { }
        internal virtual void ConsentChanged(Dictionary<Features, bool> updatedConsents) { }
    }

}
