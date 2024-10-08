## 24.8.0
* Added support for string key and Array/List value to all user given segmentations.
* Added "SetID(string newDeviceId)" function in "Countly.Instance.Device" for managing device id changes.
* Added "Countly.Instance.UserProfile" with following calls and functionality
  * "Increment" for incrementing custom property value by 1
  * "IncrementBy" for incrementing custom property value by provided value.
  * "SaveMax" for saving maximal value between existing and provided.
  * "SaveMin" for saving minimal value between existing and provided.
  * "Multiply" for multiplying custom property value by provided value.
  * "Pull" for removing value from array.
  * "Push" for inserting value to array which can have duplicates.
  * "PushUnique" for inserting value to array of unique values.
  * "Save" for sending provided values to server.
  * "SetOnce" for setting value if it does not exist.
  * "SetProperties" for setting either custom user properties or predefined user properties.
  * "SetProperty" for setting a single user property. It can be either a custom one or one of the predefined ones.
* Added the following calls in "Countly.Instance.Views":
  * "StartView" for starting a view.
  * "StartAutoStoppedView" for starting an auto-stopped view.
  * "StopViewWithName" for stopping a view by name.
  * "StopViewWithID" for stopping a view by ID.
  * "PauseViewWithID" for pausing a view by ID.
  * "ResumeViewWithID" for resuming a paused view by ID.
  * "StopAllViews" for stopping all views.
  * "SetGlobalViewSegmentation" for setting global view segmentation data.
  * "AddSegmentationToViewWithID" for adding segmentation data to a view by ID.
  * "AddSegmentationToViewWithName" for adding segmentation data to a view by name.
  * "UpdateGlobalViewSegmentation" for updating global view segmentation data.
* Deprecated the following calls from "Countly.Instance.Views":
  * "RecordOpenViewAsync": Use "StartView" instead.
  * "RecordCloseViewAsync": Use "StopView" instead.
  * "ReportActionAsync": This will be removed in the future.
* Deprecated "Countly.Instance.UserDetails" added "Countly.Instance.UserProfile" as replacement, mentioned above. 
* Fixed a bug that allowed to make it possible to close non-started views.
* Fixed issues that allowed to record User Profiles without consent.
* Fixed a bug that caused requests being stuck in the queue for WebGL build targets.

## 23.12.1
* Added 'UnityWebRequest' as the networking handler for WebGL builds.
* Gathered SDK content under the 'Countly' folder for better structure and to avoid mixing with other files.

## 23.12.0
* Added functionality to allow passing custom or overridden metrics.
* Deprecated CountlyConfiguration constructor. A replacement constructor with parameters added.
* Deprecated "ServerUrl" string in "CountlyConfiguration" class.
* Deprecated "AppKey" string in "CountlyConfiguration" class.
* Deprecated "DeviceId" string in "CountlyConfiguration" class. A setter method is added.
* Deprecated "Salt" string in "CountlyConfiguration" class. A setter method is added.
* Deprecated "RequiresConsent" bool in CountlyConfiguration class. A setter method is added.
* Deprecated "EnablePost" bool in "CountlyConfiguration" class. A setter method is added.
* Deprecated "EnableTestMode" bool in "CountlyConfiguration" class.
* Deprecated "EnableConsoleLogging" bool in "CountlyConfiguration" class. A setter method is added.
* Deprecated "EnableManualSessionHandling" bool in "CountlyConfiguration" class.
* Deprecated "EnableAutomaticCrashReporting" bool in "CountlyConfiguration" class. A setter method is added.
* Deprecated "SessionDuration" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "MaxKeyLength" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "MaxValueSize" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "MaxSegmentationValues" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "MaxStackTraceLinesPerThread" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "MaxStackTraceLineLength" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "EventQueueThreshold" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "StoredRequestLimit" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "TotalBreadcrumbsAllowed" integer in "CountlyConfiguration" class. A setter method is added.
* Deprecated "NotificationMode" enum in "CountlyConfiguration" class. A setter method is added.

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
