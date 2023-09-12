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
                                    { "Race", "Asian" },
                                  });

        await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);

    }

    public async void SetCustomeUserDetail()
    {
        CountlyUserDetailsModel userDetails = new CountlyUserDetailsModel(
                                new Dictionary<string, object>
                                {
            { "Nationality", "Turkish" },
                                    { "Height", "5.8" },
                                    { "Mole", "Lower Left Cheek" }
                 });

        await Countly.Instance.UserDetails.SetCustomUserDetailsAsync(userDetails);
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

    public async void IncrementBy()
    {
        Countly.Instance.UserDetails.IncrementBy("Weight", 2);
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void Multiply()
    {
        Countly.Instance.UserDetails.Multiply("Weight", 2);
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void Max()
    {
        Countly.Instance.UserDetails.Max("Weight", 90);
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void Min()
    {
        Countly.Instance.UserDetails.Min("Weight", 10);
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void Push()
    {
        Countly.Instance.UserDetails.Push("Area", new string[] { "width", "height" });
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void PushUnique()
    {
        Countly.Instance.UserDetails.PushUnique("Mole", new string[] { "Left Cheek", "Left Cheek" });
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void Pull()
    {
        //Remove one or many values
        Countly.Instance.UserDetails.Pull("Mole", new string[] { "Left Cheek" });
        await Countly.Instance.UserDetails.SaveAsync();

    }

    public async void RecordMultiple()
    {
        //Remove one or many values
        Countly.Instance.UserDetails.Max("Weight", 90);
        Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
        Countly.Instance.UserDetails.Push("Mole", new string[] { "Left Cheek", "Back", "Toe" });
        await Countly.Instance.UserDetails.SaveAsync();

    }
}
