using Assets.Scripts.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class CountlyUserDetailsModel
    {
        [JsonProperty("name")]
        internal string Name { get; set; }
        [JsonProperty("username")]
        internal string Username { get; set; }
        [JsonProperty("email")]
        internal string Email { get; set; }
        [JsonProperty("organization")]
        internal string Organization { get; set; }
        [JsonProperty("phone")]
        internal string Phone { get; set; }

        //Web URL to picture
        //"https://pbs.twimg.com/profile_images/1442562237/012_n_400x400.jpg",
        [JsonProperty("picture")]
        internal string PictureUrl { get; set; }

        [JsonProperty("gender")]
        internal string Gender { get; set; }
        [JsonProperty("byear")]
        internal string BirthYear { get; set; }

        [JsonProperty("custom")]
        //dots (.) and dollar signs ($) in key names will be stripped out.
        internal Dictionary<string, object> Custom { get; set; }

        [JsonIgnore]
        internal static Dictionary<string, object> CustomDataProperties = new Dictionary<string, object> { };

        /// <summary>
        /// Initializes a new instance of User Model with the specified params
        /// </summary>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="organization"></param>
        /// <param name="phone"></param>
        /// <param name="pictureUrl"></param>
        /// <param name="gender"></param>
        /// <param name="birthYear"></param>
        /// <param name="customData"></param>
        public CountlyUserDetailsModel(string name, string username, string email, string organization, string phone,
                                    string pictureUrl, string gender, string birthYear, IDictionary<string, object> customData)
        {
            Name = name;
            Username = username;
            Email = email;
            Organization = organization;
            Phone = phone;
            PictureUrl = pictureUrl;
            Gender = gender;
            BirthYear = birthYear;
            if (customData != null)
                Custom = customData as Dictionary<string, object>;
        }

        /// <summary>
        /// This constructor is used to initialize custom user details only.
        /// </summary>
        /// <param name="customData"></param>
        public CountlyUserDetailsModel(IDictionary<string, object> customData)
        {
            Custom = customData as Dictionary<string, object>;
        }

        /// <summary>
        /// Uploads all user details
        /// </summary>
        /// <returns></returns>
        public async Task<CountlyResponse> SetUserDetailsAsync()
        {
            if (!CountlyHelper.IsPictureValid(PictureUrl))
                throw new Exception("Accepted picture formats are .png, .gif and .jpeg");

            var requestParams =
               new Dictionary<string, object>
               {
                    { "user_details", JsonConvert.SerializeObject(this, Formatting.Indented, 
                                        new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore }) },
               };

            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Uploads only custom data. Doesn't update any other property except Custom Data.
        /// </summary>
        /// <returns></returns>
        public async Task<CountlyResponse> SetCustomUserDetailsAsync()
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "user_details",
                        JsonConvert.SerializeObject(
                           new Dictionary<string, object>
                           {
                              { "custom", Custom }
                           })
                    }
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Sets value to key.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value)
        {
            AddToCustomData(key, value);
        }

        /// <summary>
        /// Sets value to key, only if property was not defined before for this user.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetOnce(string key, string value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$setOnce", value } });
        }

        /// <summary>
        /// To increment value, for the specified key, on the server by 1.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        public static void Increment(string key)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$inc", 1 } });
        }

        /// <summary>
        /// To increment value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void IncrementBy(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$inc", value } });
        }

        /// <summary>
        /// To multiply value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Multiply(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$mul", value } });
        }

        /// <summary>
        /// To store maximal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Max(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$max", value } });
        }

        /// <summary>
        /// To store minimal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Min(string key, double value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$min", value } });
        }

        /// <summary>
        /// Add one or many values to array property (can have multiple same values, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Push(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$push", value } });
        }

        /// <summary>
        /// Add one or many values to array property (will only store unique values in array, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void PushUnique(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$addToSet", value } });
        }

        /// <summary>
        /// Remove one or many values from array property (only removes value from array properties).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Pull(string key, string[] value)
        {
            AddToCustomData(key, new Dictionary<string, object> { { "$pull", value } });
        }

        /// <summary>
        /// Saves all custom user data updates done since the last save request.
        /// </summary>
        /// <returns></returns>
        public static async Task<CountlyResponse> SaveAsync()
        {
            if (!CustomDataProperties.Any())
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No data to save."
                };
            }

            var model = new CountlyUserDetailsModel(CustomDataProperties);

            CustomDataProperties = new Dictionary<string, object> { };
            return await model.SetCustomUserDetailsAsync();
        }

        #region Private Methods

        private static void AddToCustomData(string key, object value)
        {
            if (CustomDataProperties.ContainsKey(key))
            {
                var item = CustomDataProperties.Select(x => x.Key).FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    CustomDataProperties.Remove(item);
                }
            }

            CustomDataProperties.Add(key, value);
        }

        #endregion
    }
}
