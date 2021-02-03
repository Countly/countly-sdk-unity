
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public abstract class AbstractBaseService
    {
        internal List<AbstractBaseService> Listeners { get; set; }

        protected CountlyLogHelper Log { get; private set; }
        protected readonly CountlyConfiguration _configuration;
        protected readonly ConsentCountlyService _consentService;

        protected AbstractBaseService(CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService)
        {
            Log = logHelper;
            _configuration = configuration;
            _consentService = consentService;
        }

        internal virtual void OnInitializationCompleted() { }
        internal virtual void DeviceIdChanged(string deviceId, bool merged) { }
        internal virtual void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue) { }
    }

}
