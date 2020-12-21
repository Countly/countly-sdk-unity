using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class ConsentCountlyService
    {

        internal ConsentCountlyService()
        { }

        #region Consents

        #region Unused Code

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool CheckConsent(Features feature)
        {
            return true;// ConsentModel.CheckConsent(feature.toString());
        }

        /// <summary>
        /// Updates a feature to give/deny consent
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="permission"></param>
        public void UpdateConsent(string feature, bool permission)
        {
            ConsentModel.UpdateConsent(feature, permission);
        }

        #endregion

        #endregion


    }
}