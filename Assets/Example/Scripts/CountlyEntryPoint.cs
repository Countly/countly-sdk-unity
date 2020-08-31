using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.Countly;
using Plugins.Countly.Impl;
using Plugins.Countly.Models;
using UnityEngine;

public class CountlyEntryPoint : MonoBehaviour
{
	public Plugins.Countly.Impl.Countly countly;
	public CountlyWrapper countlyWrapper;

	private ICountly _countly;
	
	private void Awake ()
	{
        _countly = Instantiate(countly);

        /* You can use 'countlyWrapper' to call Countly functions without them sending any events to your server.
         * This might be useful while testing in the Unity Editor. */

        //#if  !UNITY_EDITOR
        //		_countly = Instantiate(countly);      
        //#else
        //		_countly = Instantiate(countlyWrapper);
        //#endif
    }

    public async void BasicEvent()
    {
        await _countly.Events.RecordEventAsync("Basic Event");
    }

    public async void EventWithSum()
    {
        await _countly.Events.RecordEventAsync("Event With Sum", null, false, 1, 23, null);
    }

    public async void EventWithSegmentation()
    {

        SegmentModel segment = new SegmentModel(new Dictionary<string, object>{
            { "Time Spent", "1234455"},
            { "Retry Attempts", "10"}
        });
        await _countly.Events.RecordEventAsync("Event With Segmentation", segment);
    }

    public async void EventWithSumAndSegmentation()
    {
        SegmentModel segment = new SegmentModel(new Dictionary<string, object>{
            { "Time Spent", "1234455"},
            { "Retry Attempts", "10"}
        });

        await _countly.Events.RecordEventAsync("Event With Sum And Segmentation", segment, false, 1, 23, null);
       
    }

    public async void ReportViewMainScene()
    {
        await _countly.Views.RecordOpenViewAsync("Main Scene");
        
    }

    public async void ReportViewHomeScene()
    {
        await _countly.Views.RecordOpenViewAsync("Home Scene");
    }

    public void SetLocation()
    {
        _countly.OptionalParameters.SetLocation(41.884697, 12.578456);
    }

    public void SetCity()
    {
        _countly.OptionalParameters.SetCity("Lahore");
    }

    public void SetIPAddress()
    {
        _countly.OptionalParameters.SetIPAddress("192.168.1.10");
    }

    public void SetCountryCode()
    {
        _countly.OptionalParameters.SetCountryCode("PK");
    }

    public void DisableLocation()
    {
        _countly.OptionalParameters.DisableLocation();
        
        
    }


    public async void SendCrashReport()
    {
        try {

            throw new DivideByZeroException();
        }
        catch (Exception ex) {
           await _countly.CrushReports.SendCrashReportAsync(ex.Message, ex.StackTrace, LogType.Exception, null, true); 
        }
       
    }

    public async void SetRating()
    {
           await _countly.StarRating.ReportStarRatingAsync("unity_android", "0.1", 3);
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

        await _countly.UserDetails.SetUserDetailsAsync(userDetails);

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

        await _countly.UserDetails.SetUserDetailsAsync(userDetails);
    }

    public async void SetPropertyOnce()
    {
        _countly.UserDetails.SetOnce("Distance", "10KM");
       await _countly.UserDetails.SaveAsync();
        
    }

    public async void IncreamentValue()
    {
        _countly.UserDetails.Increment("Weight");
        await _countly.UserDetails.SaveAsync();

    }

    public async void IncreamentBy()
    {
        _countly.UserDetails.IncrementBy("Weight", 2);
        await _countly.UserDetails.SaveAsync();

    }

    public async void Multiply()
    {
        _countly.UserDetails.Multiply("Weight", 2);
        await _countly.UserDetails.SaveAsync();

    }

    public async void Max()
    {
        _countly.UserDetails.Max("Weight", 90);
        await _countly.UserDetails.SaveAsync();

    }

    public async void Min()
    {
        _countly.UserDetails.Min("Weight", 10);
        await _countly.UserDetails.SaveAsync();

    }

    public async void Push()
    {
        _countly.UserDetails.Push("Area", new string[] { "width", "height" });
        await _countly.UserDetails.SaveAsync();

    }

    public async void PushUnique()
    {
        _countly.UserDetails.PushUnique("Mole", new string[] { "Left Cheek", "Left Cheek" });
        await _countly.UserDetails.SaveAsync();

    }

    public async void Pull()
    {
        //Remove one or many values
        _countly.UserDetails.Pull("Mole", new string[] { "Left Cheek" });
        await _countly.UserDetails.SaveAsync();

    }

    public async void RecordMultiple()
    {
        //Remove one or many values
        _countly.UserDetails.Max("Weight", 90);
        _countly.UserDetails.SetOnce("Distance", "10KM");
        _countly.UserDetails.Push("Mole", new string[] { "Left Cheek", "Back", "Toe" }); ;
        await _countly.UserDetails.SaveAsync();

    }
}
