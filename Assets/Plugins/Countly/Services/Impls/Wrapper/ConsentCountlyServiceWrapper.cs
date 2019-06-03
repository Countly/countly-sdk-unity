using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class ConsentCountlyServiceWrapper : IConsentCountlyService
    {
        public bool CheckConsent(string feature)
        {
            Debug.Log("[ConsentCountlyServiceWrapper] Check consent, feature: " + feature);
            return true;
        }

        public void UpdateConsent(string feature, bool permission)
        {
            Debug.Log("[ConsentCountlyServiceWrapper] Update consent, feature: " + feature + ", permission: " + permission);
        }
    }
}