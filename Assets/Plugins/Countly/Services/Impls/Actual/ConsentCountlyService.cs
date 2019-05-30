using Plugins.Countly.Models;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class ConsentCountlyService : IConsentCountlyService
    {
        

        #region Consents

        #region Unused Code

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool CheckConsent(string feature)
        {
            return ConsentModel.CheckConsent(feature);
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