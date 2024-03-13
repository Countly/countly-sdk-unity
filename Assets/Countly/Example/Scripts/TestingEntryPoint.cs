using System;
using System.Collections.Generic;
using System.Threading;
using Notifications;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;

public class TestingEntryPoint : MonoBehaviour, INotificationListener
{
    private Countly countly;
    private readonly string _appKey = "YOUR_APP_KEY";
    private readonly string _serverUrl = "https://try.count.ly/";

    private void Awake()
    {
        countly = Countly.Instance;

        InitOnAnotherThread();
    }

    private void InitOnAnotherThread()
    {
        int participants = 2;
        Thread[] threads = new Thread[participants];
        threads[0] = new Thread(() =>
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .EnableLogging()
                .SetParameterTamperingProtectionSalt("test-salt-checksum")
                .EnableForcedHttpPost()
                .SetRequiresConsent(true)
                .SetEventQueueSizeToSend(1)
                .SetNotificationMode(TestMode.AndroidTestToken);

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
            configuration.AddNotificationListener(this);

            countly.Init(configuration);
        });

        threads[1] = new Thread(() =>
        {
            Debug.Log("This is another thread");
        });

        for (int i = 0; i < participants; i++) {
            threads[i].Start();
        }

        for (int i = 0; i < participants; i++) {
            threads[i].Join();
        }
    }


    public void TestWithMultipleThreads()
    {
        int participants = 13;
        Barrier barrier = new Barrier(participantCount: participants, postPhaseAction: (_) => {
            Debug.Log("All threads reached the barrier at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        });

        Thread[] threads = new Thread[participants];
        threads[0] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[00] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                _ = Countly.Instance.Events.RecordEventAsync("Event With Segmentation", sum: 10);
                Debug.Log("Thread[00] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[1] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[01] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Dictionary<string, object> segment = new Dictionary<string, object> {
                { "Time Spent", "60"},
                { "Retry Attempts", "10"}
                };

                _ = Countly.Instance.Events.RecordEventAsync("Event With Segmentation", segmentation: segment);

                Debug.Log("Thread[01] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[2] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[02] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Dictionary<string, object> segment = new Dictionary<string, object> {
                { "Time Spent", "60"},
                { "Retry Attempts", "10"}
                };

                _ = Countly.Instance.Events.RecordEventAsync("Event With Segmentation", segmentation: segment, sum: 5);

                Debug.Log("Thread[02] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[3] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[03] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                _ = Countly.Instance.Views.RecordOpenViewAsync("View A");
                Debug.Log("Thread[03] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[4] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[04] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time Spent", "1234455"},
                { "Retry Attempts", "10"}
                };
                _ = Countly.Instance.CrashReports.SendCrashReportAsync("Exception", "Stacktrace", seg);


                Debug.Log("Thread[04] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[5] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[05] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Debug.Log("Thread[05] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[6] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[06] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Countly.Instance.UserDetails.Pull("Food", new []{ "Pizza" });
                _ = Countly.Instance.UserDetails.SaveAsync();

                Debug.Log("Thread[06] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[7] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[07] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Countly.Instance.UserDetails.PushUnique("Mole", new []{ "Right foot", "Left foot" });
                _ = Countly.Instance.UserDetails.SaveAsync();

                Debug.Log("Thread[07] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[8] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[08] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Countly.Instance.UserDetails.PushUnique("Animals", new []{ "Lion", "Tiger" });
                _ = Countly.Instance.UserDetails.SaveAsync();

                Debug.Log("Thread[08] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[9] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[09] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Countly.Instance.UserDetails.Min("Weight", 10);
                _ = Countly.Instance.UserDetails.SaveAsync();

                Debug.Log("Thread[09] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[10] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[10] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Countly.Instance.UserDetails.Max("Weight", 90);
                Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
                Countly.Instance.UserDetails.Push("FootballTeams", new []{ "Arsenal", "Chelsea", "Barcelona" });

                _ = Countly.Instance.UserDetails.SaveAsync();

                Debug.Log("Thread[10] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[11] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[11] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                CountlyUserDetailsModel userDetails = new CountlyUserDetailsModel(
                                  "Full Name", "username", "useremail@email.com", "Organization",
                                  "222-222-222",
                  "http://webresizer.com/images2/bird1_after.jpg",
                    "M", "1986",
                                  new Dictionary<string, object>
                                  {
                                    { "Beverage", "Coke" },
                                    { "FavoritePlayer", "Messi" },
                                  });

                _ = Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);

                Debug.Log("Thread[11] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[12] = new Thread(() => {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[12] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Dictionary<string, object> userCustomDetail = new Dictionary<string, object> {
                            { "DeviceOS", "Android" },
                            { "Vehicle", "Car" },
                };
                Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
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

    public void SetLocation()
    {
        string countryCode = "uk";
        string city = "??";
        string latitude = "51.5072";
        string longitude = "0.1276";
        string ipAddress = null;

        countly.Location.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
    }

    public void DisableLocation()
    {
        countly.Location.DisableLocation();
    }

    public void SetRatingAsync()
    {
        _ = countly.StarRating.ReportStarRatingAsync("unity", "0.1", 3);
    }

    public void RemoteConfigAsync()
    {
        _ = countly.RemoteConfigs.Update();

        Dictionary<string, object> config = countly.RemoteConfigs.Configs;
        Debug.Log("RemoteConfig: " + config?.ToString());
    }


    #region interface methods
    public void OnNotificationReceived(string message)
    {
        Debug.Log("[Example] OnNotificationReceived: " + message);
    }

    public void OnNotificationClicked(string message, int index)
    {
        Debug.Log("[Example] OnNotificationClicked: " + message + ", index: " + index);
    }
    #endregion
}
