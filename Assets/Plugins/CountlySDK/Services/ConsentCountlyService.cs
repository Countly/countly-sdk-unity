using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class ConsentCountlyService : AbstractBaseService
    {
        
        internal bool RequiresConsent { get; private set; }

        private bool _sendConsentOnChange;
        internal readonly RequestCountlyHelper _requestCountlyHelper;
        private Dictionary<string, Consents[]> _countlyConsentGroups;
        internal readonly Dictionary<Consents, bool> _countlyConsents;

        internal ConsentCountlyService(CountlyConfiguration config, CountlyLogHelper logHelper, ConsentCountlyService consentService, RequestCountlyHelper requestCountlyHelper) : base(config, logHelper, consentService)
        {
            Log.Debug("[ConsentCountlyService] Initializing.");
            _requestCountlyHelper = requestCountlyHelper;
            _countlyConsents = new Dictionary<Consents, bool>();
            RequiresConsent = _configuration.RequiresConsent;
            _countlyConsentGroups = new Dictionary<string, Consents[]>(_configuration.ConsentGroups);

            if (_configuration.EnabledConsentGroups != null) {
                foreach (KeyValuePair<string, Consents[]> entry in _countlyConsentGroups) {
                    if (_configuration.EnabledConsentGroups.Contains(entry.Key)) {
                        SetConsentInternal(entry.Value, true);
                    }
                }
            }

            SetConsentInternal(_configuration.GivenConsent, true);
        }

        #region Public Methods

        /// <summary>
        ///  Check if consent for the specific feature has been given
        /// </summary>
        /// <param name="consent">The consent that should be checked</param>
        /// <returns>Returns "true" if the consent for the checked feature has been provided</returns>
        public bool CheckConsent(Consents consent)
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] CheckConsent : consent = " + consent.ToString());
                return CheckConsentInternal(consent);
            }
        }

        /// <summary>
        ///  An internal function to check if consent for the specific feature has been given
        /// </summary>
        /// <param name="consent">The consent that should be checked</param>
        /// <returns>Returns "true" if the consent for the checked feature has been provided</returns>
        internal bool CheckConsentInternal(Consents consent)
        {
            bool result = !RequiresConsent || (_countlyConsents.ContainsKey(consent) && _countlyConsents[consent]);
            Log.Verbose("[ConsentCountlyService] CheckConsent : consent = " + consent.ToString() + ", result = " + result);
            return result;
        }

        /// <summary>
        ///  Check if consent for any feature has been given
        /// </summary>
        /// <returns>Returns "true" if consent is given for any of the possible features</returns>
        internal bool AnyConsentGiven()
        {
            bool result = !RequiresConsent;

            if (result) {
                Log.Verbose("[ConsentCountlyService] AnyConsentGiven = " + result);
                return result;
            }

            foreach (KeyValuePair<Consents, bool> entry in _countlyConsents) {
                if (entry.Value) {
                    result = true;
                    break;
                }
            }

            Log.Verbose("[ConsentCountlyService] AnyConsentGiven = " + result);

            return result;
        }

        /// <summary>
        /// Give consent to the provided features
        /// </summary>
        /// <param name="consents">array of consents for which consent should be given</param>
        /// <returns></returns>
        public void GiveConsent(Consents[] consents)
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] GiveConsent : consents = " + (consents != null));

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] GiveConsent: Please set consent to be required before calling this!");
                    return;
                }

                SetConsentInternal(consents, true);
            }
        }

        /// <summary>
        /// Give consent to all features
        /// </summary>
        /// <returns></returns>
        public void GiveConsentAll()
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] GiveConsentAll");

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] GiveConsentAll: Please set consent to be required before calling this!");
                    return;
                }

                Consents[] consents = Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
                SetConsentInternal(consents, true);
            }
        }

        /// <summary>
        /// Remove consent from the provided features
        /// </summary>
        /// <param name="consents">array of consents for which consent should be removed</param>
        /// <returns></returns>
        public void RemoveConsent(Consents[] consents)
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] RemoveConsent : consents = " + (consents != null));

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] RemoveConsent: Please set consent to be required before calling this!");
                    return;
                }

                if (consents == null) {
                    Log.Debug("[ConsentCountlyService] Calling RemoveConsent with null consents list!");
                    return;
                }
                //Remove Duplicates entries
                consents = consents.Distinct().ToArray();

                SetConsentInternal(consents, false);
            }

        }

        /// <summary>
        /// Remove consent from all features
        /// </summary>
        /// <returns></returns>
        public void RemoveAllConsent()
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] RemoveAllConsent");

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] RemoveAllConsent: Please set consent to be required before calling this!");
                    return;
                }

                SetConsentInternal(_countlyConsents.Keys.ToArray(), false);
            }
        }

        /// <summary>
        /// Give consent to the provided feature groups
        /// </summary>
        /// <param name="groupName">array of consent group for which consent should be given</param>
        /// <returns></returns>
        public void GiveConsentToGroup(string[] groupName)
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] GiveConsentToGroup : groupName = " + (groupName != null));

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] GiveConsentToGroup: Please set consent to be required before calling this!");
                    return;
                }

                if (groupName == null) {
                    Log.Debug("[ConsentCountlyService] Calling GiveConsentToGroup with null groupName!");
                    return;
                }

                foreach (string name in groupName) {
                    if (_countlyConsentGroups.ContainsKey(name)) {
                        Consents[] consents = _countlyConsentGroups[name];
                        SetConsentInternal(consents, true);
                    }
                }
            }
        }

        /// <summary>
        /// Remove consent from the provided feature groups
        /// </summary>
        /// <param name="groupName">An array of consent group names for which consent should be removed</param>
        /// <returns></returns>
        public void RemoveConsentOfGroup(string[] groupName)
        {
            lock (LockObj) {
                Log.Info("[ConsentCountlyService] RemoveConsentOfGroup : groupName = " + (groupName != null));

                if (!RequiresConsent) {
                    Log.Debug("[ConsentCountlyService] RemoveConsentOfGroup: Please set consent to be required before calling this!");
                    return;
                }

                if (groupName == null) {
                    Log.Debug("[ConsentCountlyService] Calling RemoveConsentOfGroup with null groupName!");
                    return;
                }

                foreach (string name in groupName) {
                    if (_countlyConsentGroups.ContainsKey(name)) {
                        Consents[] consents = _countlyConsentGroups[name];
                        SetConsentInternal(consents, false);
                    }
                }
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Internal method that send consent changes to server.
        /// </summary>
        /// <param name="consents">List of consent</param>
        /// <param name="value">value to be set</param>
        internal async Task SendConsentChanges(List<Consents> consents, bool value)
        {
            _sendConsentOnChange = true;

            if (!RequiresConsent || consents.Count == 0 ) {
                return;
            }

            JObject jObj = new JObject();
            foreach (Consents consent in consents) {
                jObj.Add(GetConsentKey(consent), value);
            }
            
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "consent", jObj
                    }
                };

            _requestCountlyHelper.AddToRequestQueue(requestParams);
            await _requestCountlyHelper.ProcessQueue();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Private method that returns consent key.
        /// </summary>
        /// <param name="consent">a consent</param>
        ///<returns>string</returns>
        private string GetConsentKey(Consents consent)
        {
            string key = "";
            switch (consent) {
                case Consents.Clicks:
                    key = "clicks";
                    break;
                case Consents.Crashes:
                    key = "crashes";
                    break;
                case Consents.Events:
                    key = "events";
                    break;
                case Consents.Location:
                    key = "location";
                    break;
                case Consents.Push:
                    key = "push";
                    break;
                case Consents.RemoteConfig:
                    key = "remote-config";
                    break;
                case Consents.Sessions:
                    key = "sessions";
                    break;
                case Consents.StarRating:
                    key = "star-rating";
                    break;
                case Consents.Users:
                    key = "users";
                    break;
                case Consents.Views:
                    key = "views";
                    break;
            }

            return key;
        }
            /// <summary>
            /// Private method that update selected consents.
            /// </summary>
            /// <param name="consents">List of consent</param>
            /// <param name="value">value to be set</param>
            private async void SetConsentInternal(Consents[] consents, bool value)
        {
            if (consents == null) {
                Log.Debug("[ConsentCountlyService] Calling SetConsentInternal with null consents list!");
                return;
            }

            List<Consents> updatedConsents = new List<Consents>(consents.Length);
            foreach (Consents consent in consents) {
                if (_countlyConsents.ContainsKey(consent) && _countlyConsents[consent] == value) {
                    continue;
                }

                if (!_countlyConsents.ContainsKey(consent) && !value) {
                    continue;
                }

                updatedConsents.Add(consent);
                _countlyConsents[consent] = value;

                Log.Debug("[ConsentCountlyService] Setting consent for: [" + consent.ToString() + "] with value: [" + value + "]");
            }

            if (_sendConsentOnChange) {
                await SendConsentChanges(updatedConsents, value);
            }
            NotifyListeners(updatedConsents, value);
        }

        /// <summary>
        /// On consents changed, call <code>ConsentChanged</code> on all listeners.
        /// </summary>
        /// <param name="updatedConsents">List of modified consent</param>
        /// <param name="newConsentValue">Modified Consents's new value</param>
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
