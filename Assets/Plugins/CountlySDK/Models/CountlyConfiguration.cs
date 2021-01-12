using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.CountlySDK.Enums;
using UnityEngine;

namespace Plugins.CountlySDK.Models
{
    [Serializable]
    public class CountlyConfiguration
    {
        public string ServerUrl = null;
        public string AppKey = null;
        public string DeviceId = null;

        public string Salt = null;
        public bool EnableFirstAppLaunchSegment = false;
        public bool EnablePost = false;
        public bool EnableTestMode = false;
        public bool EnableConsoleLogging = false;
        public bool IgnoreSessionCooldown = false;
        public TestMode NotificationMode = TestMode.None;
        public readonly bool EnableManualSessionHandling = false;
        public int SessionDuration = 60;
        public int EventQueueThreshold = 100;
        public int StoredRequestLimit = 1000;
        public int TotalBreadcrumbsAllowed = 100;
        public bool EnableAutomaticCrashReporting = true;

        internal string City;
        internal string Location;
        internal string IPAddress;
        internal string CountryCode;
        internal bool IsLocationDisabled;

        public bool RequiresConsent = false;
        internal Features[] Features { get; private set;}
        internal Dictionary<string, Features[]> FeatureGroups { get; private set; }
        

        /// <summary>
        ///     Parent must be undestroyable
        /// </summary>
        public GameObject Parent = null;

        public CountlyConfiguration()
        {
            FeatureGroups = new Dictionary<string, Features[]>();
        }

        internal CountlyConfiguration(CountlyAuthModel authModel, CountlyConfigModel config)
        {
            FeatureGroups = new Dictionary<string, Features[]>();

            ServerUrl = authModel.ServerUrl;
            AppKey = authModel.AppKey;
            DeviceId = authModel.DeviceId;

            Salt = config.Salt;
            EnablePost = config.EnablePost;
            EnableManualSessionHandling = config.EnableManualSessionHandling;
            EnableFirstAppLaunchSegment = config.EnableFirstAppLaunchSegment;
            EnableTestMode = config.EnableTestMode;
            EnableConsoleLogging = config.EnableConsoleLogging;
            IgnoreSessionCooldown = config.IgnoreSessionCooldown;
            NotificationMode = config.NotificationMode;
            SessionDuration = config.SessionDuration;
            EventQueueThreshold = config.EventQueueThreshold;
            StoredRequestLimit = config.StoredRequestLimit;
            TotalBreadcrumbsAllowed = config.TotalBreadcrumbsAllowed;
            EnableAutomaticCrashReporting = config.EnableAutomaticCrashReporting;

        }

        public override string ToString()
        {
            return $"{nameof(Salt)}: {Salt}, {nameof(EnablePost)}: {EnablePost}, {nameof(EnableConsoleLogging)}: {EnableConsoleLogging}, {nameof(IgnoreSessionCooldown)}: {IgnoreSessionCooldown}, {nameof(NotificationMode)}: {NotificationMode}, {nameof(EnableManualSessionHandling)}: {EnableManualSessionHandling}, {nameof(SessionDuration)}: {SessionDuration}, {nameof(EventQueueThreshold)}: {EventQueueThreshold}, {nameof(StoredRequestLimit)}: {StoredRequestLimit}, {nameof(TotalBreadcrumbsAllowed)}: {TotalBreadcrumbsAllowed}, {nameof(EnableAutomaticCrashReporting)}: {EnableAutomaticCrashReporting}";
        }

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public void DisableLocation()
        {
            IsLocationDisabled = true;
        }

        /// <summary>
        ///     Set location parameters that will be used during init.
        /// </summary>
        /// <returns></returns>
        public void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress)
        {
            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;
        }

        public void EnableFeatursConsents(Features[] features)
        {
            if (!RequiresConsent) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly] CountlyConfiguration: Enable Consents");
                }

                return;
            }

            if (features == null) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly] CountlyConfiguration: Calling GiveConsent with null features list!");
                }

                return;
            }
        }

        public void CreateFeatureGroup(string groupName, Features[] features)
        {
            if (!RequiresConsent) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly CountlyConfiguration] : Consents are not enable");
                }

                return;
            }

            if (groupName == null) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly] CountlyConfiguration: Calling CreateFeatureGroup with null groupName!");
                }
                return;
            }

            if (features == null) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly] CountlyConfiguration: Calling CreateFeatureGroup with null features list!");
                }
                return;
            }


            if (FeatureGroups.ContainsKey(groupName)) {
                if (EnableConsoleLogging) {
                    Debug.Log("[Countly] ConsentCountlyService: Feature Group '" + groupName + "' already exist!");
                }
                return;
            }

            FeatureGroups.Add(groupName, features);

        }
    }


}