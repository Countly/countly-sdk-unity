using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class UserDetailsCountlyService : AbstractBaseService
    {
        internal Dictionary<string, object> CustomDataProperties { get; private set; }

        private readonly CountlyUtils _countlyUtils;
        internal readonly RequestCountlyHelper _requestCountlyHelper;
        internal UserDetailsCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, CountlyUtils countlyUtils, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[UserDetailsCountlyService] Initializing.");

            _countlyUtils = countlyUtils;
            _requestCountlyHelper = requestCountlyHelper;
            CustomDataProperties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetailsModel">User's detail object</param>
        /// <returns></returns>
        internal async Task UserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {

            Log.Debug("[UserDetailsCountlyService] UserDetailsAsync : userDetails = " + (userDetailsModel != null));

            if (userDetailsModel == null) {
                Log.Warning("[UserDetailsCountlyService] UserDetailsAsync : The parameter 'userDetailsModel' can't be null.");
                return;
            }

            await SetUserDetailsAsync(userDetailsModel);
        }

        /// <summary>
        /// Modifies custom user data only. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetailsModel">User's custom detail object</param>
        /// <return></returns>
        internal async Task UserCustomDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            Log.Debug("[UserDetailsCountlyService] UserCustomDetailsAsync " + (userDetailsModel != null));

            if (userDetailsModel == null) {
                Log.Warning("[UserDetailsCountlyService] UserCustomDetailsAsync : The parameter 'userDetailsModel' can't be null.");
                return;
            }

            await SetCustomUserDetailsAsync(userDetailsModel);
        }

        /// <summary>
        /// Sets information about user.
        /// </summary>
        /// <param name="userDetailsModel">User Model with the specified params</param>
        /// <returns></returns>
        public async Task SetUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] SetUserDetailsAsync " + (userDetailsModel != null));

                if (!_consentService.CheckConsentInternal(Consents.Users)) {
                    return;
                }

                if (userDetailsModel == null) {
                    Log.Warning("[UserDetailsCountlyService] SetUserDetailsAsync : The parameter 'userDetailsModel' can't be null.");
                    return;
                }

                if (!_countlyUtils.IsPictureValid(userDetailsModel.PictureUrl)) {
                    throw new Exception("Accepted picture formats are .png, .gif and .jpeg");
                }

                Dictionary<string, object> requestParams =
                    new Dictionary<string, object>
                    {
                    { "user_details", JsonConvert.SerializeObject(userDetailsModel, Formatting.Indented,
                        new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore }) },
                    };

                _requestCountlyHelper.AddToRequestQueue(requestParams);
                _= _requestCountlyHelper.ProcessQueue();
            }
        }

        /// <summary>
        /// Sets information about user with custom properties.
        /// In custom properties you can provide any string key values to be stored with user.
        /// </summary>
        /// <param name="userDetailsModel">User Detail Model with the custom properties</param>
        /// <returns></returns>
        public async Task SetCustomUserDetailsAsync(CountlyUserDetailsModel userDetailsModel)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] SetCustomUserDetailsAsync " + (userDetailsModel != null));

                if (!_consentService.CheckConsentInternal(Consents.Users)) {
                    return;
                }

                if (userDetailsModel == null) {
                    Log.Warning("[UserDetailsCountlyService] SetCustomUserDetailsAsync : The parameter 'userDetailsModel' can't be null.");
                    return;
                }

                if (userDetailsModel.Custom == null || userDetailsModel.Custom.Count == 0) {
                    Log.Warning("[UserDetailsCountlyService] SetCustomUserDetailsAsync : The custom property 'userDetailsModel.Custom' can't be null or empty.");

                    return;
                }

                Dictionary<string, object> requestParams =
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
                _requestCountlyHelper.AddToRequestQueue(requestParams);
                _= _requestCountlyHelper.ProcessQueue();
            }
        }

        /// <summary>
        /// Send provided values to server.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            lock (LockObj) {
                if (!CustomDataProperties.Any()) {
                    return;
                }

                Log.Info("[UserDetailsCountlyService] SaveAsync");


                CountlyUserDetailsModel model = new CountlyUserDetailsModel(CustomDataProperties);

                CustomDataProperties = new Dictionary<string, object> { };
                _= SetCustomUserDetailsAsync(model);
            }
        }


        /// <summary>
        /// Sets custom provide key/value as custom property.
        /// </summary>
        /// <param name="key">string with key for the property</param>
        /// <param name="value">string with value for the property</param>
        public void Set(string key, string value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Set : key = " + key + ", value = " + value);

                AddToCustomData(key, value);
            }
        }

        /// <summary>
        /// Set value only if property does not exist yet.
        /// </summary>
        /// <param name="key">string with property name to set</param>
        /// <param name="value">string value to set</param>
        public void SetOnce(string key, string value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] SetOnce : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$setOnce", value } });
            }
        }

        /// <summary>
        /// Increment custom property value by 1.
        /// </summary>
        /// <param name="key">string with property name to increment</param>
        public void Increment(string key)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Increment : key = " + key);

                AddToCustomData(key, new Dictionary<string, object> { { "$inc", 1 } });
            }
        }

        /// <summary>
        /// Increment custom property value by provided value.
        /// </summary>
        /// <param name="key">string with property name to increment</param>
        /// <param name="value">double value by which to increment</param>
        public void IncrementBy(string key, double value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] IncrementBy : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$inc", value } });
            }
        }

        /// <summary>
        /// Multiply custom property value by provided value.
        /// </summary>
        /// <param name="key">string with property name to multiply</param>
        /// <param name="value">double value by which to multiply</param>
        public void Multiply(string key, double value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Multiply : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$mul", value } });
            }
        }

        /// <summary>
        /// Save maximal value between existing and provided.
        /// </summary>
        /// <param name="key">String with property name to check for max</param>
        /// <param name="value">double value to check for max</param>
        public void Max(string key, double value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Max : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$max", value } });
            }
        }

        /// <summary>
        /// Save minimal value between existing and provided.
        /// </summary>
        /// <param name="key">string with property name to check for min</param>
        /// <param name="value">double value to check for min</param>
        public void Min(string key, double value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Min : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$min", value } });
            }
        }

        /// <summary>
        /// Create array property, if property does not exist and add value to array
        /// You can only use it on array properties or properties that do not exist yet.
        /// </summary>
        /// <param name="key">string with property name for array property</param>
        /// <param name="value">array with values to add</param>
        public void Push(string key, string[] value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Push : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$push", value } });
            }
        }

        /// <summary>
        /// Create array property, if property does not exist and add value to array, only if value is not yet in the array
        /// You can only use it on array properties or properties that do not exist yet.
        /// </summary>
        /// <param name="key">string with property name for array property</param>
        /// <param name="value">array with values to add</param>
        public void PushUnique(string key, string[] value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] PushUnique : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$addToSet", value } });
            }
        }

        /// <summary>
        /// Create array property, if property does not exist and remove value from array.
        /// </summary>
        /// <param name="key">String with property name for array property</param>
        /// <param name="value">array with values to remove from array</param>
        public void Pull(string key, string[] value)
        {
            lock (LockObj) {
                Log.Info("[UserDetailsCountlyService] Pull : key = " + key + ", value = " + value);

                AddToCustomData(key, new Dictionary<string, object> { { "$pull", value } });
            }
        }


        /// <summary>
        /// Create a property
        /// </summary>
        /// <param name="key">property name</param>
        /// <param name="value">property value</param>
        private void AddToCustomData(string key, object value)
        {
            Log.Debug("[UserDetailsCountlyService] AddToCustomData: " + key + ", " + value);

            if (!_consentService.CheckConsentInternal(Consents.Users)) {
                return;
            }

            if (CustomDataProperties.ContainsKey(key)) {
                string item = CustomDataProperties.Select(x => x.Key).FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (item != null) {
                    CustomDataProperties.Remove(item);
                }
            }

            CustomDataProperties.Add(key, value);
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {

        }
        #endregion
    }
}
