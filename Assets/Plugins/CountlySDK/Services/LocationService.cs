using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class LocationService : AbstractBaseService
    {
        internal string City { get; private set; }
        internal string Location { get; private set; }
        internal string IPAddress { get; private set; }
        internal string CountryCode { get; private set; }
        internal bool IsLocationDisabled { get; private set; }

        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly CountlyConfiguration _countlyConfiguration;


        internal LocationService(CountlyConfiguration countlyConfiguration, RequestCountlyHelper requestCountlyHelper, ConsentCountlyService consentService) : base(consentService)
        {
            _countlyConfiguration = countlyConfiguration;
            _requestCountlyHelper = requestCountlyHelper;
            IsLocationDisabled = countlyConfiguration.IsLocationDisabled;

            if (IsLocationDisabled || !_consentService.CheckConsent(Features.Location)) {
                City = null;
                Location = null;
                IPAddress = null;
                CountryCode = null;
            } else {
                City = countlyConfiguration.City;
                Location = countlyConfiguration.Location;
                IPAddress = countlyConfiguration.IPAddress;
                CountryCode = countlyConfiguration.CountryCode;
            }
        }

        internal async Task SendRequestWithEmptyLocation()
        {
            Dictionary<string, object> requestParams =
               new Dictionary<string, object> {
                   { "location", string.Empty }
               };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        internal async Task SendIndependantLocationRequest()
        {

            if (!_consentService.CheckConsent(Features.Location)) {
                return;
            }

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>();

            /*
             * Empty country code, city and IP address can not be sent.
             */

            if (!string.IsNullOrEmpty(IPAddress)) {
                requestParams.Add("ip_address", IPAddress);
            }

            if (!string.IsNullOrEmpty(CountryCode)) {
                requestParams.Add("country_code", CountryCode);
            }

            if (!string.IsNullOrEmpty(City)) {
                requestParams.Add("city", City);
            }

            if (!string.IsNullOrEmpty(Location)) {
                requestParams.Add("location", Location);
            }

            if (requestParams.Count > 0) {
                await _requestCountlyHelper.GetResponseAsync(requestParams);
            }
        }



        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public async void DisableLocation()
        {
            if (!_consentService.CheckConsent(Features.Location)) {
                return;
            }

            IsLocationDisabled = true;
            City = null;
            Location = null;
            IPAddress = null;
            CountryCode = null;

            /*
             *If the location feature gets disabled or location consent is removed,
             *the SDK sends a request with an empty "location". 
             */

            await SendRequestWithEmptyLocation();
        }

        /// <summary>
        /// Set Country code (ISO Country code), City, Location and IP address to be used for future requests.
        /// </summary>
        /// <param name="countryCode">ISO Country code for the user's country</param>
        /// <param name="city">Name of the user's city</param>
        /// <param name="gpsCoordinates">comma separate lat and lng values. For example, "56.42345,123.45325"</param>
        /// <param name="ipAddress">ipAddress like "192.168.88.33"</param>
        /// <returns></returns>
        public async void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress)
        {
            if (!_consentService.CheckConsent(Features.Location)) {
                return;
            }

            /*If city is not paired together with country,
             * a warning should be printed that they should be set together.
             */
            if (_countlyConfiguration.EnableConsoleLogging &&
                ((!string.IsNullOrEmpty(CountryCode) && string.IsNullOrEmpty(City))
                || (!string.IsNullOrEmpty(City) && string.IsNullOrEmpty(CountryCode)))) {
                Debug.LogWarning("[Countly LocationService] In \"SetLocation\" both country code and city should be set together");
            }

            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;

            /*
             * If location consent is given and location gets re-enabled (previously was disabled), 
             * we send that set location information in a separate request and save it in the internal location cache.
             */
            if (countryCode != null || city != null || gpsCoordinates != null || ipAddress != null) {
                IsLocationDisabled = false;
                await SendIndependantLocationRequest();
            }
        }

        /*
         * If location consent is removed,
         * the SDK sends a request with an empty "location" parameter.
         */
        private async void OnLocationConsentRemoved()
        {
            City = null;
            Location = null;
            IPAddress = null;
            CountryCode = null;

            await SendRequestWithEmptyLocation();
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {
            if (updatedConsents.ContainsKey(Features.Location) && !updatedConsents[Features.Location]) {
                OnLocationConsentRemoved();
            }
            
        }
        #endregion
    }
}