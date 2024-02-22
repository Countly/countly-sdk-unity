using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;

namespace Assets.Tests.PlayModeTests.Scenarios
{
    public class CustomIdProvider : ISafeIDGenerator
    {
        int viewCount;

        public string GenerateValue()
        {
            viewCount++;
            string customValue = "idv" + viewCount.ToString();

            return customValue;
        }
    }

    public class MV_ManualView
    {
        ViewCountlyService viewService;

        readonly Action<ViewCountlyService, string, Dictionary<string, object>>[] allMethods = new Action<ViewCountlyService, string, Dictionary<string, object>>[]
         {
            (service, arg1, _) => service.RecordOpenViewAsync(arg1), // 0
            (service, arg1, arg2) => service.RecordOpenViewAsync(arg1, arg2), // 1
            (service, arg1, _) => service.RecordCloseViewAsync(arg1), // 2
            (service, arg1, _) => service.StartView(arg1), // 3
            (service, arg1, arg2) => service.StartView(arg1, arg2), // 4
            (service, arg1, _) => service.StartAutoStoppedView(arg1), // 5
            (service, arg1, arg2) => service.StartAutoStoppedView(arg1, arg2), // 6
            (service, arg1, _) => service.StopViewWithName(arg1), // 7
            (service, arg1, arg2) => service.StopViewWithName(arg1, arg2), // 8
            (service, arg1, _) => service.StopViewWithID(arg1), // 9
            (service, arg1, arg2) => service.StopViewWithID(arg1, arg2), // 10
            (service, arg1, _) => service.PauseViewWithID(arg1), // 11
            (service, arg1, _) => service.ResumeViewWithID(arg1), // 12
            (service, _, arg2) => service.StopAllViews(arg2), // 13
            (service, _, arg2) => service.SetGlobalViewSegmentation(arg2), // 14
            (service, arg1, arg2) => service.AddSegmentationToViewWithID(arg1, arg2), // 15
            (service, arg1, arg2) => service.AddSegmentationToViewWithName(arg1, arg2), // 16
            (service, _, arg2) => service.UpdateGlobalViewSegmentation(arg2), // 17
         };

        private void AssertViewEventProperties(CountlyEventModel viewEvent, string expectedEventID, string expectedPreviousViewID, int? expectedDuration, int? expectedVisit, bool expectStart)
        {
            Assert.AreEqual(viewEvent.EventID, expectedEventID);
            Assert.AreEqual(viewEvent.PreviousViewID, expectedPreviousViewID);

            if (expectedDuration.HasValue) {
                Assert.AreEqual(viewEvent.Duration, expectedDuration.Value);
            }

            if (expectStart) {
                if (expectedVisit.HasValue) {
                    Assert.AreEqual(viewEvent.Segmentation["visit"], expectedVisit.Value);
                }
            } else {
                Assert.IsFalse(viewEvent.Segmentation.ContainsKey("visit"));
                Assert.IsFalse(viewEvent.Segmentation.ContainsKey("start"));
            }
        }

        Dictionary<string, object> testSegmentation;

        // Initializes Countly and clears event repo and request repo
        // Validates that repositories are clean before testing
        [SetUp]
        public void ScenarioTestSetup()
        {
            TestUtility.TestCleanup();

            testSegmentation = TestUtility.TestSegmentation();
            CountlyConfiguration config = new CountlyConfiguration("appKey", "serverURl")
                .SetRequiresConsent(true);

            config.GiveConsent(new Consents[] { Consents.Events, Consents.Views });
            Countly.Instance.Init(config);

            Countly.Instance.RequestHelper._requestRepo.Clear();
            Countly.Instance.Views._eventService._eventRepo.Clear();

            viewService = Countly.Instance.Views;
            viewService.safeViewIDGenerator = new CustomIdProvider();

            Assert.IsTrue(Countly.Instance.RequestHelper._requestRepo.Count == 0);
            Assert.IsTrue(viewService._eventService._eventRepo.Count == 0);
        }

