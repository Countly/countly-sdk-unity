using System.Collections.Generic;
using System.Linq;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Models
{
    internal class ConsentModel
    {
        #region Fields

        public readonly string ConsentFormattedName;
        public readonly string ConsentActualName;
        public string[] Features;
        public bool IsConsentGranted;

        public static HashSet<ConsentModel> CountlyFeatureConsents;
        public static HashSet<ConsentModel> CountlyFeatureGroupConsents;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of ConsentModel class and initializes it with the supplied values
        /// </summary>
        /// <param name="formattedName"></param>
        /// <param name="name"></param>
        /// <param name="isConsentGranted"></param>
        /// <param name="features"></param>
        public ConsentModel(string formattedName, string name, bool isConsentGranted, string[] features = null)
        {
            ConsentFormattedName = formattedName;
            ConsentActualName = name;
            IsConsentGranted = isConsentGranted;
            Features = features;
        }

        /// <summary>
        /// Initializes the features list
        /// </summary>
        static ConsentModel()
        {
            CountlyFeatureGroupConsents = new HashSet<ConsentModel>();
            CountlyFeatureConsents =
                new HashSet<ConsentModel>
                {
                    new ConsentModel(Enums.Features.Sessions.ToString(), "sessions", true),
                    new ConsentModel(Enums.Features.Events.ToString(), "events", true),
                    new ConsentModel(Enums.Features.Location.ToString(), "location", true),
                    new ConsentModel(Enums.Features.Views.ToString(), "views", true),
                    new ConsentModel(Enums.Features.Scrolls.ToString(), "scrolls", true),
                    new ConsentModel(Enums.Features.Clicks.ToString(), "clicks", true),
                    new ConsentModel(Enums.Features.Forms.ToString(), "forms", true),
                    new ConsentModel(Enums.Features.Crashes.ToString(), "crashes", true),
                    new ConsentModel(Enums.Features.Attribution.ToString(), "attribution", true),
                    new ConsentModel(Enums.Features.Users.ToString(), "users", true),
                    new ConsentModel(Enums.Features.Push.ToString(), "push", true),
                    new ConsentModel(Enums.Features.StarRating.ToString(), "star-rating", true),
                    new ConsentModel(Enums.Features.AccessoryDevices.ToString(), "accessory-devices", true),
                };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="consentToSearch"></param>
        /// <returns></returns>
        public static bool CheckConsent(string consentToSearch)
        {
            var consent = CountlyFeatureConsents.FirstOrDefault(item => item.ConsentFormattedName == consentToSearch);
            return consent != null && consent.IsConsentGranted;
        }

        /// <summary>
        /// Updates consent for a particular feature
        /// </summary>
        /// <param name="consentToUpdate"></param>
        /// <param name="isConsentGranted"></param>
        public static void UpdateConsent(string consentToUpdate, bool isConsentGranted)
        {
            var consent = CountlyFeatureConsents.FirstOrDefault(item => item.ConsentFormattedName == consentToUpdate);
            if (consent != null)
            {
                consent.IsConsentGranted = isConsentGranted;
            }
        }

        #endregion
    }
}