using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class ConsentCountlyService
    {
        internal bool RequiresConsent { get; private set; }
        private readonly CountlyConfiguration _config;
        private readonly LocationService _locationService;
        private Dictionary<Features, bool> _countlyFeatureConsents;
        private Dictionary<string, Features[]> _countlyFeatureGroups;


        internal ConsentCountlyService(CountlyConfiguration config, LocationService locationService)
        {
            _countlyFeatureConsents = new Dictionary<Features, bool>();
            _countlyFeatureGroups = new Dictionary<string, Features[]>();

            _config = config;
            _locationService = locationService;
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
            GiveConsentInternal(feature);
        }

        /// <summary>
        /// Give consent to list of features
        /// </summary>
        /// <param name="features">List of features for which consents should be given</param>
        /// <returns></returns>
        public void GiveConsent(Features[] features)
        {
            if (features == null)
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsent with null features list!");
                }

                return;
            }
            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            foreach (Features feature in features)
            {
                GiveConsentInternal(feature);
            }
        }

        /// <summary>
        /// Gives consent for all features
        /// </summary>
        /// <returns></returns>
        public void GiveConsentAll()
        {
            //Features[] features = new Features { };
            //foreach (Features feature in features)
            //{
            //    GiveConsentInternal(feature);
            //}
        }

        /// <summary>
        /// Remove consent of a feature
        /// </summary>
        /// <param name="feature">a feature for which consent should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Features feature)
        {
            RemoveConsentInternal(feature);
        }

        /// <summary>
        /// Remove consent of features
        /// </summary>
        /// <param name="features">List of features for which consents should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Features[] features)
        {
            if (features == null)
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsent with null features list!");
                }

                return;
            }
            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            foreach (Features feature in features)
            {
                RemoveConsentInternal(feature);
            }
        }

        /// <summary>
        /// Remove All consents
        /// </summary>
        /// <returns></returns>
        public void RemoveAllConsent()
        {
            foreach (Features feature in _countlyFeatureConsents.Keys)
            {
                RemoveConsentInternal(feature);
            }

        }

        /// <summary>
        /// Give consent to a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void GiveConsentToFeatureGroup(string groupName)
        {
            Features[] features = _countlyFeatureGroups[groupName];

            if (features == null)
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsentToFeatureGroup with null groupName!");
                }
                return;
            }

            foreach (Features feature in features)
            {
                GiveConsentInternal(feature);
            }
        }

        /// <summary>
        /// Remove consent of a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveConsentOfFeatureGroup(string groupName)
        {
            Features[] features = _countlyFeatureGroups[groupName];

            if (features == null)
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsentOfFeatureGroup with null groupName!");
                }
                return;
            }

            foreach (Features feature in features)
            {
                RemoveConsentInternal(feature);
            }
        }

        /// <summary>
        /// Remove group and consent of features of this group
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveFeatureGroup(string groupName)
        {
            if (!_countlyFeatureGroups.ContainsKey(groupName))
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Feature Group '" + groupName + "' does not exist!");
                }
                return;
            }

            RemoveConsentOfFeatureGroup(groupName);

            _countlyFeatureGroups.Remove(groupName);
        }

        /// <summary>
        /// Group multiple features into a feature group
        /// </summary>
        /// <param name="groupName">Name of the feature group</param>
        /// <param name="features">array of feature to be added to the consent group</param>
        /// <returns></returns>
        public void CreateFeatureGroup(string groupName, Features[] features)
        {
            if (features == null)
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] Calling ConsentCountlyService: CreateFeatureGroup with null groupName!");
                }
                return;
            }


            if (_countlyFeatureGroups.ContainsKey(groupName))
            {
                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("[Countly] ConsentCountlyService: Feature Group '" + groupName + "' already exist!");
                }
                return;
            }

            //Remove Duplicates entries
            features = features.Distinct().ToArray();

            _countlyFeatureGroups.Add(groupName, features);
        }

        #endregion

        #region Helper Methods

        private void GiveConsentInternal(Features feature)
        {
            if (_countlyFeatureConsents.ContainsKey(feature))
            {
                _countlyFeatureConsents[feature] = true;

            }
            else
            {
                _countlyFeatureConsents.Add(feature, true);
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly] Setting consent for feature: [" + feature.ToString() + "] with value: [true]");
            }
        }

        private void RemoveConsentInternal(Features feature)
        {
            if (_countlyFeatureConsents.ContainsKey(feature))
            {
                CheckIfLocationConsentIsRemoved(feature);
                _countlyFeatureConsents[feature] = false;
            }
            else
            {
                _countlyFeatureConsents.Add(feature, false);
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly] Setting consent for feature: [" + feature.ToString() + "] with value: [false]");
            }
        }

        /*
         * If location consent is removed,
         * the SDK sends a request with an empty "location" parameter.
         */
        private async void CheckIfLocationConsentIsRemoved(Features feature)
        {
            if (feature == Features.Location && _countlyFeatureConsents[feature])
            {
                 await _locationService.SendRequestWithEmptyLocation();
            }
        }

        #endregion
    }
}