        // All public functions in ViewCountlyService
        // Null values provided as viewID/viewName parameter for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_100_badValues_null()
        {
            foreach (var func in allMethods) {
                func(viewService, null, null);
                func(viewService, null, testSegmentation);
                Assert.IsTrue(Countly.Instance.RequestHelper._requestRepo.Count == 0); // validate that RQ is empty
                Assert.IsTrue(Countly.Instance.Views._eventService._eventRepo.Count == 0); // validate that EQ is empty
            }
        }

        // 'RecordView', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Empty string values provided as viewID/viewName parameter for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_101_badValues_emptyString()
        {
            var testArray = allMethods.Take(allMethods.Length - 3).ToArray();

            foreach (var func in testArray) {
                func(viewService, "", null);
                func(viewService, "", testSegmentation);
                Assert.IsTrue(Countly.Instance.RequestHelper._requestRepo.Count == 0); // validate that RQ is empty
                Assert.IsTrue(Countly.Instance.Views._eventService._eventRepo.Count == 0); // validate that EQ is empty
            }
        }

        // 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Non-existing viewNames and viewIDs provided for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_102_badValues_nonExistingViews()
        {
            string viewName = "NonStartedView";
            string viewID = "nonExistingViewID";

            int[] testIndexes = { 7, 8, 9, 10, 11, 12, 15, 16 };
            var testArray = GetMethodsByIndexes(testIndexes);

            foreach(var func in testArray) {
                func(viewService, viewName, null);
                func(viewService, viewName, testSegmentation);
                func(viewService, viewID, null);
                func(viewService, viewID, testSegmentation);

                Assert.IsTrue(Countly.Instance.RequestHelper._requestRepo.Count == 0); // validate that RQ is empty
                Assert.IsTrue(Countly.Instance.Views._eventService._eventRepo.Count == 0); // validate that EQ is empty
            }
        }

        // 'StartAutoStoppedView' and 'StopAllViews' functions in ViewCountlyService
        // Starting 3 auto stopping views one after another, waiting 1 sec between and stopping all views in the end
        // Auto stopped views should stop when another view starts, and should record correctly
        [Test]
        public void MV_200B_autoStoppedView_autoClose()
        {
            string viewA = "viewA";
            string viewB = "viewB";
            string viewC = "viewC";

            viewService.StartAutoStoppedView(viewA);
            Thread.Sleep(1000);

            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStart, "idv1", "", null, 1, true);

