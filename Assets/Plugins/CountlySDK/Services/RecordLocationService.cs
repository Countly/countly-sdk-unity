using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class RecordLocationService
    {
        public string City { get; private set; }
        public string Location { get; private set; }
        public string IPAddress { get; private set; }
        public string CountryCode { get; private set; }

        private readonly CountlyConfiguration _countlyConfiguration;

        internal RecordLocationService(CountlyConfiguration countlyConfiguration)
        {
            City = countlyConfiguration.City;
            Location = countlyConfiguration.Location;
            IPAddress = countlyConfiguration.IPAddress;
            CountryCode = countlyConfiguration.CountryCode;

            _countlyConfiguration = countlyConfiguration;
        }

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public void DisableLocation()
        {
            Location = string.Empty;
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
                Debug.LogWarning("[Countly] CountryCode and City should be set together");
            }

            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;
            
        }
    }
}