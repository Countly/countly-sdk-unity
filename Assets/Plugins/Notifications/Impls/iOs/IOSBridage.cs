using UnityEngine;
using System;
using Plugins.Countly.Models;
using Newtonsoft.Json.Linq;

public class IOSBridage : MonoBehaviour
{
    public string MessageId { get; set; }
    public string ButtonIndex { get; set; }

    public CountlyConfigModel Config { get; set; }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void registerForRemoteNotifications();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void setListenerGameObject(string listenerName);

    private Action<string> _onTokenResult;
    private Action<string> _OnNotificationReceiveResult;
    private Action<string, int> _OnNotificationClickResult;

    public void ListenTokenResult(Action<string> result) => _onTokenResult = result;
    public void ListenReceiveResult(Action<string> result) => _OnNotificationReceiveResult = result;
    public void ListenClickResult(Action<string, int> result) => _OnNotificationClickResult = result;

    // Use this for initialization
    void Start()
    {
        setListenerGameObject(this.gameObject.name);
        
    }

    public void GetToken()
    {
        registerForRemoteNotifications();
    }


    //Sent when the application successfully registered with Apple Push Notification Service (APNS).
    void OnDidRegisterForRemoteNotificationsWithDeviceToken(string deviceToken)
    {
        if (deviceToken != null && deviceToken.Length != 0)
        {
            if (Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] OnDidRegisterForRemoteNotificationsWithDeviceToken Token: " + deviceToken);
            }
            
            _onTokenResult?.Invoke(deviceToken);
        }
    }

    //Sent when the application failed to be registered with Apple Push Notification Service (APNS).
    void OnDidFailToRegisterForRemoteNotificcallBackationsWithError(string error)
    {
        if (Config.EnableConsoleErrorLogging)
        {
            Debug.Log("[Countly] OnDidFailToRegisterForRemoteNotificcallBackationsWithError error: " + error);
        }
    }

    
    void OnPushNotificationsReceived(string pushData)
    {
        if (Config.EnableConsoleErrorLogging)
        {
            Debug.Log("[Countly] OnPushNotificationsReceived: " + pushData);
        }
        _OnNotificationReceiveResult?.Invoke(pushData);

    }

    void OnPushNotificationsClicked(string pushData)
    {
        if (Config.EnableConsoleErrorLogging)
        {
            Debug.Log("[Countly] OnPushNotificationsClicked: " + pushData);
        }

        JObject item = JObject.Parse(pushData);
         
        MessageId = item.GetValue("i").ToString();
        ButtonIndex = item.GetValue("action_index").ToString();

        _OnNotificationClickResult?.Invoke(pushData, int.Parse(ButtonIndex));

    }


    void OnDidRegisterUserNotificationSettings(string setting)
    {
        if (Config.EnableConsoleErrorLogging)
        {
            Debug.Log("[Countly] OnDidRegisterUserNotificationSettings error: " + setting);
        }
    }

}