            viewService.StartAutoStoppedView(viewB);
            CountlyEventModel viewEventAEnd = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAEnd, "idv1", "", 1, 0, false);

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStart, "idv2", "idv1", null, 1, true);

            Thread.Sleep(1000);

            viewService.StartAutoStoppedView(viewC);
            CountlyEventModel viewEventBEnd = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBEnd, "idv2", "idv1", 1, 0, false);

            CountlyEventModel viewEventCStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCStart, "idv3", "idv2", null, 1, true);

            viewService.StopAllViews(null);
            CountlyEventModel viewEventCEnd = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCEnd, "idv3", "idv2", 0, 0, false);

            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);
        }

        // 'StartView', 'StartAutoStoppedView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, pauses and resumes viewB and stops both afterwards
        // Both views should behave and record the events correctly
        [Test]
        public void MV_201B_autoStopped_pausedResumed()
        {
            viewService.StartView("viewA");
            string viewIdB = viewService.StartAutoStoppedView("viewB");

            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStart, "idv1", "", null, 1, true);

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStart, "idv2", "idv1", null, 1, true);

            Thread.Sleep(1000);

            viewService.PauseViewWithID(viewIdB);
            CountlyEventModel viewEventBPause = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBPause, "idv2", "idv1", 1, 0, false);

            Thread.Sleep(1000);

            viewService.ResumeViewWithID(viewIdB);

            viewService.StopAllViews(null);

            CountlyEventModel viewEventAStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStop, "idv1", "idv1", 2, null, false);

            CountlyEventModel viewEventBStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStop, "idv2", "idv1", 0, null, false);
        }

        // 'StartView', 'StartAutoStoppedView', 'StopViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, after stopping both starts a 3rd one
        // All views should behave and record the events correctly
        [Test]
        public void MV_202B_autoStopped_stopped()
        {
            string viewA = "viewA";
            string viewB = "viewB";

            viewService.StartView(viewA);
            Thread.Sleep(1000);

            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStart, "idv1", "", null, 1, true);

            viewService.StopViewWithName(viewA);

            CountlyEventModel viewEventAStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStop, "idv1", "", 1, null, false);

            string viewIdB = viewService.StartAutoStoppedView(viewB);
            Thread.Sleep(1000);

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStart, "idv2", "idv1", null, 1, true);

            viewService.StopViewWithID(viewIdB);

            CountlyEventModel viewEventBStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStop, "idv2", "idv1", 1, null, false);

            viewService.StartAutoStoppedView("viewC");
            Thread.Sleep(1000);

            CountlyEventModel viewEventCStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCStart, "idv3", "idv2", null, 1, true);

            viewService.StopAllViews(null);

            CountlyEventModel viewEventCStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCStop, "idv3", "idv2", 1, null, false);
        }

        // 'StartView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts a view with StartView, pauses it, resumes it and stops it after 1 second
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_PausedResumed()
        {
            string viewIdA = viewService.StartView("viewA");
            Thread.Sleep(1000);

            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStart, "idv1", "", null, 1, true);

            viewService.PauseViewWithID(viewIdA);
            Thread.Sleep(1000);

            CountlyEventModel viewEventAPause = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAPause, "idv1", "", 1, 0, false);

            viewService.ResumeViewWithID(viewIdA);
            Thread.Sleep(1000);

            viewService.StopAllViews(null);

            CountlyEventModel viewEventAStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStop, "idv1", "", 1, 0, false);
        }

        // 'StartView', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Starts a view with StartView, stops it with StopViewWithName, starts another view, and stops it with StopViewWithID
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_stopped()
        {
            string viewA = "viewA";

            viewService.StartView(viewA);
            Thread.Sleep(1000);

            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStart, "idv1", "", null, 1, true);

            viewService.StopViewWithName(viewA);

            CountlyEventModel viewEventAStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventAStop, "idv1", "", 1, null, false);

            string viewIdB = viewService.StartView("viewB");
            viewService.StopViewWithID(viewIdB);

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStart, "idv2", "idv1", null, 1, true);

            CountlyEventModel viewEventBStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventBStop, "idv2", "idv1", 0, null, false);

            viewService.StartView("viewC");
            Thread.Sleep(1000);

            CountlyEventModel viewEventCStart = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCStart, "idv3", "idv2", null, 1, true);

            viewService.StopAllViews(null);

            CountlyEventModel viewEventCStop = viewService._eventService._eventRepo.Dequeue();
            AssertViewEventProperties(viewEventCStop, "idv3", "idv2", 1, null, false);
        }

        // 'RecordOpenViewAsync', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Testing the behavior when calling these functions with no user consent to record view-related events
        // Expecting that no view events are recorded, as the functions are called without user consent
        [Test]
        public void MV_300A_callingWithNoConsent_legacy()
        {
            Countly.Instance.Consents.RemoveAllConsent();
            Assert.IsFalse(Countly.Instance.Consents.CheckConsent(Consents.Views));

            viewService.RecordOpenViewAsync("viewTest");
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.RecordOpenViewAsync("viewTest", testSegmentation);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StartAutoStoppedView("viewTest");
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StartAutoStoppedView("viewTest", testSegmentation);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StartView("viewTest");
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StartView("viewTest", testSegmentation);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            string viewId1 = viewService.StartView("viewTest");
            viewService.PauseViewWithID(viewId1);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.ResumeViewWithID(viewId1);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StopViewWithName("viewTest");
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            viewService.StopViewWithName("viewTest", testSegmentation);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            string viewId2 = viewService.StartView("viewTest");
            viewService.StopViewWithID(viewId2);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);

            string viewId3 = viewService.StartView("viewTest");
            viewService.StopViewWithID(viewId3, testSegmentation);
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }

        private Action<ViewCountlyService, string, Dictionary<string, object>>[] GetMethodsByIndexes(int[] includedIndexes)
        {
            return includedIndexes.Select(index => allMethods[index]).ToArray();
        }
    }
}