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

public class CountlyEntryPoint : MonoBehaviour, INotificationListener
{
    public Countly countlyPrefab;

    private Countly countly;

    private void Awake()
    {
        if (Countly.Instance.IsSDKInitialized) {
            return;
        }

        CountlyConfiguration configuration = new CountlyConfiguration {
            ServerUrl = "https://try.count.ly/",
            AppKey = "YOUR_APP_KEY",
            EnableConsoleLogging = true,
            Salt = "test-salt-checksum",
            EnablePost = false,
            RequiresConsent = true,
            EventQueueThreshold = 1,
            NotificationMode = TestMode.AndroidTestToken
        };

        string countryCode = "us";
        string city = "Böston’ 墨尔本";
        string latitude = "29.634933";
        string longitude = "-95.220255";
        string ipAddress = "10.2.33.12";

        configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
        configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
        configuration.AddNotificationListener(this);

        Countly.Instance.Init(configuration);
        countly = Countly.Instance;
    }

    private void OnApplicationQuit()
    {
        Countly.Instance?.Notifications?.RemoveListener(this);

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
                //EventWithSum();
                Debug.Log("Thread[00] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });


        threads[1] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[01] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //EventWithSegmentation();
                Debug.Log("Thread[01] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[2] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[02] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
               //EventWithSumAndSegmentation();
                Debug.Log("Thread[02] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[3] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[03] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //ReportViewMainScene();
                Debug.Log("Thread[03] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[4] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[04] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //SendCrashReport();
                Debug.Log("Thread[04] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[5] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[05] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
              //  RecordMultiple();
                Debug.Log("Thread[05] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[6] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[06] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //Pull();
                Debug.Log("Thread[06] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[7] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[07] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //Push();
                Debug.Log("Thread[07] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[8] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[08] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
               // PushUnique();
                Debug.Log("Thread[08] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[9] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[09] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //Min();
                Debug.Log("Thread[09] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });

        threads[10] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[10] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //Multiply();
                Debug.Log("Thread[10] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[11] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[11] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                //SetUserDetail();
                Debug.Log("Thread[11] finished at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
        threads[12] = new Thread(delegate () {
            {
                barrier.SignalAndWait();
                Debug.Log("Thread[12] executing at: " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
              //  SetCustomeUserDetail();
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

    public void CustomEvents()
    {

        SceneManager.LoadScene(sceneBuildIndex: 1);

    }

    public void CrashReporting()
    {
        SceneManager.LoadScene(sceneBuildIndex: 2);

    }

    public void UserDetails()
    {
        SceneManager.LoadScene(sceneBuildIndex: 3);

    }

    public void ViewTracking()
    {
        SceneManager.LoadScene(sceneBuildIndex: 4);

    }

    public void DeviceId()
    {
        SceneManager.LoadScene(sceneBuildIndex: 5);

    }

    public void SetLocation()
    {
        string countryCode = "us";
        string city = "Böston’ 墨尔本";
        string latitude = "29.634933";
        string longitude = "-95.220255";
        string ipAddress = null;

        countly.Location.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

    }

    public void DisableLocation()
    {
        countly.Location.DisableLocation();
    }

    public async void SetRating()
    {
        await countly.StarRating.ReportStarRatingAsync("unity", "0.1", 3);
    }

    public async void RemoteConfigAsync()
    {
        await countly.RemoteConfigs.Update();

        Dictionary<string, object> config = countly.RemoteConfigs.Configs;
        Debug.Log("RemoteConfig: " + config?.ToString());
    }

    public void OnNotificationReceived(string message)
    {
        Debug.Log("[Example] OnNotificationReceived: " + message);
    }

    public void OnNotificationClicked(string message, int index)
    {
        Debug.Log("[Example] OnNotificationClicked: " + message + ", index: " + index);
    }
}
