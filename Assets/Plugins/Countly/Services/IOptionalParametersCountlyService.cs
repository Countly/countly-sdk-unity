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
        void SetCountryCode(string country_code);

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        void SetCity(string city);

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        void SetLocation(double latitude, double longitude);

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        void SetIPAddress(string ip_address);

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        void DisableLocation();
    }
}