using System.Collections.Generic;
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
        
        // It validates the event repository initial state.
        // After initializing Countly, ViewCountlyService shouldn't be "null".
        // If no event is recorded, count in the event repository should be 0.
        [Test]
        public void ViewsRepoInitialState()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We validate the view interraction with of 'Event Consent'.
        // Views Service should only record if consent is given.
        [Test]
        public async void EventConsentDependency()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.RequiresConsent = true;

            configuration.GiveConsent(new Consents[] { Consents.Views });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "open_view", true);

            await Countly.Instance.Views.RecordOpenViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "close_view", false);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // Validating how Views Service performs if no "views" consent is given.
        // If no consent is given, Views Service shouldn't record anything
        [Test]
        public async void ViewsConsent()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.RequiresConsent = true;
            Countly.Instance.Init(configuration);

            Countly.Instance.Events._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // We validate the limit of the view's name size with configuring MaxKeyLength.
        // View Name length should be equal to the "MaxKeyLength"
        [Test]
        public async void ViewNameLimit()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.MaxKeyLength = 5;
            Countly.Instance.Init(configuration);

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(2, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "open_", true);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "close", false);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // We close a view. Verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated by TestUtility.ValidateViewEvent
        [Test]
        public async void RecordCloseViewAsync()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "close_view", false);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // Close a view, verify that the event is correctly recorded in the Views repository
        // If a null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordCloseViewAsync_NullViewName()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync(null);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // Close a view, verify that the event is correctly recorded in the Views repository
        // If an empty view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordCloseViewAsync_EmptyViewName()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated by TestUtility.ValidateViewEvent
        // It's possible to open 2 views at the same time
        [Test]
        public async void RecordOpenViewAsync()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            //record the first view
            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);
            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "open_view", true);

            //record the second view and make sure it's not marked as a start view
            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);
            model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "open_view_2", true, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If an null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordOpenViewAsync_NullViewName()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync(null);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If an empty view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordOpenViewAsync_EmptyViewName()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view with segmentation  and verify that verifies the event is correctly recorded in the Views repository 
        // If a valid view name and segmentation is provided, it should be recorded and EventModel should be validated by TestUtility.ValidateViewEvent
        [Test]
        public async void RecordOpenViewAsyncWithSegment()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Dictionary<string, object> segmentations = new Dictionary<string, object> {
                { "name", "new_open_view" }, // override name
                { "key1", "value1" },
                { "key2", null }, // invalid value
                { "", "value2" }, // invalid key
                { "visit", null } // override existing key with invalid value
            };

            await Countly.Instance.Views.RecordOpenViewAsync("open_view", segmentations);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            TestUtility.ValidateViewEvent(model, "open_view", true);
            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.IsFalse(model.Segmentation.ContainsKey("key2"));
            Assert.IsFalse(model.Segmentation.ContainsKey(""));

            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            TestUtility.ValidateViewEvent(model, "open_view_2", true, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We set an EventQueueThreshold limit before initialization and add events.
        // Once we reach the treshold, all events in the EQ should be written out to the RQ
        [Test]
        public async void EventQueueThreshold_Limit()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.EventQueueThreshold = 3;
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("open_view");
            Assert.AreEqual(2, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 10, 100, 100);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            //todo: add verification in the RQ
        }

        // 'ReportActionAsync' method in ViewCountlyService.
        // We report a particular action with the specified details
        // The action should be recorded and its fields are validated.
        [Test]
        public async void ReportActionAsync()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 20, 100, 100);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "", false, 0, true);

            Assert.AreEqual("action", model.Segmentation["type"]);
            Assert.AreEqual(10, model.Segmentation["x"]);
            Assert.AreEqual(20, model.Segmentation["y"]);
            Assert.AreEqual(100, model.Segmentation["width"]);
            Assert.AreEqual(100, model.Segmentation["height"]);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We record a view. Consent is required, consent is given. The recorded view is the first view.
        // The recorded view should be marked with te "first_view" flag
        // "isFirstView" functionality is not exposed anymore. This test is going to be removed with RecordOpenViewAsync method, in the future.
        [Test]
        public async void StartField()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfigConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);
            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ValidateViewEvent(model, "first_view", true);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService and 'ChangeDeviceIdWithoutMerge' method in DeviceIdCountlyService.
        // We validate the behavior of the "isFirstView" and view event recording after changing the device ID without merging
        // "isFirstView" field should be true again after changing the device ID.
        // "isFirstView" functionality is not exposed anymore. This test is going to be removed with RecordOpenViewAsync method, in the future.
        [Test]
        public async void StartField_AfterDeviceIdChangeWithoutMerge()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfigConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new device id");
            Countly.Instance.Events._eventRepo.Clear();

            await Countly.Instance.Views.RecordOpenViewAsync("second_view_open");

            Assert.IsFalse(Countly.Instance.Consents.CheckConsent(Consents.Views));
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
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
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId = views.StartView(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
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
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            views.StartView(null);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            views.StartView(" ");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
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
        // View should be stopped and a stopped view shouldn't be able to stop again
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithName(viewNames[0]);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithName(viewNames[0], testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithID("random view id");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.StopViewWithID(viewId, testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);
            views.PauseViewWithID(viewId);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = Countly.Instance.Events._eventRepo.Dequeue();
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());
            
            views.PauseViewWithID("random view id");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.PauseViewWithID(viewId);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = Countly.Instance.Events._eventRepo.Dequeue();
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.PauseViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultPause = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(resultPause, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());
            
            views.ResumeViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.StopViewWithID(viewId, testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel resultStop = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId1, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = views.StartView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result2 = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result2, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], true, false), viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel result1End = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result1End, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[0], false, false), viewId1, viewId1, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel result2End = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.AddSegmentationToViewWithID(viewId, TestUtility.TestSegmentation());

            Thread.Sleep(1000);
            
            views.StopViewWithID(viewId);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel segmResult = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            views.AddSegmentationToViewWithName(viewNames[0], testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel segmResult = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(segmResult, 1, 0, 1, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            Dictionary<string, object> segmentationUpdate = new Dictionary<string, object>();
            segmentationUpdate.Add("string", "Bye Bye!");
            segmentationUpdate.Add("New Value", 88);

            views.UpdateGlobalViewSegmentation(segmentationUpdate);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel secondResult = Countly.Instance.Events._eventRepo.Dequeue();
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

            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
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
            CountlyEventModel result = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[0], true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = views.StartAutoStoppedView(viewNames[1]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2Start = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Start, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[1], true, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.PauseViewWithID(viewId2);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view2Pause = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Pause, 1, 0, 1, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            views.ResumeViewWithID(viewId2);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewId3 = views.StartView(viewNames[2]);
            TestUtility.ValidateRQEQSize(cly, 2, 2);
            CountlyEventModel view2Stop = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view2Stop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewNames[1], false, false), viewId2, viewId, null, null, TestUtility.TestTimeMetrics());

            CountlyEventModel view3Start = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Start, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewNames[2], true, false), viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.SetGlobalViewSegmentation(testSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.PauseViewWithID(viewId3);
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel view3Pause = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Pause, 1, 0, 1, testSegmentation, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());

            views.ResumeViewWithID(viewId3);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> updatedSegmentation = testSegmentation;
            updatedSegmentation.Add("another string", "another object");
            views.UpdateGlobalViewSegmentation(updatedSegmentation);
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopViewWithName(viewNames[0]);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view1Stop = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view1Stop, 1, 0, 2, updatedSegmentation, viewId, viewId2, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel view3Stop = Countly.Instance.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(view3Stop, 1, 0, 1, updatedSegmentation, viewId3, viewId2, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        [SetUp][TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}