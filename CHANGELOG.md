## XX.X.X
* Added MetricHelper class and "overriddenMetrics" field in the configuration object, allowing to pass custom or overridden metrics.

## 23.06.1
* Added app version metric to every request sent.
* Fixed a bug that caused build issues when not running inside the editor.

## 23.06.0
* Deprecated "CrushReports" and "OptionalParameters" getters, in "Countly" class are removed.
* Deprecated "EnableFirstAppLaunchSegment" and "IgnoreSessionCooldown" bools are removed from the "CountlyConfiguration" class.
* Deprecated "LogCallback" method in "CrashReportsCountlyService" class is removed.
* Deprecated "ChangeDeviceIdAndEndCurrentSessionAsync" and "ChangeDeviceIdAndMergeSessionDataAsync" methods, in "DeviceIdCountlyService" class are removed.
* Deprecated "ReportCustomEventAsync" method, in "EventCountlyService" class is removed.
* Deprecated "SetCustomUserDetailsAsync" method, in "UserDetailsCountlyService" class is removed.
* Deprecated "RecordOpenViewAsync" and "RecordCloseViewAsync"in "ViewCountlyService" class are removed
* Deprecated "SetCustomeUserDetail" method in "UserDetails" class is removed.
  
* "SendCrashReportAsync" method in "CrashReportsCountlyService" class is deprecated. A replacement method with different parameter list was added.

## 22.06.1
* ! Minor breaking change ! The unhandled crash handler will no longer report messages with the LogType "errors". It will report only messages of the type "exceptions".
* Default max segmentation value count changed from 30 to 100

## 22.06.0
* Adding device ID type information to all requests.

## 22.02.0
* Added device ID type.
* Added calls to record Timed Events
* Add new 'segmentation' parameter in 'RecordOpenViewAsync' method to record view with segmentation.
* When changing consent, the SDK will now send the full state of the consent and not just the delta.
* Added platform information to push actioned events.

## 21.11.0
* !! Major breaking change !! Changing device ID without merging will now clear the current consent. Consent has to be given again after performing this action.
* 'UserCustomDetailAsync (CountlyUserDetailsModel userDetailsModel)' is deprecated.
* Fixed issues with how parameter tampering protection was handled.
* Device id can be changed without giving any consent.

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
