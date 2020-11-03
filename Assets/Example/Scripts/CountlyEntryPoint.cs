using Notifications;
using Plugins.Countly;
using Plugins.Countly.Helpers;
using Plugins.Countly.Impl;
using Plugins.Countly.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CountlyEntryPoint : MonoBehaviour, INotificationListener
{
	public Plugins.Countly.Impl.Countly countlyPrefab;

	private ICountly countly;
	
	private void Awake ()
	{
        countly = Instantiate(countlyPrefab);

        /* You can use 'countlyWrapper' to call Countly functions without them sending any events to your server.
         * This might be useful while testing in the Unity Editor. */

        //#if  !UNITY_EDITOR
        //		countly = Instantiate(countly);      
        //#else
        //		countly = Instantiate(countlyWrapper);
        //#endif
    }

    private void Start()
    {
        countly.Notifications.AddListener(this);
    }

    private void Stop() {
        countly.Notifications.RemoveListener(this);
    }

    public async void BasicEvent()
    {
        await countly.Events.RecordEventAsync("Basic Event");
    }

    public async void EventWithSum()
    {
        await countly.Events.RecordEventAsync("Event With Sum", segmentation: null, sum: 23);
    }

    public async void EventWithSegmentation()
    {

        SegmentModel segment = new SegmentModel(new Dictionary<string, object>
{
            { "Time Spent", "60"},
            { "Retry Attempts", "10"}
        });

        await countly.Events.RecordEventAsync("Event With Segmentation", segmentation: segment);
    }

    public async void EventWithSumAndSegmentation()
    {
        SegmentModel segments = new SegmentModel(new Dictionary<string, object>{
            { "Time Spent", "1234455"},
            { "Retry Attempts", "10"}
        });

        await countly.Events.RecordEventAsync("Event With Sum And Segmentation", segmentation: segments, sum: 23);
       
    }

    public async void ReportViewMainScene()
    {
        await countly.Views.RecordOpenViewAsync("Main Scene");
        
    }

    public async void ReportViewHomeScene()
    {
        await countly.Views.RecordOpenViewAsync("Home Scene");
    }

    public void SetLocation()
    {
        string countryCode = "us";
        string city = "Houston";
        string latitude = "29.634933";
        string longitude = "-95.220255";
        string ipAddress = null;

        countly.OptionalParameters.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

    }

    public void DisableLocation()
    {
        countly.OptionalParameters.DisableLocation();
        
        
    }


    public async void SendCrashReport()
    {
        try
        {

            throw new DivideByZeroException();
        }
        catch (Exception ex)
        {
           await countly.CrashReports.SendCrashReportAsync(ex.Message, ex.StackTrace, LogType.Exception); 
        }
       
    }

    public async void SetRating()
    {
           await countly.StarRating.ReportStarRatingAsync("unity", "0.1", 3);
    }

    public async void SetUserDetail()
    {
        var userDetails = new CountlyUserDetailsModel(
                                  "Full Name", "username", "useremail@email.com", "Organization",
                                  "222-222-222",
                  "http://webresizer.com/images2/bird1_after.jpg",
          "M", "1986",
                                  new Dictionary<string, object>
                                  {
                                    { "Hair", "Black" },
                                    { "Race", "Asian" },
                                  });

        await countly.UserDetails.SetUserDetailsAsync(userDetails);

    }

    public async void SetCustomeUserDetail()
    {
        var userDetails = new CountlyUserDetailsModel(
                                new Dictionary<string, object>
                                {
            { "Nationality", "Turkish" },
                                    { "Height", "5.8" },
                                    { "Mole", "Lower Left Cheek" }
                 });

        await countly.UserDetails.SetUserDetailsAsync(userDetails);
    }

    public async void SetPropertyOnce()
    {
        countly.UserDetails.SetOnce("Distance", "10KM");
       await countly.UserDetails.SaveAsync();
        
    }

    public async void IncreamentValue()
    {
        countly.UserDetails.Increment("Weight");
        await countly.UserDetails.SaveAsync();

    }

    public async void IncreamentBy()
    {
        countly.UserDetails.IncrementBy("Weight", 2);
        await countly.UserDetails.SaveAsync();

    }

    public async void Multiply()
    {
        countly.UserDetails.Multiply("Weight", 2);
        await countly.UserDetails.SaveAsync();

    }

    public async void Max()
    {
        countly.UserDetails.Max("Weight", 90);
        await countly.UserDetails.SaveAsync();

    }

    public async void Min()
    {
        countly.UserDetails.Min("Weight", 10);
        await countly.UserDetails.SaveAsync();

    }

    public async void Push()
    {
        countly.UserDetails.Push("Area", new string[] { "width", "height" });
        await countly.UserDetails.SaveAsync();

    }

    public async void PushUnique()
    {
        countly.UserDetails.PushUnique("Mole", new string[] { "Left Cheek", "Left Cheek" });
        await countly.UserDetails.SaveAsync();

    }

    public async void Pull()
    {
        //Remove one or many values
        countly.UserDetails.Pull("Mole", new string[] { "Left Cheek" });
        await countly.UserDetails.SaveAsync();

    }

    public async void RecordMultiple()
    {
        //Remove one or many values
        countly.UserDetails.Max("Weight", 90);
        countly.UserDetails.SetOnce("Distance", "10KM");
        countly.UserDetails.Push("Mole", new string[] { "Left Cheek", "Back", "Toe" }); ;
        await countly.UserDetails.SaveAsync();

    }

    public async Task RemoteConfigAsync()
    {
        await countly.RemoteConfigs.Update();

        Dictionary<string, object> config = countly.RemoteConfigs.Configs;
        Debug.Log("RemoteConfig: " + config.ToString());
    }

    public void OnNotificationReceived(string message)
    {
        Debug.Log("[Countly Example] OnNotificationReceived: " + message);
    }

    public void OnNotificationClicked(string message, int index)
    {
        Debug.Log("[Countly Example] OnNoticicationClicked: " + message + ", index: " + index);
    }
}
