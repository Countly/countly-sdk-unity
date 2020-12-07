using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class LocationService
    {
        internal bool IsLocationDisabled { get; private set; }
        internal string City { get; private set; }
        internal string Location { get; private set; }
        internal string IPAddress { get; private set; }
        internal string CountryCode { get; private set; }

        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly CountlyConfiguration _countlyConfiguration;
        private readonly ConsentCountlyService _consentCountlyService;

        internal LocationService(CountlyConfiguration countlyConfiguration, RequestCountlyHelper requestCountlyHelper)
        {
            City = countlyConfiguration.City;
            Location = countlyConfiguration.Location;
            IPAddress = countlyConfiguration.IPAddress;
            CountryCode = countlyConfiguration.CountryCode;
            IsLocationDisabled = countlyConfiguration.IsLocationDisabled;

            _countlyConfiguration = countlyConfiguration;
            _requestCountlyHelper = requestCountlyHelper;
        }

        private async void OnLocatinoReenabledAsync()
        {
            /*
             * If location consent is given and location gets reenabled (previously was disabled), 
             * we send that set location information in a separate request and save it in the internal location cache.
             */

            if (!_consentCountlyService.CheckConsent(FeaturesEnum.Location))
            {
                await SendIndependatLocationRequest();
            }
        }

        internal async Task SendRequestWithEmptyLocation()
        {
            var requestParams =
               new Dictionary<string, object>();


            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            requestParams.Add("location", string.Empty);

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        internal async Task SendIndependatLocationRequest()
        {
            
            var requestParams =
                new Dictionary<string, object>();

           
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            /*
             * Empty country code, city and IP address can not be sent.
             */

            if (!string.IsNullOrEmpty(IPAddress))
            {
                requestParams.Add("ip_address", IPAddress);
            }

            if (!string.IsNullOrEmpty(CountryCode))
            {
                requestParams.Add("country_code", CountryCode);
            }

            if (!string.IsNullOrEmpty(City))
            {
                requestParams.Add("city", City);
            }

            if (!string.IsNullOrEmpty(Location))
            {
                requestParams.Add("location", Location);
            }

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }



        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public async void DisableLocation()
        {
            IsLocationDisabled = true;

            /*
             *If the location feature gets disabled or location consent is removed,
             *the SDK sends a request with an empty "location". 
             *TODO ~ On Consent removed
             */

            await SendRequestWithEmptyLocation();
        }

        /// <summary>
        /// Set Country code (ISO Country code), City, Location and IP address to be used for future requests.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="city"></param>
        /// <param name="gpsCoordinates"></param>
        /// <param name="ipAddress"></param>
        public void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress)
        {

            //If city is not paired together with country, a warning should be printed that they should be set together.
            if (_countlyConfiguration.EnableConsoleLogging &&
                (!string.IsNullOrEmpty(CountryCode) && string.IsNullOrEmpty(City)
                || !string.IsNullOrEmpty(City) && string.IsNullOrEmpty(CountryCode)))
            {
                Debug.LogWarning("[Countly LocationService] In \"SetLocation\" both country code and city should be set together");
            }

            if (countryCode != null || city != null || gpsCoordinates != null || ipAddress != null)
            {
                if (IsLocationDisabled)
                {
                    OnLocatinoReenabledAsync();
                }

                IsLocationDisabled = false;
            }
            
            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;
            
        }
    }
}