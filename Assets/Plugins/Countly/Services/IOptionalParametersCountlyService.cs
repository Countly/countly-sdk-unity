using System;

namespace Plugins.Countly.Services
{
    public interface IOptionalParametersCountlyService
    {
        string CountryCode { get; }
        string City { get; }
        string Location { get; }
        string IPAddress { get; }

        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="country_code"></param>
        [Obsolete("SetCountryCode is deprecated, please use SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress) instead.")]
        void SetCountryCode(string country_code);

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        [Obsolete("SetCity is deprecated, please use SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress) instead.")]
        void SetCity(string city);

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        [Obsolete("SetLocation(double latitude, double longitude) is deprecated, please use SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress) instead.")]
        void SetLocation(double latitude, double longitude);

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="city"></param>
        /// <param name="gpsCoordinates"></param>
        /// <param name="ipAddress"></param>
        void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress);

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        [Obsolete("SetIPAddress is deprecated, please use SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress) instead.")]
        void SetIPAddress(string ip_address);

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        void DisableLocation();
    }
}