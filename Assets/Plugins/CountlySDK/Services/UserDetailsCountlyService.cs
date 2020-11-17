using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class UserDetailsCountlyService
    {
        private Dictionary<string, object> _customDataProperties = new Dictionary<string, object>();

        
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly CountlyUtils _countlyUtils;

        internal UserDetailsCountlyService(RequestCountlyHelper requestCountlyHelper, CountlyUtils countlyUtils)
        {
            _requestCountlyHelper = requestCountlyHelper;
            _countlyUtils = countlyUtils;
        }

        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        internal async Task UserDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
            {
                return;
            }

            await SetUserDetailsAsync(userDetails);
        }

        /// <summary>
        /// Modifies custom user data only. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returCountlyUserDetailsModel.Pushns></returns>
        internal async Task UserCustomDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
            {
                return;
            }

            await SetCustomUserDetailsAsync(userDetails);
        }
        
        /// <summary>
        /// Uploads all user details
        /// </summary>
        /// <returns></returns>
        public async Task SetUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            if (!_countlyUtils.IsPictureValid(userDetailsModel.PictureUrl))
                throw new Exception("Accepted picture formats are .png, .gif and .jpeg");

            var requestParams =
                new Dictionary<string, object>
                {
                    { "user_details", JsonConvert.SerializeObject(userDetailsModel, Formatting.Indented, 
                        new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore }) },
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Uploads only custom data. Doesn't update any other property except Custom Data.
        /// </summary>
        /// <returns></returns>
        public async Task SetCustomUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            var requestParams =
                new Dictionary<string, object>
                {
                    { "user_details",
                        JsonConvert.SerializeObject(
                            new Dictionary<string, object>
                            {
                                { "custom", userDetailsModel.Custom }
                            })
                    }
                };
            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }
        
        /// <summary>
        /// Saves all custom user data updates done since the last save request.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            if (!_customDataProperties.Any())
            {
                return;
            }

            var model = new CountlyUserDetailsModel(_customDataProperties);

            _customDataProperties = new Dictionary<string, object> { };
            await SetCustomUserDetailsAsync(model);
        }
        
        
        /// <summary>
        /// Sets value to key.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            AddToCustomData(key, value);
        }

        /// <summary>
        /// Sets value to key, only if property was not defined before for this user.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetOnce(string key, string value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$setOnce", value } });
        }

        /// <summary>
        /// To increment value, for the specified key, on the server by 1.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        public void Increment(string key)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$inc", 1 } });
        }

        /// <summary>
        /// To increment value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void IncrementBy(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$inc", value } });
        }

        /// <summary>
        /// To multiply value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Multiply(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$mul", value } });
        }

        /// <summary>
        /// To store maximal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Max(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$max", value } });
        }

        /// <summary>
        /// To store minimal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Min(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$min", value } });
        }

        /// <summary>
        /// Add one or many values to array property (can have multiple same values, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Push(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$push", value } });
        }

        /// <summary>
        /// Add one or many values to array property (will only store unique values in array, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void PushUnique(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$addToSet", value } });
        }

        /// <summary>
        /// Remove one or many values from array property (only removes value from array properties).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Pull(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$pull", value } });
        }


        public void AddToCustomData(string key, object value)
        {
            if (_customDataProperties.ContainsKey(key))
            {
                var item = _customDataProperties.Select(x => x.Key).FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    _customDataProperties.Remove(item);
                }
            }

            _customDataProperties.Add(key, value);
        }
        
        
    }
}