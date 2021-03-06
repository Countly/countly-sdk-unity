using System;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class OptionalParametersCountlyService : AbstractBaseService
    {
        public string CountryCode { get { return _recordLocation.CountryCode; } }
        public string City { get { return _recordLocation.City; } }
        public string Location { get { return _recordLocation.Location; } }
        public string IPAddress { get { return _recordLocation.IPAddress; } }

        private readonly LocationService _recordLocation;

        internal OptionalParametersCountlyService(LocationService recordLocation, CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[OptionalParametersCountlyService] Initializing.");

            _recordLocation = recordLocation;
        }

        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="countryCode">ISO Country code for the user's country</param>
        [Obsolete("SetCountryCode is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetCountryCode(string country_code)
        {
            SetLocation(country_code, City, Location, IPAddress);
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city">Name of the user's city</param>
        [Obsolete("SetCity is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetCity(string city)
        {
            SetLocation(CountryCode, city, Location, IPAddress);
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude">latitude value for example, 56.42345</param>
        /// <param name="longitude">longitude value for example, 123.45325</param>
        [Obsolete("SetLocation is deprecated, please use SetLocation function of LocationService instead.")]
        public void SetLocation(double latitude, double longitude)
        {
            string location = latitude + "," + longitude;
            SetLocation(CountryCode, City, location, IPAddress);
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ipAddress">ipAddress like "192.168.88.33"</param>
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
        /// <param name="countryCode">ISO Country code for the user's country</param>
        /// <param name="city">Name of the user's city</param>
        /// <param name="gpsCoordinates">comma separate lat and lng values. For example, "56.42345,123.45325"</param>
        /// <param name="ipAddress">ipAddress like "192.168.88.33"</param>
        /// <returns></returns>
        public void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress)
        {
            _recordLocation.SetLocation(countryCode, city, gpsCoordinates, ipAddress);

            Log.Warning("[OptionalParametersCountlyService] OptionalParameters is deprecated, please use Location instead");
        }
    }
}