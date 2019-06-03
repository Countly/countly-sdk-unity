using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using Plugins.Countly.Services.Impls.Actual;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class UserDetailsCountlyServiceWrapper : IUserDetailsCountlyService
    {
        public Task<CountlyResponse> UserDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] UserDetailsAsync, userDetails: \n" + userDetails);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> UserCustomDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] UserCustomDetailsAsync, userDetails: \n" + userDetails);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> SetUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] SetUserDetailsAsync, userDetailsModel: \n" + userDetailsModel);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> SetCustomUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] SetCustomUserDetailsAsync, userDetailsModel: \n" + userDetailsModel);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> SaveAsync()
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] SaveAsync");
            return Task.FromResult(new CountlyResponse());
        }

        public void Set(string key, string value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Set, key: " + key + ", value: " + value);
        }

        public void SetOnce(string key, string value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] SetOnce, key: " + key + ", value: " + value);
        }

        public void Increment(string key)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Increment, key: " + key);
        }

        public void IncrementBy(string key, double value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] IncrementBy, key: " + key + ", value: " + value);
        }

        public void Multiply(string key, double value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Multiply, key: " + key + ", value: " + value);
        }

        public void Max(string key, double value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Max, key: " + key + ", value: " + value);
        }

        public void Min(string key, double value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Min, key: " + key + ", value: " + value);
        }

        public void Push(string key, string[] value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Push, key: " + key + ", value: " + value);
        }

        public void PushUnique(string key, string[] value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] PushUnique, key: " + key + ", value: " + value);
        }

        public void Pull(string key, string[] value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] Pull, key: " + key + ", value: " + value);
        }

        public void AddToCustomData(string key, object value)
        {
            Debug.Log("[UserDetailsCountlyServiceWrapper] AddToCustomData, key: " + key + ", value: " + value);
        }
    }
}