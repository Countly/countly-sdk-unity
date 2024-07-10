using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;

public class UserProfile : AbstractBaseService, IUserProfileModule
{
    #region UserProfileData Parameters and Keys
    internal string Name { get; set; }
    internal string Username { get; set; }
    internal string Email { get; set; }
    internal string Organization { get; set; }
    internal string Phone { get; set; }
    internal string PictureUrl { get; set; }
    internal string Gender { get; set; }
    internal int BirthYear { get; set; }
    static string NAME_KEY = "name";
    static string USERNAME_KEY = "username";
    static string EMAIL_KEY = "email";
    static string ORG_KEY = "organization";
    static string PHONE_KEY = "phone";
    static string PICTURE_KEY = "picture";
    static string GENDER_KEY = "gender";
    static string BYEAR_KEY = "byear";
    static string CUSTOM_KEY = "custom";
    readonly string[] NamedFields = { NAME_KEY, USERNAME_KEY, EMAIL_KEY, ORG_KEY, PHONE_KEY, PICTURE_KEY, GENDER_KEY, BYEAR_KEY };
    #endregion

    internal Dictionary<string, object> CustomDataProperties { get; private set; }
    private readonly Countly cly;
    private readonly CountlyConfiguration config;
    private readonly CountlyUtils utils;
    internal readonly RequestCountlyHelper requestHelper;

