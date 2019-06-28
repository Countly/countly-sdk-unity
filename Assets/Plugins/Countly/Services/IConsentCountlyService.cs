namespace Plugins.Countly.Services
{
    public interface IConsentCountlyService
    {
        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        bool CheckConsent(string feature);

        /// <summary>
        /// Updates a feature to give/deny consent
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="permission"></param>
        void UpdateConsent(string feature, bool permission);
    }

}