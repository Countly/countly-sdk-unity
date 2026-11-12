using Notifications;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDetail : MonoBehaviour
{
    public void Increment()
    {
        Countly.Instance.UserProfile.Increment("Samurai");
    }

    public void IncrementBy()
    {
        Countly.Instance.UserProfile.IncrementBy("Silverhand", 2077);
    }

    public void SaveMax()
    {
        Countly.Instance.UserProfile.SaveMax("PowerLevel", 9001);
    }

    public void SaveMin()
    {
        Countly.Instance.UserProfile.SaveMin("BestTime", 23);
    }

    public void Multiply()
    {
        Countly.Instance.UserProfile.Multiply("CurrentScore", 2);
    }

    public void Pull()
    {
        Countly.Instance.UserProfile.Pull("Inventory", "Blackblade");
    }

    public void Push()
    {
        Countly.Instance.UserProfile.Push("Backpack", "Unity");
    }

    public void PushUnique()
    {
        Countly.Instance.UserProfile.PushUnique("OneTimeItems", "AccessCard");
    }

    public void Save()
    {
        Countly.Instance.UserProfile.Save();
    }

    public void SetOnce()
    {
        Countly.Instance.UserProfile.SetOnce("ChosenEnding", "Ending1");
    }

    public void SetProperties()
    {
        Dictionary<string, object> userProperties = new Dictionary<string, object> {
            { "name", "Johnny Silverhand" },
            { "username", "silverhand" },
            { "email", "info@samurai.com" },
            { "organization", "Samurai" },
            { "phone", "+1 555 123 4567" },
            { "picture", "https://static1.thegamerimages.com/wordpress/wp-content/uploads/2021/01/cyberpunk-2077-johnny-silverhand-glasses.jpg" },
            { "gender", "M" },
            { "byear", 1988 },
            { "Boolean", true },
            { "Integer", 34 },
            { "Float", 6.2f },
            { "Quote", "Wake up, Samurai. We have a city to burn." },
            { "SignificantYears", new [] { 2020, 2077, 2088 } },
            { "TruthValues", new [] { true, false, true } },
            { "SkillLevels", new [] { 1.5f, 2.5f, 3.5f } },
            { "FavoriteItems", new [] { "guitar", "revolver", "cyberarm" } },
            { "YearList", new List<int> { 2020, 2077, 2088 } },
            { "BooleanList", new List<bool> { true, false, true } },
            { "SkillLevelList", new List<float> { 1.5f, 2.5f, 3.5f } },
            { "FavoriteItemList", new List<string> { "guitar", "revolver", "cyberarm" } }
        };

        Countly.Instance.UserProfile.SetProperties(userProperties);
    }

    public void SetProperty()
    {
        Countly.Instance.UserProfile.SetProperty("Best Attribute", "Int");
    }

    #region LegacyCode
    public async void SetUserDetail()
    {
        CountlyUserDetailsModel userDetails = new CountlyUserDetailsModel(
                                  "Full Name", "username", "useremail@email.com", "Organization",
                                  "222-222-222",
                  "http://webresizer.com/images2/bird1_after.jpg",
          "M", "1986",
                                  new Dictionary<string, object>
                                  {
                                    { "Hair", "Black" },
                                    { "Age", "30" },
                                  });

        await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);

    }

    public async void SetCustomUserDetail()
    {
        Dictionary<string, object> userCustomDetail = new Dictionary<string, object> {
                        { "Language", "English" },
                        { "Height", "5.9" },
                        { "Empty Value", "" }, // valid
                        { "Null Value", null } // invalid
            };
        Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);

        await Task.CompletedTask;
    }

    public async void SetPropertyOnce()
    {
        Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void IncrementValue()
    {
        Countly.Instance.UserDetails.Increment("Weight");
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void IncrementBy_Legacy()
    {
        Countly.Instance.UserDetails.IncrementBy("ShoeSize", 2);
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Multiply_Legacy()
    {
        Countly.Instance.UserDetails.Multiply("PetNumber", 2);
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Max()
    {
        Countly.Instance.UserDetails.Max("TravelDistance", 90);
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Min()
    {
        Countly.Instance.UserDetails.Min("YearsExperience", 10);
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Push_Legacy()
    {
        Countly.Instance.UserDetails.Push("Area", new string[] { "width", "height" });
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void PushUnique_Legacy()
    {
        Countly.Instance.UserDetails.PushUnique("Mole", new string[] { "Left Cheek", "Right Cheek" });
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Pull_Legacy()
    {
        //Remove one or many values
        Countly.Instance.UserDetails.Pull("Cat", new string[] { "Claw" });
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void RecordMultiple()
    {
        //Remove one or many values
        Countly.Instance.UserDetails.Max("Income", 9000);
        Countly.Instance.UserDetails.SetOnce("FavoriteColor", "Blue");
        Countly.Instance.UserDetails.Push("Inventory", new string[] { "Sword", "Shield", "Armor" });
        await Countly.Instance.UserDetails.SaveAsync();
    }

    public async void Save_Legacy()
    {
        await Countly.Instance.UserDetails.SaveAsync();
    }
    #endregion
}
