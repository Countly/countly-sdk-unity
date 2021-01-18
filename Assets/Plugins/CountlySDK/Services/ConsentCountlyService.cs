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
        private Dictionary<Consents, bool> _countlyConsents;
        private Dictionary<string, Consents[]> _countlyConsentGroups;

        internal ConsentCountlyService(CountlyConfiguration config, ConsentCountlyService consentService) : base(consentService)
        {
            _config = config;
            _countlyConsents = new Dictionary<Consents, bool>();

            RequiresConsent = _config.RequiresConsent;
            _countlyConsentGroups = new Dictionary<string, Consents[]>(_config.ConsentGroups);

            foreach (KeyValuePair<string, Consents[]> entry in _countlyConsentGroups) {
                if (_config.EnabledConsentGroups.Contains(entry.Key)) {
                    SetConsentInternal(entry.Value, true);
                }
            }

            SetConsentInternal(_config.GivenConsent, true);
        }

        #region Public Methods

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="consent">a feature for which consent should be checked</param>
        /// <returns>bool</returns>
        public bool CheckConsent(Consents consent)
        {
            return !RequiresConsent || (_countlyConsents.ContainsKey(consent) && _countlyConsents[consent]);
        }

        internal bool AnyConsentGiven()
        {
            if (!RequiresConsent) {
                //no consent required - all consent given
                return true;
            }

            foreach (KeyValuePair<Consents, bool> entry in _countlyConsents) {
                if (entry.Value) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Give consent to list of features
        /// </summary>
        /// <param name="consents">List of features for which consents should be given</param>
        /// <returns></returns>
        public void GiveConsent(Consents[] consents)
        {
            SetConsentInternal(consents, true);

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

            Consents[] consents = Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            SetConsentInternal(consents, true);
        }

        /// <summary>
        /// Remove consent of features
        /// </summary>
        /// <param name="consents">List of features for which consents should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Consents[] consents)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (consents == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsent with null features list!");
                }

                return;
            }
            //Remove Duplicates entries
            consents = consents.Distinct().ToArray();

            SetConsentInternal(consents, false);

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
            SetConsentInternal(_countlyConsents.Keys.ToArray(), false);
        }

        /// <summary>
        /// Give consent to a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void GiveConsentToGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsentToGroup with null groupName!");
                }
                return;
            }

            foreach (string name in groupName) {
                if (_countlyConsentGroups.ContainsKey(name)) {
                    Consents[] consents = _countlyConsentGroups[name];
                    SetConsentInternal(consents, true);
                }
            }
        }

        /// <summary>
        /// Remove consent of a group of features
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveConsentOfGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling RemoveConsentOfGroup with null groupName!");
                }

                return;
            }

            foreach (string name in groupName) {
                if (_countlyConsentGroups.ContainsKey(name)) {
                    Consents[] consents = _countlyConsentGroups[name];
                    SetConsentInternal(consents, false);
                }
            }
        }
        #endregion

        #region Helper Methods

        private void SetConsentInternal(Consents[] consents, bool flag)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Enable Consents");
                }

                return;
            }

            if (consents == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Calling GiveConsent with null consents list!");
                }

                return;
            }

            List<Consents> updatedConsents = new List<Consents>();
            foreach (Consents consent in consents) {
                if (_countlyConsents.ContainsKey(consent) && _countlyConsents[consent] == flag) {
                    continue;
                }

                updatedConsents.Add(consent);
                _countlyConsents[consent] = flag;

                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] Setting consent for feature: [" + consent.ToString() + "] with value: [" + flag + "]");
                }
            }

            NotifyListeners(updatedConsents, flag);
        }

        private void NotifyListeners(List<Consents> updatedConsents, bool newConsentValue)
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