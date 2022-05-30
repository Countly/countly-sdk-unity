# Countly Unity SDK

We're hiring: Countly is looking for full stack devs, devops and growth hackers (remote work). [Click this link for job description](https://angel.co/countly/jobs/)

* **Slack user?** [Join our Slack community](http://slack.count.ly)
* **Questions?** [Ask in our Community forum](http://community.count.ly)

## What's Countly?
[Countly](http://count.ly) is an innovative, real-time, open source mobile analytics application. It collects data from mobile devices, and visualizes this information to analyze mobile application usage and end-user behavior. There are two parts of Countly: the server that collects and analyzes data, and mobile SDK that sends this data. Both parts are open source with different licensing terms.

## How to set up the project
1. Fill Countly prefab with ServerUrl and AppKey. Also you can set up other countly parameters.

<img src="https://api.monosnap.com/file/download?id=Un5qt0s49orTp3qA0zPEJ6FVyxnNdN" width="30%" height="30%">

2. Change notification icons if necessary. All icons are placed in folder /Plugins/Android/Notifications/res. You can find more information in official Android documentation.
3. Register in Firebase and create google-services.xml from google-services.json. You can use online converter [here](https://dandar3.github.io/android/google-services-json-to-xml.html). Put your file google-services.xml in /Plugins/Android/Notifications/res/values (replace if necessary).
4. Put your applicationId in mainTemplate.gradle. Read more about mainTemplate [here](https://docs.unity3d.com/Manual/android-gradle-overview.html)
5. Set **Write Permission** to 'External (SDCard)'. Sometimes it is required for iBoxDb.

<img src="https://api.monosnap.com/file/download?id=Y1S7nuBAvZrdc0po5BROBzFoaxkRoY" width="30%" height="30%">

## How to use this SDK
In order to start using countly you need to attach script CountlyEntryPoint to any gameobject on your scene and assign Countly and CountlyWrapper prefabs to proper fields. All prefabs are stored in Prefabs folder.
CountlyWrapper is used only in Editor mode, it emulates and logs all events.
Countly is used in production. It contains all settings. Just do not forget put ServerUrl and AppKey into Countly prefab :)
Example scene: EntryPoint in folder Scenes.

The core class of an entire SDK is Countly.cs. You can use any handler from that class.

List of handlers:
1. Consents
2. CrushReports
3. Device
4. Events - use this handler to publish any custom events.
5. Initialization
6. OptionalParameters
7. RemoteConfigs - use this handler to retrieve remote configs.
8. StarRating
9. UserDetails - use this handler to publish any user parameters including custom attributes.
10. Views - use this handler to publish any custom events. 

Examples:
```csharp
//Record event
await Plugins.Countly.Impl.Countly.Instance.Events.RecordEventAsync("Test event");

//Record event and add segment 'useNumberInSameSession'
await Plugins.Countly.Impl.Countly.Instance.Events.RecordEventAsync("Test event", true);

//Report view event
await Plugins.Countly.Impl.Countly.Instance.Views.RecordOpenViewAsync("Menu");
await Plugins.Countly.Impl.Countly.Instance.Views.RecordCloseViewAsync("Menu");

//Init remote configs
await Plugins.Countly.Impl.Countly.Instance.RemoteConfigs.InitConfig(); //you should wait a bit after calling this method till configs is loaded.

//Get configs
var configs = Plugins.Countly.Impl.Countly.Instance.Configs;

```

## iOs. Messages (Push notifications)
iOs has native support for remote notifications.
In Unity we removed deprecated UnityEngine.Ios.NotificationServices and replaced it with Unity Mobile Notifications Package, 
documentation [here](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@1.0/manual/index.html).
This prevent compilation errors during a build process in Xcode.

<img src="https://api.monosnap.com/file/download?id=mHci5I31WCXHkM1CloF3ykCSP8Di0T" width="70%" height="70%">
Also notification settings can be configured via file NotificationSettings in Assets/Editor/com.unity.mobile.notifications.
**IMPORTANT!!** When Unity recompiles code a checkbox 'Enable Push notifications' get disabled. You need to enable it manually before making a build. 

In Countly prefab switch Notification Mode to anything but none None.
<img src="https://api.monosnap.com/file/download?id=udE3qa26avJJiVMr6TFHYmiBU8y18N" width="70%" height="70%">

## ANDROID. Messages (Push notifications)
On android all notifications are called messages.
Countly works with FCS to send messages.
There are two types of messages:
* Notification messages, sometimes thought of as "display messages." These are handled by the FCM SDK automatically.
* Data messages, which are handled by the client app.

#### ANDROID. Push notification messages from countly

Countly sends **ONLY** data messages. 

<img src="https://api.monosnap.com/file/download?id=kzLK5q7A6K6mKYyF6AKDqtVxIeT0QC" width="70%" height="70%">


There are two ways to send data messages via countly:

<img src="https://api.monosnap.com/file/download?id=1bxCfiYlfzD87D6hEJTbYPWqkmWVYh" width="70%" height="70%">

Json received on device:
```json
{
  "message": {
    "token": "bk3RNwTe3H0:CI2k_HHwgIpoDKCIZvvDMExUdFQ3P1...",
    "data": {
      "c.i": "5cee37ba7fdb4c235667a1fe",
      "c.l": "http://google.com",
      "test": "data",
      "badge": "123456",
      "sound": "default",
      "title": "Welcome",
      "message": "HelloWorld"
    }
  }
}
```

<img src="https://api.monosnap.com/file/download?id=crwFY13K6AZnsyO4txIFiOCay02cdQ" width="70%" height="70%">

Json received on device:

```json
{
  "message": {
    "token": "bk3RNwTe3H0:CI2k_HHwgIpoDKCIZvvDMExUdFQ3P1...",
    "data": {
      "c.i": "5cee3826cdbe68192b25260e",
      "c.l": "http://google.com",
      "c.s": "true",
      "test": "data",
      "badge": "123456",
      "sound": "default",
      "title": "Welcome",
      "message": "HelloWorld"
    }
  }
}
```

## About

Need help? See [Countly SDK for Unity](http://resources.count.ly/v1.0/docs/) documentation at [Countly Resources](http://resources.count.ly), or ask us on our [Countly Analytics Community Slack channel](http://slack.count.ly).

## Security

Security is very important to us. If you discover any issue regarding security, please disclose the information responsibly by sending an email to security@count.ly and **not by creating a GitHub issue**.

## Other Github resources

This SDK needs one of the following Countly Editions to work: 

* Countly Community Edition, [downloadable from Github](https://github.com/Countly/countly-server)
* [Countly Enterprise Edition](http://count.ly/product)

For more information about Countly Enterprise Edition, see [comparison of different Countly editions](https://count.ly/compare/)

There are also other Countly SDK repositories (both official and community supported) on [Countly resources](http://resources.count.ly/v1.0/docs/downloading-sdks).

## How can I help you with your efforts?
Glad you asked. We need ideas, feedback and constructive comments. All your suggestions will be taken care with utmost importance. We are on [Twitter](http://twitter.com/gocountly) and [Facebook](http://www.facebook.com/Countly) if you would like to keep up with our fast progress!

## Badges

If you like Countly, [why not use one of our badges](https://count.ly/brand-assets) and give a link back to us, so others know about this wonderful platform? 

<a href="https://count.ly/f/badge" rel="nofollow"><img style="width:145px;height:60px" src="https://count.ly/badges/dark.svg?v2" alt="Countly - Product Analytics" /></a>

    <a href="https://count.ly/f/badge" rel="nofollow"><img style="width:145px;height:60px" src="https://count.ly/badges/dark.svg" alt="Countly - Product Analytics" /></a>

<a href="https://count.ly/f/badge" rel="nofollow"><img style="width:145px;height:60px" src="https://count.ly/badges/light.svg?v2" alt="Countly - Product Analytics" /></a>

    <a href="https://count.ly/f/badge" rel="nofollow"><img style="width:145px;height:60px" src="https://count.ly/badges/light.svg" alt="Countly - Product Analytics" /></a>

### Support

For Community support, visit [http://community.count.ly](http://community.count.ly "Countly Community Forum").