    internal UserProfile(Countly countly, CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, CountlyUtils countlyUtils, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
    {
        Log.Debug("[UserProfile] Initializing.");

        cly = countly;
        config = configuration;
        utils = countlyUtils;
        requestHelper = requestCountlyHelper;
        CustomDataProperties = new Dictionary<string, object>();
    }

    #region PublicAPI
    /// <summary>
    /// Increment custom property value by 1.
    /// </summary>
    /// <param name="key">string with property name to increment</param>
    public void Increment(string key)
    {
        Log.Info("[UserProfile] Calling Increment with key: " + key);
        IncrementInternal(key, 1);
    }

    /// <summary>
    /// Increment custom property value by provided value.
    /// </summary>
    /// <param name="key">string with property name to increment</param>
    /// <param name="value">value by which to increment</param>
    public void IncrementBy(string key, double value)
    {
        Log.Info("[UserProfile] Calling IncrementBy with key: " + key + " and value: " + value);
        IncrementInternal(key, value);
    }

    /// <summary>
    /// Save maximal value between existing and provided.
    /// </summary>
    /// <param name="key">string with property name to check for max</param>
    /// <param name="value">value to check for max</param>
    public void Max(string key, double value)
    {
        Log.Info("[UserProfile] Calling Max with key: " + key + " and value: " + value);
        MaxInternal(key, value);
    }

    /// <summary>
    /// Save minimal value between existing and provided.
    /// </summary>
    /// <param name="key">string with property name to check for min</param>
    /// <param name="value">value to check for min</param>
    public void Min(string key, double value)
    {
        Log.Info("[UserProfile] Calling Min with key: " + key + " and value: " + value);
        MinInternal(key, value);
    }

    /// <summary>
    /// Multiply custom property value by provided value.
    /// </summary>
    /// <param name="key">string with property name to multiply</param>
    /// <param name="value">value by which to multiply</param>
    public void Multiply(string key, double value)
    {
        Log.Info("[UserProfile] Calling Multiply with key: " + key + " and value: " + value);
        MultiplyInternal(key, value);
    }

    /// <summary>
    /// Create array property, if property does not exist and remove value from array.
    /// You can only use it on array properties or properties that do not exist yet.
    /// </summary>
    /// <param name="key">string with property name for array property</param>
    /// <param name="value">string with value to remove from array</param>
    public void Pull(string key, string[] value)
    {
        Log.Info("[UserProfile] Calling Pull with key: " + key + " and value: " + value.ToString());
        PullInternal(key, value);
    }

    /// <summary>
    /// Create array property, if property does not exist and add value to array.
    /// You can only use it on array properties or properties that do not exist yet
    /// </summary>
    /// <param name="key">string with property name for array property</param>
    /// <param name="value">string with value to add to array</param>
    public void Push(string key, string[] value)
    {
        Log.Info("[UserProfile] Calling Push with key: " + key + " and value: " + value.ToString());
        PushInternal(key, value);
    }

    /// <summary>
    /// Create array property, if property does not exist and add value to array, only if value is not yet in the array.
    /// You can only use it on array properties or properties that do not exist yet
    /// </summary>
    /// <param name="key">string with property name for array property</param>
    /// <param name="value">string with value to add to array</param>
    public void PushUnique(string key, string[] value)
    {
        Log.Info("[UserProfile] Calling PushUnique with key: " + key + " and value: " + value.ToString());
        PushUniqueInternal(key, value);
    }

    /// <summary>
    /// Send provided values to server.
    /// </summary>
    public void Save()
    {
        Log.Info("[UserProfile] Calling Save");
        SaveInternal();
    }

    /// <summary>
    /// Sets user data values.
    /// </summary>
    /// <param name="userData">Dictionary with user data</param>
    public void SetData(Dictionary<string, object> userData)
    {
        Log.Info("[UserProfile] Calling SetData for " + userData.Count + " values");
        SetDataInternal(userData);
    }

    /// <summary>
    /// Set value only if property does not exist yet.
    /// </summary>
    /// <param name="key">string with property name to set</param>
    /// <param name="value">value string value to set</param>
    public void SetOnce(string key, string value)
    {
        Log.Info("[UserProfile] Calling SetOnce with key: " + key + " and value: " + value);
        SetOnceInternal(key, value);
    }

    /// <summary>
    /// Provide a Dictionary of user properties to set.
    /// Those can be either custom user properties or predefined user properties.
    /// </summary>
    /// <param name="data">Dictionary with data to set</param>
    public void SetProperties(Dictionary<string, object> data)
    {
        Log.Info("[UserProfile] Calling SetProperties for " + data.Count + " values");
        SetPropertiesInternal(data);
    }

    /// <summary>
    /// Set a single user property. It can be either a custom one or one of the predefined ones.
    /// </summary>
    /// <param name="key">string with key for the user property</param>
    /// <param name="value">value for the user property to be set. The value should be the allowed data type.</param>
    public void SetProperty(string key, object value)
    {
        Log.Info("[UserProfile] Calling SetProperty with key: " + key + " and value: " + value);
        SetPropertyInternal(key, value);
    }
    #endregion
    #region Internal Calls
    private void IncrementInternal(string key, double value)
    {
        if (!ValidateConsentAndKey(key, "IncrementInternal")) {
            return;
        }

        Log.Info("[UserProfile] IncrementInternal, key:[" + key + "]" + " value:[" + value + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$inc", value } });
    }

    private void MaxInternal(string key, double value)
    {
        if (!ValidateConsentAndKey(key, "MaxInternal")) {
            return;
        }

        Log.Info("[UserProfile] MaxInternal, key:[" + key + "]" + " value:[" + value + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$max", value } });
    }

    private void MinInternal(string key, double value)
    {
        if (!ValidateConsentAndKey(key, "MinInternal")) {
            return;
        }

        Log.Info("[UserProfile] MinInternal, key:[" + key + "]" + " value:[" + value + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$min", value } });
    }

    private void MultiplyInternal(string key, double value)
    {
        if (!ValidateConsentAndKey(key, "MultiplyInternal")) {
            return;
        }

        Log.Info("[UserProfile] MultiplyInternal, key:[" + key + "]" + " value:[" + value + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$mul", value } });
    }

    private void PullInternal(string key, string[] value)
    {
        if (!ValidateConsentAndKey(key, "PullInternal")) {
            return;
        }

        Log.Info("[UserProfile] PullInternal, key:[" + key + "]" + " value:[" + value + "]");
        value = TrimValues(value);
        AddToCustomData(key, new Dictionary<string, object> { { "$pull", value } });
    }

    private void PushInternal(string key, string[] value)
    {
        if (!ValidateConsentAndKey(key, "PushInternal")) {
            return;
        }

        Log.Info("[UserProfile] PushInternal, key:[" + key + "]" + " value:[" + string.Join(", ", value) + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$push", TrimValues(value) } });
    }

    private void PushUniqueInternal(string key, string[] value)
    {
        if (!ValidateConsentAndKey(key)) {
            return;
        }

        Log.Info("[UserProfile] PushUniqueInternal, key:[" + key + "]" + " value:[" + string.Join(", ", value) + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$addToSet", TrimValues(value) } });
    }

    private void SaveInternal()
    {
        if (!_consentService.CheckConsentInternal(Consents.Users)) {
            Log.Debug("[UserProfile] SaveInternal, consent is not given, ignoring the request.");
            return;
        }
        string cachedUserData = GetDataForRequest();
        if (utils.IsNullEmptyOrWhitespace(cachedUserData)) {
            Log.Debug("[UserProfile] SaveInternal, no user data to save");
            return;
        }
        
        Dictionary<string, object> requestParams = Converter.ConvertJsonToDictionary(cachedUserData, Log);
        cly.Events.AddEventsToRequestQueue(true);
        requestHelper.AddRequestDirectlyToQueue(requestParams);
        _ = requestHelper.ProcessQueue();
        ClearInternal();
    }

    private void SetDataInternal(Dictionary<string, object> userData)
    {
        if (!_consentService.CheckConsentInternal(Consents.Users)) {
            Log.Debug("[UserProfile][SetDataInternal] Consent is not given, ignoring the request.");
            return;
        }

        if (userData.Count <= 0) {
            Log.Debug("[UserProfile][SetDataInternal] Provided userData is empty, ignoring the request.");
            return;
        }

        if (userData.TryGetValue(NAME_KEY, out object nameValue) && nameValue is string name) {
            Name = TrimValue(NAME_KEY, name);
        }

        if (userData.TryGetValue(USERNAME_KEY, out object usernameValue) && usernameValue is string username) {
            Username = TrimValue(USERNAME_KEY, username);
        }

        if (userData.TryGetValue(EMAIL_KEY, out object emailValue) && emailValue is string email) {
            Email = TrimValue(EMAIL_KEY, email);
        }

        if (userData.TryGetValue(ORG_KEY, out object orgValue) && orgValue is string organization) {
            Organization = TrimValue(ORG_KEY, organization);
        }

        if (userData.TryGetValue(PHONE_KEY, out object phoneValue) && phoneValue is string phone) {
            Phone = TrimValue(PHONE_KEY, phone);
        }

        if (userData.TryGetValue(PICTURE_KEY, out object pictureValue) && pictureValue is string pictureUrl) {
            if (!utils.IsPictureValid(pictureUrl)) {
                Log.Warning($"[UserDetailsCountlyService] SetUserDetailsInternal, Picture format for URL '{pictureUrl}' is not as expected. Expected formats are .png, .gif, or .jpeg");
            } else if (pictureUrl != null && pictureUrl.Length > 4096) {
                PictureUrl = pictureUrl.Substring(0, 4096);
            }
        }

        if (userData.TryGetValue(GENDER_KEY, out object genderValue) && genderValue is string gender) {
            Gender = TrimValue(GENDER_KEY, gender);
        }

        if (userData.TryGetValue(BYEAR_KEY, out object birthYearValue) && birthYearValue is int birthYear) {
            BirthYear = birthYear;
        }

        if (userData.TryGetValue(CUSTOM_KEY, out object custom) && custom is Dictionary<string, object>) {
            Dictionary<string, object> customDictionary = (Dictionary<string, object>)FixSegmentKeysAndValues((IDictionary<string, object>)custom);
            utils.CopyDictionaryToDestination(CustomDataProperties, customDictionary, Log);
        }
    }

    private void SetPropertiesInternal(Dictionary<string, object> data)
    {
        if (!_consentService.CheckConsentInternal(Consents.Users)) {
            Log.Debug("[UserProfile][SetPropertiesInternal] Consent is not given, ignoring the request.");
            return;
        }

        if (data.Count <= 0) {
            Log.Debug("[UserProfile][SetPropertiesInternal] Provided data is empty, ignoring the request.");
            return;
        }

        Dictionary<string, object> namedFields = new Dictionary<string, object>();
        Dictionary<string, object> customFields = new Dictionary<string, object>();

        // separate named user fields and custom fields
        foreach (KeyValuePair<string, object> kvp in data) {
            if (NamedFields.Contains(value: kvp.Key) && kvp.Value != null) {
                namedFields.Add(kvp.Key, kvp.Value);
            } else if (!NamedFields.Contains(kvp.Key) && kvp.Value != null) {
                customFields.Add(kvp.Key, kvp.Value);
            }
        }

        // set user data
        FixSegmentKeysAndValues(namedFields);
        SetDataInternal(namedFields);

        // set custom data
        FixSegmentKeysAndValues(customFields);
        utils.CopyDictionaryToDestination(CustomDataProperties, customFields, Log);
    }

    private void SetPropertyInternal(string key, object value)
    {
        if (!ValidateConsentAndKey(key, "SetPropertyInternal")) {
            return;
        }

        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add(key, value);

        SetPropertiesInternal(data);
    }

    private void SetOnceInternal(string key, string value)
    {
        if (!ValidateConsentAndKey(key, "SetOnceInternal")) {
            return;
        }

        Log.Info("[UserProfile] SetOnceInternal, key:[" + key + "]" + " value:[" + value + "]");
        AddToCustomData(key, new Dictionary<string, object> { { "$setOnce", TrimValue(key, value) } });
    }
    #endregion
    #region Helper Methods
    private void AddToCustomData(string key, object value)
    {
        Log.Debug("[UserProfile] AddToCustomData, key:[" + key + "]" + " value:[" + value + "]");

        if (!ValidateConsentAndKey(key, "AddToCustomData")) {
            return;
        }

        key = TrimKey(key);

        if (CustomDataProperties.ContainsKey(key)) {
            string item = CustomDataProperties.Select(x => x.Key).FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (item != null) {
                CustomDataProperties.Remove(item);
            }
        }

        CustomDataProperties.Add(key, value);
    }

    private void ClearInternal()
    {
        Log.Debug("[UserProfile] ClearInternal");

        Name = null;
        Username = null;
        Email = null;
        Organization = null;
        Phone = null;
        PictureUrl = null;
        Gender = null;
        BirthYear = 0;
        CustomDataProperties = new Dictionary<string, object>();
    }

    private string ConvertToJSON()
    {
        JObject json = new JObject();

        try {
            if (utils.IsNullEmptyOrWhitespace(Name)) {
                json.Add(NAME_KEY, null);
            } else {
                json.Add(NAME_KEY, Name);
            }

            if (utils.IsNullEmptyOrWhitespace(Username)) {
                json.Add(USERNAME_KEY, null);
            } else {
                json.Add(USERNAME_KEY, Username);
            }

            if (utils.IsNullEmptyOrWhitespace(Email)) {
                json.Add(EMAIL_KEY, null);
            } else {
                json.Add(EMAIL_KEY, Email);
            }

            if (utils.IsNullEmptyOrWhitespace(Organization)) {
                json.Add(ORG_KEY, null);
            } else {
                json.Add(ORG_KEY, Organization);
            }

            if (utils.IsNullEmptyOrWhitespace(Phone)) {
                json.Add(PHONE_KEY, null);
            } else {
                json.Add(PHONE_KEY, Phone);
            }

            if (utils.IsNullEmptyOrWhitespace(PictureUrl) || !utils.IsPictureValid(PictureUrl)) {
                json.Add(PICTURE_KEY, null);
            } else {
                json.Add(PICTURE_KEY, PictureUrl);
            }

            if (utils.IsNullEmptyOrWhitespace(Gender)) {
                json.Add(GENDER_KEY, null);
            } else {
                json.Add(GENDER_KEY, Gender);
            }

            if (BirthYear != 0) {
                if (BirthYear > 0) {
                    json.Add(BYEAR_KEY, BirthYear);
                } else {
                    json.Add(BYEAR_KEY, null);
                }
            }

            JObject ob;
            if (CustomDataProperties != null) {
                utils.TruncateSegmentationValues(CustomDataProperties, config.GetMaxSegmentationValues(), "[UserProfile][ConvertToJSON]", Log);
                ob = JObject.FromObject(CustomDataProperties);
            } else {
                ob = new JObject();
            }

            json.Add(CUSTOM_KEY, ob);

        } catch (JsonException e) {
            Log.Warning($"[UserProfile] Got exception converting an UserData to JSON. Exception:{e}");
        }

        return json.ToString();
    }

    private string GetDataForRequest()
    {
        string json = ConvertToJSON();
        if (utils.IsNullEmptyOrWhitespace(json)) {
            json += "&user_details=" + json;
        } else {
            json = "";
        }
        return json;
    }

    private bool ValidateConsentAndKey(string key, string caller = null)
    {
        if (!_consentService.CheckConsentInternal(Consents.Users)) {
            Log.Debug($"[UserProfile][{caller}] Consent is not given, ignoring the request.");
            return false;
        }

        if (string.IsNullOrEmpty(key)) {
            Log.Warning($"[UserProfile][{caller}] Provided key isn't valid.");
            return false;
        }

        return true;
    }
    #endregion
}