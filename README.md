##What's Countly?

Countly is an innovative, real-time, open source mobile analytics application. 
It collects data from mobile devices, and visualizes this information to analyze 
mobile application usage and end-user behavior. There are two parts of Countly: 
the server that collects and analyzes data, and mobile SDK that sends this data. 
Both parts are open source with different licensing terms.

Countly Mobile Analytics - Unity SDK is developed by [Panteon Technologies](http://panteon.com.tr/).

##Installing Unity SDK

1. Add Countly prefab to your scene.
2. Click Countly game object on Hierarchy. 
3. Set following variables from inspector in Countly script:
- Server URL: Set your server URL. Enter https://cloud.count.ly for Countly Cloud, or your servername if you are going to have your own server.
- App Key: Your Application Key. You can get it from Countly web site after login. `Management -> Application -> Your Application -> Application Key`
- App Version: Your application version.

##Optional features & settings

* **Debug Mode:** Allows you to see what is going on. You can activate it by checking `Is Debug Mode On` variable.
* **Manual Mode:** If you want to control when user session starts and ends you should remove check of `Auto Start` variable. You can call `Countly.Instance.OnStart()` and `Countly.Instance.OnStop()` whenever you want.
* **Data Check Period:** Countly checks that is there any data waiting to send server and sends them with this period. 
* **KeepAliveSendPeriod:** Countly sends kind of Keep Alive messages to server to calculate session lengths. You can change the period by changing this value. (Default: 30)
* **Sleep After Failed Try:** Countly waits some time after failed server connection. For each failed try it increases waiting time by multiplying this value with number of failed tries. You can change this duration by changing this value. (Default: 5)
* **Package Size For Events:** Countly sends multiple events at one time to decrease data transfer. You can change the package size by changing this value.

##Countly server & SDK repositories

Check Countly Server source code below, if you are going to install your own server.

- [Countly Server (countly-server)](https://github.com/Countly/countly-server)

There are also other Countly SDK repositories below.

- [Countly iOS SDK](https://github.com/Countly/countly-sdk-ios)
- [Countly Android SDK](https://github.com/Countly/countly-sdk-android)
- [Countly Windows Phone SDK](https://github.com/Countly/countly-sdk-windows-phone)
- [Countly Blackberry Webworks SDK](https://github.com/Countly/countly-sdk-blackberry-webworks)
- [Countly Mac OS X SDK](https://github.com/mrballoon/countly-sdk-osx) (Community supported)
- [Countly Appcelerator Titanium SDK](https://github.com/euforic/Titanium-Count.ly) (Community supported)

##How can I help you with your efforts?
Glad you asked. We need ideas, feedbacks and constructive comments. All your suggestions will be taken care with upmost importance. 

We are on [Twitter](http://twitter.com/gocountly) and [Facebook](http://www.facebook.com/Countly) if you would like to keep up with our fast progress!

For community support page, see [http://support.count.ly](http://support.count.ly "Countly Support").
