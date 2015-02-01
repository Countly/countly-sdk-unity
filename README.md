

**WARNING:** This SDK is outdated. Please use [Mário Freitas's Unity SDK](https://github.com/imkira/unity-countly).


##Installing Unity SDK

You need Unity3D 4.0. It won't work on Unity3D 3.5 and below without modification.

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

