namespace Plugins.CountlySDK.Services
{
    public class OptionalParametersCountlyService
    {
        public string CountryCode { get; private set; }
        public string City { get; private set; }
        public string Location { get; private set; }
        public string IPAddress { get; private set; }

        internal OptionalParametersCountlyService()
        { }

        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="country_code"></param>
        public void SetCountryCode(string country_code)
        {
            SetLocation(country_code, City, Location, IPAddress);
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        public void SetCity(string city)
        {
            SetLocation(CountryCode, city, Location, IPAddress);
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetLocation(double latitude, double longitude)
        {
            string location = latitude + "," + longitude;
            SetLocation(CountryCode, City, location, IPAddress);
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        public void SetIPAddress(string ip_address)
        {
            SetLocation(CountryCode, City, Location, ip_address);
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
            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;
            
        }
    }
}