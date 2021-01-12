using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class ConsentCountlyService : IBaseService
    {
        internal bool RequiresConsent { get; private set; }

        private List<IBaseService> _listeners;
        private readonly CountlyConfiguration _config;
        private readonly LocationService _locationService;
        private readonly Dictionary<Features, bool> _modifiedConsents;
        private readonly Dictionary<Features, bool> _countlyFeatureConsents;
        private readonly Dictionary<string, Features[]> _countlyFeatureGroups;

        internal ConsentCountlyService(CountlyConfiguration config, LocationService locationService)
        {
            _modifiedConsents = new Dictionary<Features, bool>();
            _countlyFeatureConsents = new Dictionary<Features, bool>();
            _countlyFeatureGroups = new Dictionary<string, Features[]>();

            _config = config;
            _locationService = locationService;
        }

        internal void AddListeners(List<IBaseService> listeners)
        {
            _listeners = listeners;
            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly DeviceIdCountlyService] AddListeners");
            }
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

        /// <summary>
        /// Give consent to feature
        /// </summary>
        /// <param name="feature">a feature for which consent should be given</param>
        /// <returns></returns>
        public void GiveConsent(Features feature)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents!");
                }

                return;
            }

            GiveConsentInternal(feature);

            NotifyListeners();
        }

        /// <summary>
        /// Give consent to list of features
        /// </summary>
        /// <param name="features">List of features for which consents should be given</param>
        /// <returns></returns>
        public void GiveConsent(Features[] features)
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
            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            foreach (Features feature in features) {
                GiveConsentInternal(feature);
            }

            NotifyListeners();
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
            foreach (Features feature in features) {
                GiveConsentInternal(feature);
            }

            NotifyListeners();
        }

        /// <summary>
        /// Remove consent of a feature
        /// </summary>
        /// <param name="feature">a feature for which consent should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Features feature)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents!");
                }

                return;
            }

            RemoveConsentInternal(feature);

            NotifyListeners();
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

            foreach (Features feature in features) {
                RemoveConsentInternal(feature);
            }

            NotifyListeners();
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

            foreach (Features feature in _countlyFeatureConsents.Keys) {
                RemoveConsentInternal(feature);
            }

            NotifyListeners();

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
                Features[] features = _countlyFeatureGroups[name];
                foreach (Features feature in features) {
                    GiveConsentInternal(feature);
                }
            }

            NotifyListeners();
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
                Features[] features = _countlyFeatureGroups[name];
                foreach (Features feature in features) {
                    RemoveConsentInternal(feature);
                }
            }

            NotifyListeners();
        }

        /// <summary>
        /// Remove group and consent of features of this group
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveFeatureGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveFeatureGroup with null groupName!");
                }
                return;
            }

            //Remove Consent of features groups
            RemoveConsentOfFeatureGroup(groupName);

            //Remove features groups
            foreach (string name in groupName) {
                if (!_countlyFeatureGroups.ContainsKey(name)) {
                    if (_config.EnableConsoleLogging) {
                        Debug.Log("[Countly] ConsentCountlyService: Feature Group '" + name + "' does not exist!");
                    }
                    return;
                }
                _countlyFeatureGroups.Remove(name);
            }

            NotifyListeners();
        }

        /// <summary>
        /// Group multiple features into a feature group
        /// </summary>
        /// <param name="groupName">Name of the feature group</param>
        /// <param name="features">array of feature to be added to the consent group</param>
        /// <returns></returns>
        public void CreateFeatureGroup(string groupName, Features[] features)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] Calling ConsentCountlyService: CreateFeatureGroup with null groupName!");
                }
                return;
            }

            if (features == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] Calling ConsentCountlyService: CreateFeatureGroup with null features list!");
                }
                return;
            }


            if (_countlyFeatureGroups.ContainsKey(groupName)) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Feature Group '" + groupName + "' already exist!");
                }
                return;
            }

            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            _countlyFeatureGroups.Add(groupName, features);

            NotifyListeners();
        }

        #endregion

        #region Helper Methods

        private void GiveConsentInternal(Features feature)
        {
            if (_countlyFeatureConsents.ContainsKey(feature)) {
                _countlyFeatureConsents[feature] = true;

            } else {
                _countlyFeatureConsents.Add(feature, true);
            }

            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly] Setting consent for feature: [" + feature.ToString() + "] with value: [true]");
            }
        }

        private void RemoveConsentInternal(Features feature)
        {
            if (_countlyFeatureConsents.ContainsKey(feature)) {
                _countlyFeatureConsents[feature] = false;
            } else {
                _countlyFeatureConsents.Add(feature, false);
            }

            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly] Setting consent for feature: [" + feature.ToString() + "] with value: [false]");
            }
        }

        private void NotifyListeners()
        {
            Features[] features = Enum.GetValues(typeof(Features)).Cast<Features>().ToArray();
            foreach (Features feature in features) {

                if (_modifiedConsents.ContainsKey(feature)) {
                    if (_countlyFeatureConsents.ContainsKey(feature)) {
                        if (_modifiedConsents[feature] == _countlyFeatureConsents[feature]) {
                            _modifiedConsents.Remove(feature);
                        } else {
                            _modifiedConsents[feature] = _countlyFeatureConsents[feature];
                        }
                    } else {
                        if (_modifiedConsents[feature]) {
                            _modifiedConsents[feature] = false;
                        } else {
                            _modifiedConsents.Remove(feature);
                        }
                    }

                } else {
                    if (_countlyFeatureConsents.ContainsKey(feature) && _countlyFeatureConsents[feature]) {
                        _modifiedConsents.Add(feature, true);
                    }
                }
            }

            if (_listeners == null) {
                return;
            }

            foreach (IBaseService listener in _listeners) {
                listener.ConsentChanged(_modifiedConsents);
            }
        }
        #endregion

        #region override Methods
        public void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        public void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {

        }
        #endregion
    }
}