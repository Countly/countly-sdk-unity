using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;
using UnityEngine;

namespace Assets.Tests.PlayModeTests.Scenarios
{
    public class CustomIdProvider : ISafeIDGenerator
    {
        static int viewCount = 0;

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

        Action<ViewCountlyService, string, Dictionary<string, object>>[] allMethods = new Action<ViewCountlyService, string, Dictionary<string, object>>[]
         {
            (service, arg1, arg2) => service.RecordOpenViewAsync(arg1), // 0
            (service, arg1, arg2) => service.RecordOpenViewAsync(arg1, arg2), // 1
            (service, arg1, arg2) => service.RecordCloseViewAsync(arg1), // 2
            (service, arg1, arg2) => service.StartView(arg1), // 3
            (service, arg1, arg2) => service.StartView(arg1, arg2), // 4
            (service, arg1, arg2) => service.StartAutoStoppedView(arg1), // 5
            (service, arg1, arg2) => service.StartAutoStoppedView(arg1, arg2), // 6
            (service, arg1, arg2) => service.StopViewWithName(arg1), // 7
            (service, arg1, arg2) => service.StopViewWithName(arg1, arg2), // 8
            (service, arg1, arg2) => service.StopViewWithID(arg1), // 9
            (service, arg1, arg2) => service.StopViewWithID(arg1, arg2), // 10
            (service, arg1, arg2) => service.PauseViewWithID(arg1), // 11
            (service, arg1, arg2) => service.ResumeViewWithID(arg1), // 12
            (service, arg1, arg2) => service.StopAllViews(arg2), // 13
            (service, arg1, arg2) => service.SetGlobalViewSegmentation(arg2), // 14
            (service, arg1, arg2) => service.AddSegmentationToViewWithID(arg1, arg2), // 15
            (service, arg1, arg2) => service.AddSegmentationToViewWithName(arg1, arg2), // 16
            (service, arg1, arg2) => service.UpdateGlobalViewSegmentation(arg2), // 17
         };

        Dictionary<string, object> testSegmentation;

        // Initializes Countly and clears event repo and request repo
        // Validates that repositories are clean before testing
        [SetUp]
        public void ScenarioTestSetup()
        {
            TestUtility.TestCleanup();

            testSegmentation = TestUtility.TestSegmentation();
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Countly.Instance.RequestHelper._requestRepo.Clear();
            Countly.Instance.Views._eventService._eventRepo.Clear();

            viewService = Countly.Instance.Views;
            viewService.safeViewIDGenerator = idGenerator;
            //viewService._eventService.safeEventIDGenerator = new CustomIdProvider();

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
                Debug.Log($"{func.Method.Name} executed successfully with null values.");
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
                Debug.Log($"{func.Method.Name} executed successfully with empty string values.");
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
                Debug.Log($"{func.Method.Name} executed successfully with empty string values.");
            }
        }

        [Test]
        public void MV_200B_autoStoppedView_autoClose()
        {
            string viewA = "viewA";
            string viewB = "viewB";
            //string viewC = "viewC";

            viewService.StartAutoStoppedView(viewA);
            Thread.Sleep(1000);

            // eE_A d=1 id=idv1 pvid="", segm={}
            CountlyEventModel viewEventAStart = viewService._eventService._eventRepo.Dequeue();
            Assert.AreEqual(viewEventAStart.EventID, "idv1");
            Assert.AreEqual(viewEventAStart.PreviousViewID, "");
            Assert.AreEqual(viewEventAStart.Segmentation["visit"], 1);
            Assert.AreEqual(viewEventAStart.Segmentation["start"], 1);

            Debug.Log(viewEventAStart);

            // eE_A d=1 id=idv1 pvid="", segm={}, (sE_B id=idv2 pvid=idv1 segm={visit="1"}
            viewService.StartAutoStoppedView(viewB);
            CountlyEventModel viewEventAEnd = viewService._eventService._eventRepo.Dequeue();

            Assert.AreEqual(viewEventAEnd.EventID, "idv1");
            Assert.AreEqual(viewEventAEnd.PreviousViewID, "");
            Assert.AreEqual(viewEventAEnd.Duration, 1);

            Assert.IsFalse(viewEventAEnd.Segmentation.ContainsKey("visit"));
            Assert.IsFalse(viewEventAEnd.Segmentation.ContainsKey("start"));

            Debug.Log(viewEventAEnd);

            /*
            viewService.StartAutoStoppedView(viewB);
            CountlyEventModel viewEventAEnd = viewService._eventService._eventRepo.Dequeue();
            Debug.Log(viewEventAEnd);

            CountlyEventModel viewEventBStart = viewService._eventService._eventRepo.Dequeue();
            Debug.Log(viewEventBStart);
            Thread.Sleep(1000);

            /*
            viewService.StartAutoStoppedView(viewC);
            CountlyEventModel viewEventBEnd = viewService._eventService._eventRepo.Dequeue();
            Debug.Log(viewEventBEnd);
            CountlyEventModel viewEventCStart = viewService._eventService._eventRepo.Dequeue();
            Debug.Log(viewEventCStart);

            viewService.StopAllViews(null);
            CountlyEventModel viewEventCEnd = viewService._eventService._eventRepo.Dequeue();
            Debug.Log(viewEventCEnd);
            */
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