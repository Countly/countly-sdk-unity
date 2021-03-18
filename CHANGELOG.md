## 20.11.1
* Added Consent feature
* Added data type checking for segmentation
* Reworked init, added a new way to init the SDK from code. That will be the mandatoy method going further.

* Fixed issues with location handling and reworked some aspects of it's behaviour
* Fixed potential issues by calculating Timestamp, dow, hours and tz from the same time moment
* Fixed crashes that could happen on Application quit
* "EnableFirstAppLaunchSegment" in SDK configuration has been deprecated and it's functinality has been removed. This variable is going to be removed in the future.
* "LogCallback" in "CrashReportsCountlyService" is deprecated, this is going to be removed in the future.
* Changed the default Device ID generation mechanism for ios devices. Now it will use 'SystemInfo.deviceUniqueIdentifier' as it's source instead of 'iOS.Device.advertisingIdentifier'

## 20.11.0
* Added Sample App
* Added test mode feature 
* Added logging flag which now will prevent any console logs to be made
* Added push notification "clicked" and "received" callbacks

* Reworked push notifications. Added support for push buttons and actioned events
* Reworked push notification native plugins

* Updated Remote Config
* Tweaked how location recording should be done
* Changed SDK namespaces and sdk structure

* Removed Wrapper classes and unnecessary interfaces
* Removed Unity Mobile Notifications Plugin
* Renamed Crash Reporting interface 

* Fixed undesired request queue behaviour
