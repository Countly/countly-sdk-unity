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
        readonly ISafeIDGenerator idGenerator = new CustomIdProvider();

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
            viewService.safeViewIDGenerator = idGenerator;

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
            Assert.AreEqual(viewEventAStart.EventID, "idv1");
            Assert.AreEqual(viewEventAStart.PreviousViewID, "");
            Assert.AreEqual(viewEventAStart.Segmentation["visit"], 1);
            Assert.AreEqual(viewEventAStart.Segmentation["start"], 1);

            viewService.StartAutoStoppedView(viewB);
            CountlyEventModel viewEventAEnd = viewService._eventService._eventRepo.Dequeue();

            Assert.AreEqual(viewEventAEnd.EventID, "idv1");
            Assert.AreEqual(viewEventAEnd.PreviousViewID, "");
            Assert.AreEqual(viewEventAEnd.Duration, 1);

            Assert.IsFalse(viewEventAEnd.Segmentation.ContainsKey("visit"));
            Assert.IsFalse(viewEventAEnd.Segmentation.ContainsKey("start"));

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            Assert.AreEqual(viewEventBStart.EventID, "idv2");
            Assert.AreEqual(viewEventBStart.PreviousViewID, "idv1");
            Assert.AreEqual(viewEventBStart.Segmentation["visit"], 1);

            Thread.Sleep(1000);

            // eE_B d=1 id=idv2 pvid=idv1, segm={}, sE_C id=idv3 pvid=idv2 segm={visit="1"}
            viewService.StartAutoStoppedView(viewC);
            CountlyEventModel viewEventBEnd = viewService._eventService._eventRepo.Dequeue();

            Assert.AreEqual(viewEventBEnd.EventID, "idv2");
            Assert.AreEqual(viewEventBEnd.PreviousViewID, "idv1");
            Assert.AreEqual(viewEventBEnd.Duration, 1);
            Assert.IsFalse(viewEventBEnd.Segmentation.ContainsKey("visit"));
            Assert.IsFalse(viewEventBEnd.Segmentation.ContainsKey("start"));

            CountlyEventModel viewEventCStart = viewService._eventService._eventRepo.Dequeue();
            Assert.AreEqual(viewEventCStart.EventID, "idv3");
            Assert.AreEqual(viewEventCStart.PreviousViewID, "idv2");
            Assert.AreEqual(viewEventCStart.Segmentation["visit"], 1);

            //eE_X d = 0 id = idv3 pvid = idv2, segm ={ }
            viewService.StopAllViews(null);
            CountlyEventModel viewEventCEnd = viewService._eventService._eventRepo.Dequeue();
            Assert.AreEqual(viewEventCEnd.EventID, "idv3");
            Assert.AreEqual(viewEventCEnd.Duration, 0);
            Assert.AreEqual(viewEventCEnd.PreviousViewID, "idv2");
            Assert.IsFalse(viewEventCEnd.Segmentation.ContainsKey("visit"));
            Assert.IsFalse(viewEventCEnd.Segmentation.ContainsKey("start"));

            // validating that no other event is recorded
            Assert.AreEqual(0, viewService._eventService._eventRepo.Count);
        }

        [Test]
        public void MV_201B_autoStopped_pausedResumed()
        {
            viewService.StartView("viewA");
            string viewIdB = viewService.StartAutoStoppedView("viewB");

            Thread.Sleep(1000);

            viewService.PauseViewWithID(viewIdB);

            Thread.Sleep(1000);

            viewService.ResumeViewWithID(viewIdB);
            viewService.StopAllViews(null);

            Assert.AreEqual(5, viewService._eventService._eventRepo.Count);
        }

        [Test]
        public void MV_202B_autoStopped_stopped()
        {
            string viewA = "viewA";
            string viewB = "viewB";

            viewService.StartView(viewA);
            Thread.Sleep(1000);
            viewService.StopViewWithName(viewA);
            string viewIdB = viewService.StartAutoStoppedView(viewB);
            Thread.Sleep(1000);
            viewService.StopViewWithID(viewIdB);
            viewService.StartAutoStoppedView("viewC");
            Thread.Sleep(1000);
            viewService.StopAllViews(null);

            Assert.AreEqual(6, viewService._eventService._eventRepo.Count);
        }

        [Test]
        public void MV_203_startView_PausedResumed()
        {
            string viewIdA = viewService.StartView("viewA");
            Thread.Sleep(1000);
            viewService.PauseViewWithID(viewIdA);
            Thread.Sleep(1000);
            viewService.ResumeViewWithID(viewIdA);
            Thread.Sleep(1000);
            viewService.StopAllViews(null);

            Assert.AreEqual(3, viewService._eventService._eventRepo.Count);
        }

        [Test]
        public void MV_203_startView_stopped()
        {
            string viewA = "viewA";
            viewService.StartView(viewA);
            Thread.Sleep(1000);
            viewService.StopViewWithName(viewA);
            string viewIdB = viewService.StartView("viewB");
            viewService.StopViewWithID(viewIdB);
            viewService.StartView("viewC");
            Thread.Sleep(1000);
            viewService.StopAllViews(null);

            Assert.AreEqual(6, viewService._eventService._eventRepo.Count);
        }

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