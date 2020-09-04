namespace Plugins.Countly.Services.Impls.Actual
{
    public class OptionalParametersCountlyService : IOptionalParametersCountlyService
    {
        public string CountryCode { get; private set; }
        public string City { get; private set; }
        public string Location { get; private set; }
        public string IPAddress { get; private set; }
        
        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="country_code"></param>
        public void SetCountryCode(string country_code)
        {
            CountryCode = country_code;
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        public void SetCity(string city)
        {
            City = city;
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetLocation(double latitude, double longitude)
        {
            Location = latitude + "," + longitude;
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        public void SetIPAddress(string ip_address)
        {
            IPAddress = ip_address;
        }

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public void DisableLocation()
        {
            Location = string.Empty;
        }

        public void SetLocation(string countryCode, string city, string gpsCoordinates, string ipAddress)
        {
            City = city;
            IPAddress = ipAddress;
            CountryCode = countryCode;
            Location = gpsCoordinates;
            
        }
    }
}