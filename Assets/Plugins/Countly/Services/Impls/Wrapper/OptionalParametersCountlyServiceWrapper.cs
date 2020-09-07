using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class OptionalParametersCountlyServiceWrapper : IOptionalParametersCountlyService
    {
        public string CountryCode { get; private set; }
        public string City { get; private set; }
        public string Location { get; private set; }
        public string IPAddress { get; private set; }
        
        public void SetCountryCode(string country_code)
        {
            SetLocation(country_code, City, Location, IPAddress);
        }

        public void SetCity(string city)
        {
            SetLocation(CountryCode, city, Location, IPAddress);
        }

        public void SetLocation(double latitude, double longitude)
        {
            string location = "latitude: " + latitude + ", longitude: " + longitude;

            SetLocation(CountryCode, City, location, IPAddress);
        }

        public void SetIPAddress(string ip_address)
        {
            SetLocation(CountryCode, City, Location, ip_address);
        }

        public void DisableLocation()
        {
            Debug.Log("[OptionalParametersCountlyServiceWrapper] DisableLocation");
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