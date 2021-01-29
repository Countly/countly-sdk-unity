
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Services
{
    public abstract class AbstractBaseService
    {
        internal List<AbstractBaseService> Listeners { get; set; }
        protected readonly ConsentCountlyService _consentService;
        

        protected AbstractBaseService(ConsentCountlyService consentService)
        {
            _consentService = consentService;
        }

        internal virtual void OnInitializationCompleted() { }
        internal virtual void DeviceIdChanged(string deviceId, bool merged) { }
        internal virtual void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue) { }
    }

}
