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
            CountryCode = country_code;
        }

        public void SetCity(string city)
        {
            City = city;
        }

        public void SetLocation(double latitude, double longitude)
        {
            Location = "latitude: " + latitude + ", longitude: " + longitude;
        }

        public void SetIPAddress(string ip_address)
        {
            IPAddress = ip_address;
        }

        public void DisableLocation()
        {
            Debug.Log("[OptionalParametersCountlyServiceWrapper] DisableLocation");
        }
    }
}