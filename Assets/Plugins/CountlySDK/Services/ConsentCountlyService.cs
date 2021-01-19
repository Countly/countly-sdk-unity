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
        /// Checks consent
        /// </summary>
        /// <param name="consent">a consent that should be checked</param>
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
        /// Give consent to a list
        /// </summary>
        /// <param name="consents">List of consents</param>
        /// <returns></returns>
        public void GiveConsent(Consents[] consents)
        {
            SetConsentInternal(consents, true);

        }

        /// <summary>
        /// Gives all consent
        /// </summary>
        /// <returns></returns>
        public void GiveConsentAll()
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService] GiveConsentAll: Please set consent to be required before calling this!");
                }

                return;
            }

            Consents[] consents = Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            SetConsentInternal(consents, true);
        }

        /// <summary>
        /// Remove consent
        /// </summary>
        /// <param name="consents">List of consents that should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Consents[] consents)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService] RemoveConsent: Please set consent to be required before calling this!");
                }

                return;
            }

            if (consents == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService]: Calling RemoveConsent with null consents list!");
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
                    Debug.Log("[Countly ConsentCountlyService] RemoveAllConsent: Please set consent to be required before calling this!");
                }

                return;
            }
            SetConsentInternal(_countlyConsents.Keys.ToArray(), false);
        }

        /// <summary>
        /// Give consent to a group list
        /// </summary>
        /// <param name="groupName">list of the consents group name</param>
        /// <returns></returns>
        public void GiveConsentToGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService] GiveConsentToGroup: Please set consent to be required before calling this!");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService]: Calling GiveConsentToGroup with null groupName!");
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
        /// Remove consent of a group
        /// </summary>
        /// <param name="groupName">name of the consent group</param>
        /// <returns></returns>
        public void RemoveConsentOfGroup(string[] groupName)
        {
            if (!RequiresConsent) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService] RemoveConsentOfGroup: Please set consent to be required before calling this!");
                }

                return;
            }

            if (groupName == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService]: Calling RemoveConsentOfGroup with null groupName!");
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

        private void SetConsentInternal(Consents[] consents, bool value)
        {
            if (consents == null) {
                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService]: Calling SetConsentInternal with null consents list!");
                }

                return;
            }

            List<Consents> updatedConsents = new List<Consents>(consents.Length);
            foreach (Consents consent in consents) {
                if (_countlyConsents.ContainsKey(consent) && _countlyConsents[consent] == value) {
                    continue;
                }

                updatedConsents.Add(consent);
                _countlyConsents[consent] = value;

                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly ConsentCountlyService] Setting consent for: [" + consent.ToString() + "] with value: [" + value + "]");
                }
            }

            NotifyListeners(updatedConsents, value);
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