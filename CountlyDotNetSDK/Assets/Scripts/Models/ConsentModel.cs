using Assets.Scripts.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Models
{
    class ConsentModel
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
                    new ConsentModel(FeaturesEnum.Sessions.ToString(), "sessions", true),
                    new ConsentModel(FeaturesEnum.Events.ToString(), "events", true),
                    new ConsentModel(FeaturesEnum.Location.ToString(), "location", true),
                    new ConsentModel(FeaturesEnum.Views.ToString(), "views", true),
                    new ConsentModel(FeaturesEnum.Scrolls.ToString(), "scrolls", true),
                    new ConsentModel(FeaturesEnum.Clicks.ToString(), "clicks", true),
                    new ConsentModel(FeaturesEnum.Forms.ToString(), "forms", true),
                    new ConsentModel(FeaturesEnum.Crashes.ToString(), "crashes", true),
                    new ConsentModel(FeaturesEnum.Attribution.ToString(), "attribution", true),
                    new ConsentModel(FeaturesEnum.Users.ToString(), "users", true),
                    new ConsentModel(FeaturesEnum.Push.ToString(), "push", true),
                    new ConsentModel(FeaturesEnum.StarRating.ToString(), "star-rating", true),
                    new ConsentModel(FeaturesEnum.AccessoryDevices.ToString(), "accessory-devices", true),
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