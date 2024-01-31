using System.Threading;
using Notifications;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;

public class TestingEntryPoint : MonoBehaviour, INotificationListener
{
    Thread mainThread;

    private Countly countly;
    private string _appKey = "YOUR_APP_KEY";
    private string _serverUrl = "https://try.count.ly/";

    private void Awake()
    {
        mainThread = Thread.CurrentThread;
        countly = Countly.Instance;
        InitOnAnotherThread();
    }

    private void InitOnAnotherThread()
    {
        if (IsMainThread()) {
            Debug.LogWarning("This is main thread you may proceed");
        }

        int participants = 2;
        Thread[] threads = new Thread[participants];
        threads[0] = new Thread(delegate () {
            if (countly.IsSDKInitialized || !IsMainThread()) {
                Debug.Log("Not on main thread");
            }

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

        threads[1] = new Thread(delegate () {
            Debug.Log("This is another thread");
        });

        for (int i = 0; i < participants; i++) {
            threads[i].Start();
        }

        for (int i = 0; i < participants; i++) {
            threads[i].Join();
        }
    }

    bool IsMainThread()
    {
        if(Thread.CurrentThread.ManagedThreadId == mainThread.ManagedThreadId) {
            return true;
        } else {
            return false;
        }
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
