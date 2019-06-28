using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;

namespace Plugins.Countly.Services.Impls.Actual
{
    public interface IUserDetailsCountlyService
    {
        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        Task<CountlyResponse> UserDetailsAsync(CountlyUserDetailsModel userDetails);

        /// <summary>
        /// Modifies custom user data only. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returCountlyUserDetailsModel.Pushns></returns>
        Task<CountlyResponse> UserCustomDetailsAsync(CountlyUserDetailsModel userDetails);

        /// <summary>
        /// Uploads all user details
        /// </summary>
        /// <returns></returns>
        Task<CountlyResponse> SetUserDetailsAsync(CountlyUserDetailsModel userDetailsModel);

        /// <summary>
        /// Uploads only custom data. Doesn't update any other property except Custom Data.
        /// </summary>
        /// <returns></returns>
        Task<CountlyResponse> SetCustomUserDetailsAsync(CountlyUserDetailsModel userDetailsModel);

        /// <summary>
        /// Saves all custom user data updates done since the last save request.
        /// </summary>
        /// <returns></returns>
        Task<CountlyResponse> SaveAsync();

        /// <summary>
        /// Sets value to key.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, string value);

        /// <summary>
        /// Sets value to key, only if property was not defined before for this user.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetOnce(string key, string value);

        /// <summary>
        /// To increment value, for the specified key, on the server by 1.
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        void Increment(string key);

        /// <summary>
        /// To increment value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void IncrementBy(string key, double value);

        /// <summary>
        /// To multiply value on server by provided value (if no value on server, assumes it is 0).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Multiply(string key, double value);

        /// <summary>
        /// To store maximal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Max(string key, double value);

        /// <summary>
        /// To store minimal value from the one on server and provided value (if no value on server, uses provided value).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Min(string key, double value);

        /// <summary>
        /// Add one or many values to array property (can have multiple same values, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Push(string key, string[] value);

        /// <summary>
        /// Add one or many values to array property (will only store unique values in array, if property is not array, converts it to array).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void PushUnique(string key, string[] value);

        /// <summary>
        /// Remove one or many values from array property (only removes value from array properties).
        /// Doesn't report it to the server until save is called.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Pull(string key, string[] value);

        void AddToCustomData(string key, object value);
    }
}