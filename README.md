## What's Countly?
[Countly](http://count.ly) is an innovative, real-time, open source mobile analytics application. It collects data from mobile devices, and visualizes this information to analyze mobile application usage and end-user behavior. There are two parts of Countly: the server that collects and analyzes data, and mobile SDK that sends this data. Both parts are open source with different licensing terms.

# Playdarium countly Unity SDK
Original Countly Unity3d SDK does not have sufficient code. Also it incliudes some deprecated rest api methods.
This version of coutly SDK contains refactoring, some improtant additional features and fixes.
Feature list (probably, I forget something:) ):
1. Local storage. Extremely important feature especially on mobile platforms because users play offline very often. Every event and request is stored locally before then are sent to Countly. We use [iBoxDb](http://iboxdb.com) as local database.
2. Firebase plugins were replace entirely with custom plugin. This allows us to reduce build size dramatically on mobile devices (up to 10 mb reducing). You can find plugin source code in Notifications folder.
3. Add Locale parameter (_locale) to CountlyMetricModel so we can track users locale via Language panel in Countly.
4. All view events ([CLY]_view) are sent in one request so only one data point is tracked by Countly. This works only for view events. Vies are sent separately from other events.
5. Refactoring: Split Countly.cs into separate services.
6. Add Wrapper for Editor. By default, countly events does not send to the server in editor
7. Fix event time. In original version events store time when request is sent. But the point is, we send request *after* event is occured (we send many events in one request). So, we changed it, and now event is stored the time when it occurs.
8. Now all events are sent to Countly when OnApplicationQuit, OnApplicationFocus(focus=false), OnApplicationPause(pause=true) occur.
9. RemoteConfigCountlyService is added. It allows to retrieve all Remote Configs from countly in one request. All retrieved configs are stored in local database. If due to some reasons impossible to retrieve Configs then the service loads configs from local database.
 
# Roadmap
1. Handle local notifications.
2. Improve code.
3. Bugfixes (self-hosted countly is requered).
 
## Messages (Notifications)
On android all notifications are called messages.
Countly works with FCS to send messages.
There are two types of messages:
* Notification messages, sometimes thought of as "display messages." These are handled by the FCM SDK automatically.
* Data messages, which are handled by the client app.

#### Notification messages from countly

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

<img src="https://api.monosnap.com/file/download?id=crwFY13K6AZnsyO4txIFiOCay02cdQ" width="50%" height="50%">

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

This repository includes the Unity SDK. 

Need help? See [Countly SDK for Unity](http://resources.count.ly/v1.0/docs/) documentation at [Countly Resources](http://resources.count.ly), or ask us on our [Countly Analytics Community Slack channel](http://slack.count.ly) or [Playdarium email](playdarium@gmail.com).

## Security

Security is very important to us. If you discover any issue regarding security, please disclose the information responsibly by sending an email to security@count.ly and **not by creating a GitHub issue**.

## Other Github resources

This SDK needs one of the following Countly Editions to work: 

* Countly Community Edition, [downloadable from Github](https://github.com/Countly/countly-server)
* [Countly Enterprise Edition](http://count.ly/product)
* [Official COuntky Unity SDK](https://github.com/Countly/countly-sdk-unity)

For more information about Countly Enterprise Edition, see [comparison of different Countly editions](https://count.ly/compare/)

There are also other Countly SDK repositories (both official and community supported) on [Countly resources](http://resources.count.ly/v1.0/docs/downloading-sdks).

## How can I help you with your efforts?
Glad you asked. We need ideas, feedback and constructive comments. All your suggestions will be taken care with utmost importance. We are on [Twitter](http://twitter.com/gocountly) and [Facebook](http://www.facebook.com/Countly) if you would like to keep up with our fast progress!


### Support

For Community support, visit [http://community.count.ly](http://community.count.ly "Countly Community Forum").
