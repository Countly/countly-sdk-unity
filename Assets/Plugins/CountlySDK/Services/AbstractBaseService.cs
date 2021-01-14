
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Services
{
    public abstract class AbstractBaseService
    {
        protected static ConsentCountlyService Consent { get; private set; }
        protected static readonly List<AbstractBaseService> Listeners = new List<AbstractBaseService>();

        protected AbstractBaseService()
        {
            Listeners.Add(this);

            if (GetType() == typeof(ConsentCountlyService)) {
                Consent = (ConsentCountlyService)this;
            }
        }



        internal virtual void DeviceIdChanged(string deviceId, bool merged) { }
        internal virtual void ConsentChanged(Dictionary<Features, bool> updatedConsents) { }
    }

}
