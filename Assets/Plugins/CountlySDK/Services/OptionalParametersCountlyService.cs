using System;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class OptionalParametersCountlyService
    {
        public string CountryCode { get { return _recordLocation.CountryCode; } }
        public string City { get { return _recordLocation.City; } }
        public string Location { get { return _recordLocation.Location; } }
        public string IPAddress { get { return _recordLocation.IPAddress; } }

        private readonly LocationService _recordLocation;
        private readonly CountlyConfiguration _countlyConfiguration;

        internal OptionalParametersCountlyService(LocationService recordLocation, CountlyConfiguration countlyConfiguration, ConsentCountlyService consentService) : base(consentService)
        {
            _recordLocation = recordLocation;
            _countlyConfiguration = countlyConfiguration;
        }

        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="country_code"></param>
        [Obsolete("SetCountryCode is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetCountryCode(string country_code)
        {
            SetLocation(country_code, City, Location, IPAddress);
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        /// 
        [Obsolete("SetCity is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetCity(string city)
        {
            SetLocation(CountryCode, city, Location, IPAddress);
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        [Obsolete("SetLocation is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetLocation(double latitude, double longitude)
        {
            string location = latitude + "," + longitude;
            SetLocation(CountryCode, City, location, IPAddress);
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        [Obsolete("SetIPAddress is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetIPAddress(string ip_address)
        {
            SetLocation(CountryCode, City, Location, ip_address);
        }

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        [Obsolete("DisableLocation is deprecated, please use DisableLocation function of LocationService instead.")]
        public void DisableLocation()
        {
            _recordLocation.DisableLocation();
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
            _recordLocation.SetLocation(countryCode, city, gpsCoordinates, ipAddress);

            if (_countlyConfiguration.EnableConsoleLogging) {
                Debug.LogWarning("[Countly] OptionalParameters is deprecated, please use Location instead");
            }

        }
    }
}