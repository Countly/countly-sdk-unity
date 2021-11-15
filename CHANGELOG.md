## 21.10.0
* !! Major breaking change !! On successfully changing device id without merge, SDK removes all given consents.
* ! Minor breaking change ! 'UserCustomDetailAsync (CountlyUserDetailsModel userDetailsModel)' is deprecated, this is going to be removed in the future.
* Fixed parameters tampering protection issue while calculating checksum.
## 20.11.5
* Added new configuration fields to manipulate internal SDK value and key limits.
* Warning! This release will introduce configurable maximum size limits for values and keys throughout the SDK. If they would exceed the limits, they would be truncated.
* Fixed a bug that caused requests to arrive out of their intended order

## 20.11.4
* Added "lock" checks for all public SDK methods.
* Fixed race issue bug that occured when events are combined into a request.

## 20.11.3
* Fixed potential timing issues in situations where events are combined into a request.

## 20.11.2
* Added Consent feature
* Added data type checking for segmentation
* Added functionality to disable automatic session tracking

* Reworked automatic session handling and fixed session timer issue.
* Reworked init, added a new way to init the SDK from code. That will be the mandatory method going further.

* Fixed potential issues with the internal event queue
* Fixed potential issues in the request queue and reworked its behavior
* Fixed issues with location handling and reworked some aspects of its behavior
* Fixed potential issues by calculating Timestamp, dow, hours, and tz from the same time moment
* Fixed crashes that could happen on Application quit

* "EnableFirstAppLaunchSegment" in SDK configuration has been deprecated and its functionality has been removed. This variable is going to be removed in the future.
* "LogCallback" in "CrashReportsCountlyService" is deprecated, this is going to be removed in the future.
* Changed the default Device ID generation mechanism for ios devices. Now it will use 'SystemInfo.deviceUniqueIdentifier' as it's source instead of 'iOS.Device.advertisingIdentifier'
* "ChangeDeviceIdAndEndCurrentSessionAsync" and "ChangeDeviceIdAndMergeSessionDataAsync" in the SDK Device module have been deprecated and this is going to be removed in the future.
* "ReportCustomEventAsync" in the SDK Event module has been deprecated and this is going to be removed in the future.

## 20.11.1
* Fixed ID generation issue for iOS devices. Now SDK using 'SystemInfo.deviceUniqueIdentifier' instead 'Device.advertisingIdentifier'

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

* Fixed undesired request queue behavior
