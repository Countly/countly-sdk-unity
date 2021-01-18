using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEditor;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class ConsentCountlyService : AbstractBaseService
    {
        internal bool RequiresConsent { get; private set; }

        private readonly CountlyConfiguration _config;
        private Dictionary<Features, bool> _countlyFeatureConsents;
        private Dictionary<string, Features[]> _countlyFeatureGroups;

        internal ConsentCountlyService(CountlyConfiguration config, ConsentCountlyService consentService) : base(consentService)
        {
            _config = config;
            _countlyFeatureConsents = new Dictionary<Features, bool>();

            RequiresConsent = _config.RequiresConsent;
            _countlyFeatureGroups = new Dictionary<string, Features[]>(_config.FeatureGroups);

            foreach (KeyValuePair<string, Features[]> entry in _countlyFeatureGroups) {
                if (_config.EnabledFeatureGroups.Contains(entry.Key)) {
                    SetConsentInternal(entry.Value, true);
                }
            }

            SetConsentInternal(_config.GivenConsent, true);
        }

        #region Public Methods

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="feature">a feature for which consent should be checked</param>
        /// <returns>bool</returns>
        public bool CheckConsent(Features feature)
        {
            return !RequiresConsent || (_countlyFeatureConsents.ContainsKey(feature) && _countlyFeatureConsents[feature]);
        }

        internal bool AnyConsentGiven()
        {
            if (!RequiresConsent) {
                //no consent required - all consent given
                return true;
            }

            foreach (KeyValuePair<Features, bool> entry in _countlyFeatureConsents) {
                if (entry.Value) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Give consent to list of features
        /// </summary>
        /// <param name="features">List of features for which consents should be given</param>
        /// <returns></returns>
        public void GiveConsent(Features[] features)
        {
            SetConsentInternal(features, true);

        }

        /// <summary>
        /// Gives consent for all features
        /// </summary>
        /// <returns></returns>
        public void GiveConsentAll()
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents!");
                }

                return;
            }

            Features[] features = Enum.GetValues(typeof(Features)).Cast<Features>().ToArray();
            SetConsentInternal(features, true);
        }

        /// <summary>
        /// Remove consent of features
        /// </summary>
        /// <param name="features">List of features for which consents should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Features[] features)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (features == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsent with null features list!");
                }

                return;
            }
            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            SetConsentInternal(features, false);

        }

        /// <summary>
        /// Remove All consents
        /// </summary>
        /// <returns></returns>
        public void RemoveAllConsent()
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }
            SetConsentInternal(_countlyFeatureConsents.Keys.ToArray(), false);
        }

        /// <summary>
        /// Give consent to a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void GiveConsentToFeatureGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsentToFeatureGroup with null groupName!");
                }
                return;
            }

            foreach (string name in groupName) {
                if (_countlyFeatureGroups.ContainsKey(name)) {
                    Features[] features = _countlyFeatureGroups[name];
                    SetConsentInternal(features, true);
                }
            }
        }

        /// <summary>
        /// Remove consent of a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveConsentOfFeatureGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsentOfFeatureGroup with null groupName!");
                }

                return;
            }

            foreach (string name in groupName) {
                if (_countlyFeatureGroups.ContainsKey(name)) {
                    Features[] features = _countlyFeatureGroups[name];
                    SetConsentInternal(features, false);
                }
            }
        }
        #endregion

        #region Helper Methods

        private void SetConsentInternal(Features[] features, bool flag)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (features == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsent with null features list!");
                }

                return;
            }

            List<Features> updatedConsents = new List<Features>();
            foreach (Features feature in features) {
                if (_countlyFeatureConsents.ContainsKey(feature) && _countlyFeatureConsents[feature] == flag) {
                    return;
                }

                updatedConsents.Add(feature);
                _countlyFeatureConsents[feature] = flag;

                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] Setting consent for feature: [" + feature.ToString() + "] with value: [" + flag + "]");
                }
            }

            NotifyListeners(updatedConsents, flag);
        }

        private void NotifyListeners(List<Features> updatedConsents, bool newConsentValue)
        {
            if (Listeners == null || updatedConsents.Count < 1) {
                return;
            }

            foreach (AbstractBaseService listener in Listeners) {
                listener.ConsentChanged(updatedConsents, newConsentValue);
            }
        }
        #endregion

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged) { }
        #endregion
    }
}