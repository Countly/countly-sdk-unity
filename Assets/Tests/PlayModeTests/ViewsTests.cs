﻿using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Services;
using System.Threading;

namespace Assets.Tests.PlayModeTests
{
    public class ViewsTests
    {
        readonly Dictionary<string, object> testSegmentation;
        static string[] viewNames;

        public ViewsTests()
        {
            testSegmentation = TestUtility.TestSegmentation();
            InitializeViewNames(10);
        }
        #region Deprecated Function Tests        

        // Method to initialize the viewNames array
        private static void InitializeViewNames(int arraySize)
        {
            if (viewNames == null) {
                viewNames = new string[arraySize];
                for (int i = 0; i < arraySize; i++) {
                    viewNames[i] = "viewName" + (i + 1);
                }
            }
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // Validating how Views Service performs if no "views" consent is given.
        // If no consent is given, Views Service shouldn't record anything
        [Test]
        public async void ViewsConsent()
        {
            CountlyConfiguration config = TestUtility.CreateNoConsentConfig();
            Countly cly = Countly.Instance;

            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await cly.Views.RecordOpenViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await cly.Views.RecordCloseViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // We try to close a view that's not existing
        // If view does not exist nothing should be recorded, nothing should crash
        [Test]
        public async void RecordCloseViewAsync_NoOpenView()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            await cly.Views.RecordCloseViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService. 
        // Close a view, verify that the event is correctly recorded in the Views repository
        // If a null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordCloseViewAsync_NullAndEmptyViewName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await cly.Views.RecordCloseViewAsync(null);
            await cly.Views.RecordCloseViewAsync("");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated
        // It's possible to open 2 views at the same time
        [Test]
        public async void RecordOpenViewAsync()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            await cly.Views.RecordOpenViewAsync(viewNames[0]);
            await cly.Views.RecordCloseViewAsync(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), "idv1", "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If an null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordOpenViewAsync_NullAndEmptyViewName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await cly.Views.RecordOpenViewAsync(null);
            await cly.Views.RecordOpenViewAsync("");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view with segmentation  and verify that verifies the event is correctly recorded in the Views repository 
        // If a valid view name and segmentation is provided, it should be recorded and EventModel should be validated
        [Test]
        public async void RecordOpenViewAsyncWithSegment()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> providedSegmentations = new Dictionary<string, object> {
                { "key1", "value1" },
                { "key2", null }, // invalid value
                { "key3", "" }, // invalid value
                { "key4", new object() }, // invalid value
            };

            Dictionary<string, object> expectedSegmentations = new Dictionary<string, object> {
                { "key1", "value1" },
            };

            await Countly.Instance.Views.RecordOpenViewAsync(viewNames[0], providedSegmentations);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, 0, null, expectedSegmentations, "idv1", "", null, null, TestUtility.TestTimeMetrics());

            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.IsFalse(model.Segmentation.ContainsKey("key2"));
            Assert.IsFalse(model.Segmentation.ContainsKey(""));

            await Countly.Instance.Views.RecordOpenViewAsync(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), "idv2", "idv1", null, null, TestUtility.TestTimeMetrics());
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We set an EventQueueThreshold limit before initialization and add events.
        // Once we reach the treshold, all events in the EQ should be written out to the RQ
        [Test]
        public async void EventQueueThreshold_Limit()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider())
                .SetEventQueueSizeToSend(1);
            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            await Countly.Instance.Views.RecordOpenViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 3, 0);
            await Countly.Instance.Views.RecordCloseViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 4, 0);
            await Countly.Instance.Views.ReportActionAsync("action", 10, 10, 100, 100);
            TestUtility.ValidateRQEQSize(cly, 5, 0);
        }

        // 'ReportActionAsync' method in ViewCountlyService.
        // We report a particular action with the specified details
        // The action should be recorded and its fields are validated.
        [Test]
        public async void ReportActionAsync()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> action = new Dictionary<string, object> {
                { "type", "action" },
                { "x", 10 },
                { "y", 20 },
                { "width", 100 },
                { "height", 100 }
            };

            await Countly.Instance.Views.ReportActionAsync("action", 10, 20, 100, 100);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, action, null, null, null, null, TestUtility.TestTimeMetrics(), true);

            Assert.AreEqual("action", model.Segmentation["type"]);
            Assert.AreEqual(10, model.Segmentation["x"]);
            Assert.AreEqual(20, model.Segmentation["y"]);
            Assert.AreEqual(100, model.Segmentation["width"]);
            Assert.AreEqual(100, model.Segmentation["height"]);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService and 'ChangeDeviceIdWithoutMerge' method in DeviceIdCountlyService.
        // We validate the behavior of the "isFirstView" and view event recording after changing the device ID without merging
        // When a view is started afterwards, it should be count as first view
        [Test]
        public async void StartField_AfterDeviceIdChangeWithoutMerge()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await Countly.Instance.Views.RecordOpenViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), "idv1", "", null, null, TestUtility.TestTimeMetrics());

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new device id");
            Countly.Instance.Consents.GiveConsentAll();

            await Countly.Instance.Views.RecordOpenViewAsync(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 4, 1);
            CountlyEventModel secondModel = cly.Events._eventRepo.Dequeue();
            // BaseViewTestSegmentation(string viewName, bool isVisit, bool isStart) passing true in here means that this is first view
            TestUtility.ViewEventValidator(secondModel, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], true, true), "idv2", "idv1", null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            await Countly.Instance.Views.RecordCloseViewAsync(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 4, 1);
            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), "idv1", "idv1", null, null, TestUtility.TestTimeMetrics());

            await Countly.Instance.Views.RecordCloseViewAsync(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 4, 1);
            CountlyEventModel resultStop2 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop2, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), "idv2", "idv1", null, null, TestUtility.TestTimeMetrics());
        }

        #endregion
        // 'StartView' method in ViewCountlyService class
        // We validate that repository is clear and, start a view with view name
        // View should be recorded with correct view name
        [Test]
        public void StartView_ValidViewName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = cly.Views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView' method in ViewCountlyService class
        // We validate that repository is clear and, try to start a view with empty and view name
        // View with empty or null ViewName shouldn't start 
        [Test]
        public void StartView_EmptyNullWhitespaceViewName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StartView(string.Empty);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StartView(null);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StartView(" ");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StartView("");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView' method in ViewCountlyService class
        // We validate the repository is clear, start a view with view name and segmentation
        // View should be recorded with given segmentation
        [Test]
        public void StartView_WithValidSegmentation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0], testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'StartView' method in ViewCountlyService class
        // We validate the repository is clear, start a view with view name and invalid custom segmentation
        // View should be recorded and invalid segmentation data types should be truncated
        [Test]
        public void StartView_WithGarbageSegmentation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            // segmentation data types are string, int, double, float, bool
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("garbageObject", new Object());

            string viewId = views.StartView(viewNames[0], segmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());
            Assert.IsFalse(result.Segmentation.ContainsKey("garbageObject"));
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithName', 'StopViewWithID' and 'StopAllViews' methods in ViewCountlyService class
        // We validate the repository is clear, try to stop a view which is not existing
        // Repository should remain clean and nothing should be recorded
        [Test]
        public void StopView_NoCurrentView()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithName(viewNames[0]);
            views.StopViewWithID("viewId");
            views.StopAllViews(null);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithName' method in ViewCountlyService class
        // We validate the repository is clear, stop a view that is open
        // View should be stopped and trying to stop a view that's already stopped shouldn't record anything
        [Test]
        public void StopViewWithName_OpenView()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());

            // validation of view is already stopped 
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithName' method in ViewCountlyService class
        // We validate the repository is clear, stop a view that is open with segmentation
        // View should be stopped and recorded with segmentation
        [Test]
        public void StopViewWithName_OpenViewWithSegmentation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithName(viewNames[0], testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 0, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, and stop a view with view id
        // View should be stopped if a valid id is provided
        [Test]
        public void StopViewWithID_ValidID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, and use an invalid id to stop a view
        // Repository should remain clean and nothing should be recorded
        [Test]
        public void StopViewWithID_InvalidID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithID("random view id");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, stop a view with view id and segmentation
        // View should be stopped and segmentation should be recorded
        [Test]
        public void StopViewWithID_WithSegmentation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithID(viewId, testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 0, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'PauseViewWithID' method in ViewCountlyService
        // We pause an open view with the view id
        // View should be paused and it should be recorded
        [Test]
        public void PauseViewWithID_ValidID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);
            views.PauseViewWithID(viewId);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultPause, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'PauseViewWithID' method in ViewCountlyService
        // We try to pause a view with an invalid view id
        // View should resume and nothing should be recorded
        [Test]
        public void PauseViewWithID_InvalidID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.PauseViewWithID("random view id");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.PauseViewWithID(viewId);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultPause, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'ResumeViewWithID' method in ViewCountlyService
        // We resume a paused event with the view id
        // View should resume and resuming view shouldn't record anything
        [Test]
        public void ResumeViewWithID_ValidID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.PauseViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultPause, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.ResumeViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.StopViewWithID(viewId, testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultStop, 1, 0, 1, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StopAllViews' method in ViewCountlyService
        // We stop all views that are open and record each stopped view
        // All views should be stopped and recorded
        [Test]
        public void StopAllViews()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId1 = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId1, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = views.StartView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result2 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result2, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], true, false), viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel result1End = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result1End, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId1, viewId1, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result2End = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result2End, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'AddSegmentationToViewWithID' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view id 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithID()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.AddSegmentationToViewWithID(viewId, TestUtility.TestSegmentation());

            Thread.Sleep(1000);

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel segmResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'AddSegmentationToViewWithID' method in ViewCountlyService
        // We provide 'null' values for 'AddSegmentationToViewWithID' method
        // Nothing should crash and no value should be recorded
        [Test]
        public void AddSegmentationToViewWithID_NullValues()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            // null
            views.AddSegmentationToViewWithID(null, null);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            // empty
            views.AddSegmentationToViewWithID("", new Dictionary<string, object>());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel segmResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'AddSegmentationToViewWithName' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view name 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.AddSegmentationToViewWithName(viewNames[0], testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel segmResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'AddSegmentationToViewWithName' method in ViewCountlyService
        // We provide null and empty values to segmentation to 'AddSegmentationToViewWithName' method
        // Nothing should crash and no value should be recorded
        [Test]
        public void AddSegmentationToViewWithName_NullEmptyValues()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            // null
            views.AddSegmentationToViewWithName(null, null);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            // empty
            views.AddSegmentationToViewWithName("", new Dictionary<string, object>());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel segmResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // Opening multiple views with same name to test functionality
        // We open the views, update the global segmentation, and singular view segmentation
        // Views should open, nothing should break and segmentation should be able to update
        [Test]
        public void MultipleViewsWithSameName()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId1 = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId1, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result2 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result2, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());

            string viewId3 = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result3 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result3, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());

            Dictionary<string, object> globalUpdate = new Dictionary<string, object>();
            globalUpdate.Add("string", "Bye Bye!");
            globalUpdate.Add("New Value", 88);

            views.UpdateGlobalViewSegmentation(globalUpdate);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> singularUpdate = new Dictionary<string, object>();
            singularUpdate.Add("console", "xbox");

            views.AddSegmentationToViewWithName(viewNames[0], singularUpdate);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            Dictionary<string, object> finalSegmentation = new Dictionary<string, object>();
            finalSegmentation.Add("console", "xbox");
            finalSegmentation.Add("string", "Bye Bye!");
            finalSegmentation.Add("New Value", 88);

            CountlyEventModel firstResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(firstResult, 1, 0, 0, finalSegmentation, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel secondResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(secondResult, 1, 0, 0, globalUpdate, viewId1, viewId2, null, null, TestUtility.TestTimeMetrics());

            CountlyEventModel thirdResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(thirdResult, 1, 0, 0, globalUpdate, viewId2, viewId2, null, null, TestUtility.TestTimeMetrics());
        }

        // 'SetGlobalViewSegmentation' and 'UpdateGlobalViewSegmentation' method in ViewCountlyService
        // We set global segmentation and update it afterwards
        // Segmentation values before and after the update should be correct
        [Test]
        public void SetAndUpdateGlobalViewSegmentation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.SetGlobalViewSegmentation(testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            Dictionary<string, object> segmentationUpdate = new Dictionary<string, object>();
            segmentationUpdate.Add("string", "Bye Bye!");
            segmentationUpdate.Add("New Value", 88);

            views.UpdateGlobalViewSegmentation(segmentationUpdate);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel secondResult = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(secondResult, 1, 0, 0, segmentationUpdate, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'SetGlobalViewSegmentation' method in ViewCountlyService
        // We set a global segmentation with garbage value
        // Segmentation should be recorded without garbage value
        [Test]
        public void SetGlobalSegmentation_GarbageValue()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            TestUtility.TestSegmentation().Add("testObj", new object());
            TestUtility.TestSegmentation().Add("nullObj", null);
            views.SetGlobalViewSegmentation(testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            Assert.IsTrue(!result.Segmentation.ContainsKey("testObj"));
            Assert.IsTrue(!result.Segmentation.ContainsKey("nullObj"));
            TestUtility.ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // Multiple methods in ViewCountlyService to test the flow
        // We start, pause, stop views and add segmentations in the mean while
        // Flow should be recorded correctly
        [Test]
        public void ViewFlow()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = views.StartAutoStoppedView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2Start = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Start, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], true, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.PauseViewWithID(viewId2);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2Pause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Pause, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            views.ResumeViewWithID(viewId2);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId3 = views.StartView(viewNames[2]);
            TestUtility.ValidateRQEQSize(cly, 2, 2);
            CountlyEventModel view2Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Stop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            CountlyEventModel view3Start = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Start, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[2], true, false), viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.SetGlobalViewSegmentation(testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.PauseViewWithID(viewId3);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel view3Pause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Pause, 1, 0, 1, testSegmentation, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());

            views.ResumeViewWithID(viewId3);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> updatedSegmentation = testSegmentation;
            updatedSegmentation.Add("another string", "another object");
            views.UpdateGlobalViewSegmentation(updatedSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view1Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view1Stop, 1, 0, 2, updatedSegmentation, viewId, viewId2, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view3Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Stop, 1, 0, 1, updatedSegmentation, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // Multiple methods in ViewCountlyService
        // We provide segmentation with view and check every supported data type
        // string, bool, float, double, string, long and, their list and arrays are supported types
        // Supported data types should be recorded, unsupported types should be removed correctly
        [Test]
        public void SegmentationDataTypeValidation()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;

            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time", 1234455 },
                { "Retry Attempts", 10 },
                { "Temp", 100.0f },
                { "IsSuccess", true },
                { "Message", "Test message" },
                { "Average", 75.5 },
                { "LargeNumber", 12345678901234L },
                { "IntArray", new int[] { 1, 2, 3 } },
                { "BoolArray", new bool[] { true, false, true } },
                { "FloatArray", new float[] { 1.1f, 2.2f, 3.3f } },
                { "DoubleArray", new double[] { 1.1, 2.2, 3.3 } },
                { "StringArray", new string[] { "a", "b", "c" } },
                { "LongArray", new long[] { 10000000000L, 20000000000L, 30000000000L } },
                { "IntList", new List<int> { 1, 2, 3 } },
                { "BoolList", new List<bool> { true, false, true } },
                { "FloatList", new List<float> { 1.1f, 2.2f, 3.3f } },
                { "DoubleList", new List<double> { 1.1, 2.2, 3.3 } },
                { "StringList", new List<string> { "a", "b", "c" } },
                { "LongList", new List<long> { 10000000000L, 20000000000L, 30000000000L } },
                { "MixedList", new List<object> { 1, "string", 2.3, true, new int[] { 1, 2, 3 }, new object(), Countly.Instance } }, // mixed list
                { "MixedArray", new object[] { 1, "string", 2.3, true, new int[] { 1, 2, 3 }, new object(), Countly.Instance } }, // mixed array
                { "Unsupported Object", new object() }, // invalid
                { "Unsupported Dictionary", new Dictionary<string, object>() } // invalid
            };

            Dictionary<string, object> expectedSegm = new Dictionary<string, object>
            {
                { "Time", 1234455 },
                { "Retry Attempts", 10 },
                { "Temp", 100.0f },
                { "IsSuccess", true },
                { "Message", "Test message" },
                { "Average", 75.5 },
                { "LargeNumber", 12345678901234L },
                { "IntArray", new int[] { 1, 2, 3 } },
                { "BoolArray", new bool[] { true, false, true } },
                { "FloatArray", new float[] { 1.1f, 2.2f, 3.3f } },
                { "DoubleArray", new double[] { 1.1, 2.2, 3.3 } },
                { "StringArray", new string[] { "a", "b", "c" } },
                { "LongArray", new long[] { 10000000000L, 20000000000L, 30000000000L } },
                { "IntList", new List<int> { 1, 2, 3 } },
                { "BoolList", new List<bool> { true, false, true } },
                { "FloatList", new List<float> { 1.1f, 2.2f, 3.3f } },
                { "DoubleList", new List<double> { 1.1, 2.2, 3.3 } },
                { "StringList", new List<string> { "a", "b", "c" } },
                { "LongList", new List<long> { 10000000000L, 20000000000L, 30000000000L } }
            };

            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            // start and stop regular view
            string viewId1 = views.StartView(viewNames[0], seg);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view1 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view1, 1, 0, null, expectedSegm, viewId1, "", null, null, TestUtility.TestTimeMetrics());
            Thread.Sleep(1000);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view1Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view1Stop, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId1, "", null, null, TestUtility.TestTimeMetrics());

            // stop view with segmentation
            string viewId2 = views.StartView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());
            Thread.Sleep(1000);
            views.StopViewWithID(viewId2, seg);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Stop, 1, 0, 1, expectedSegm, viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());

            // set global segmentation
            string viewId3 = views.StartView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view3 = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);
            views.SetGlobalViewSegmentation(seg);

            views.StopViewWithID(viewId3);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view3Stop = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Stop, 1, 0, 1, expectedSegm, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}