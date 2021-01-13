using Notifications;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CountlyEntryPoint : MonoBehaviour, INotificationListener
{
    public Countly countlyPrefab;

    private Countly countly;

    private void Awake()
    {
        CountlyConfiguration configuration = new CountlyConfiguration {
            ServerUrl = "https://try.count.ly/",
            AppKey = "YOUR_APP_KEY",
            EnableConsoleLogging = true,
            NotificationMode = TestMode.AndroidTestToken
        };

        string countryCode = "us";
        string city = "Houston";
        string latitude = "29.634933";
        string longitude = "-95.220255";
        string ipAddress = "10.2.33.12";

        configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);


        Countly.Instance.Init(configuration);
        countly = Countly.Instance;
    }

    private void Start()
    {
        countly.Notifications.AddListener(this);
    }

    private void Stop()
    {
        countly.Notifications.RemoveListener(this);
    }

    public void TestWithMultipleThreads()
    {

        int participants = 13;
        Barrier barrier = new Barrier(participantCount: participants, postPhaseAction: (bar) => {
            Debug.Log("All threads reached the barrier at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        });

        Thread[] threads = new Thread[participants];
        threads[0] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[00] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                EventWithSum();
                Debug.Log("Thread[00] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });


        threads[1] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[01] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                EventWithSegmentation();
                Debug.Log("Thread[01] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[2] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[02] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                EventWithSumAndSegmentation();
                Debug.Log("Thread[02] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[3] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[03] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                ReportViewMainScene();
                Debug.Log("Thread[03] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[4] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[04] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                SendCrashReport();
                Debug.Log("Thread[04] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[5] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[05] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                RecordMultiple();
                Debug.Log("Thread[05] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[6] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[06] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Pull();
                Debug.Log("Thread[06] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[7] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[07] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Push();
                Debug.Log("Thread[07] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[8] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[08] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                PushUnique();
                Debug.Log("Thread[08] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[9] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[09] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Min();
                Debug.Log("Thread[09] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[10] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[10] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Multiply();
                Debug.Log("Thread[10] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[11] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[11] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                SetUserDetail();
                Debug.Log("Thread[11] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[12] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[12] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                SetCustomeUserDetail();
                Debug.Log("Thread[12] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        for (int i = 0; i < participants; i++) {
            threads[i].Start();
        }


        for (int i = 0; i < participants; i++) {
            threads[i].Join();
        }

        Debug.Log("All threads completed at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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

    public async void EventWithInvalidSegmentation()
    {
        int moles = 1; //valid data type
        string name = "foo";// valid data type
        bool isMale = true; // valid data type
        float amount = 10000.75f; //valid data type
        double totalAmount = 100000.76363;
        long currentMillis = DateTime.UtcNow.Millisecond; // invalid data type
        DateTime date = DateTime.UtcNow; // invalid data type

        SegmentModel segment = new SegmentModel(new Dictionary<string, object>
        {
            { "name", name},
            { "moles", moles},
            { "male", isMale},
            { "amount", amount},
            { "total amount", totalAmount},
            { "dob", date},
            { "Current Millis", currentMillis},
        });

        await countly.Events.RecordEventAsync("Event With Invalid Segmentation", segmentation: segment);
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

        countly.Location.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

    }

    public void DisableLocation()
    {
        countly.Location.DisableLocation();
    }


    public async void SendCrashReport()
    {
        try {

            throw new DivideByZeroException();
        } catch (Exception ex) {
            await countly.CrashReports.SendCrashReportAsync(ex.Message, ex.StackTrace, LogType.Exception);
        }

    }

    public async void SetRating()
    {
        await countly.StarRating.ReportStarRatingAsync("unity", "0.1", 3);
    }

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

        await countly.UserDetails.SetUserDetailsAsync(userDetails);

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
        countly.UserDetails.Push("Mole", new string[] { "Left Cheek", "Back", "Toe" });
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
