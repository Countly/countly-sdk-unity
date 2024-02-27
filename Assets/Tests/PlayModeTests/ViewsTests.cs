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
        private IViewCountlyService _viewService;
        private string viewEventKey = "[CLY]_view";
        Dictionary<string, object> testSegmentation = new Dictionary<string, object>();

        [SetUp]
        public void SetUp()
        {
            TestUtility.TestCleanup();
            testSegmentation = TestUtility.TestSegmentation();
        }

        // Validates the properties of a CountlyEventModel object for view events or view action events
        private void ValidateViewEvent(CountlyEventModel model, string name, bool isOpenView, int start = 1, bool isAction = false)
        {
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);

            if (isAction) {
                Assert.AreEqual(CountlyEventModel.ViewActionEvent, model.Key);
            } else {
                Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
                Assert.AreEqual(name, model.Segmentation["name"]);
            }

            if (isOpenView) {
                Assert.AreEqual(1, model.Segmentation["visit"]);
                Assert.AreEqual(start, model.Segmentation["start"]);
            }
        }

        // It validates the event repository initial state.
        // After initializing Countly, ViewCountlyService shouldn't be "null".
        // If no event is recorded, count in the event repository should be 0.
        [Test]
        public void ViewsRepoInitialState()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We validate the view interraction with of 'Event Consent'.
        // Views Service should only record if consent is given.
        [Test]
        public async void EventConsentDependency()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.RequiresConsent = true;

            configuration.GiveConsent(new Consents[] { Consents.Views });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view", true);

            await Countly.Instance.Views.RecordOpenViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close_view", false);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // Validating how Views Service performs if no "views" consent is given.
        // If no consent is given, Views Service shouldn't record anything
        [Test]
        public async void ViewsConsent()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.RequiresConsent = true;
            Countly.Instance.Init(configuration);

            Countly.Instance.Views._eventService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // We validate the limit of the view's name size with configuring MaxKeyLength.
        // View Name length should be equal to the "MaxKeyLength"
        [Test]
        public async void ViewNameLimit()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.MaxKeyLength = 5;
            Countly.Instance.Init(configuration);

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_", true);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close", false);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // We close a view. Verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated by ValidateViewEvent
        [Test]
        public async void RecordCloseViewAsync()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close_view", false);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // Close a view, verify that the event is correctly recorded in the Views repository
        // If a null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordCloseViewAsync_NullViewName()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync(null);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // Close a view, verify that the event is correctly recorded in the Views repository
        // If an empty view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordCloseViewAsync_EmptyViewName()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated by ValidateViewEvent
        // It's possible to open 2 views at the same time
        [Test]
        public async void RecordOpenViewAsync()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            //record the first view
            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view", true);

            //record the second view and make sure it's not marked as a start view
            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view_2", true, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If an null view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordOpenViewAsync_NullViewName()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync(null);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view and verify that the event is correctly recorded in the Views repository
        // If an empty view name is provided, it shouldn't be recorded. 
        [Test]
        public async void RecordOpenViewAsync_EmptyViewName()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // Open a view with segmentation  and verify that verifies the event is correctly recorded in the Views repository 
        // If a valid view name and segmentation is provided, it should be recorded and EventModel should be validated by ValidateViewEvent
        [Test]
        public async void RecordOpenViewAsyncWithSegment()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            Dictionary<string, object> segmentations = new Dictionary<string, object> {
                { "name", "new_open_view" }, // override name
                { "key1", "value1" },
                { "key2", null }, // invalid value
                { "", "value2" }, // invalid key
                { "visit", null } // override existing key with invalid value
            };

            await Countly.Instance.Views.RecordOpenViewAsync("open_view", segmentations);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            ValidateViewEvent(model, "open_view", true);
            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.IsFalse(model.Segmentation.ContainsKey("key2"));
            Assert.IsFalse(model.Segmentation.ContainsKey(""));

            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            ValidateViewEvent(model, "open_view_2", true, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We set an EventQueueThreshold limit before initialization and add events.
        // Once we reach the treshold, all events in the EQ should be written out to the RQ
        [Test]
        public async void EventQueueThreshold_Limit()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.EventQueueThreshold = 3;
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("open_view");
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 10, 100, 100);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            //todo: add verification in the RQ
        }

        // 'ReportActionAsync' method in ViewCountlyService.
        // We report a particular action with the specified details
        // The action should be recorded and its fields are validated.
        [Test]
        public async void ReportActionAsync()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 20, 100, 100);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "", false, 0, true);

            Assert.AreEqual("action", model.Segmentation["type"]);
            Assert.AreEqual(10, model.Segmentation["x"]);
            Assert.AreEqual(20, model.Segmentation["y"]);
            Assert.AreEqual(100, model.Segmentation["width"]);
            Assert.AreEqual(100, model.Segmentation["height"]);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService.
        // We record a view. Consent is required, consent is given. The recorded view is the first view.
        // The recorded view should be marked with te "first_view" flag 
        [Test]
        public async void StartField()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfigConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsFalse(Countly.Instance.Views._isFirstView);

            // Dequeue the recorded event from the event repository for validation.
            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "first_view", true);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService and 'ChangeDeviceIdWithoutMerge' method in DeviceIdCountlyService.
        // We validate the behavior of the "isFirstView" and view event recording after changing the device ID without merging
        // "isFirstView" field should be true again after changing the device ID.
        [Test]
        public async void StartField_AfterDeviceIdChangeWithoutMerge()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfigConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            // Record the opening of the first view with the name "first_view".
            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsFalse(Countly.Instance.Views._isFirstView);

            // Change the device ID without merging.
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new device id");
            Countly.Instance.Views._eventService._eventRepo.Clear();

            // The "isFirstView" field should be true again after changing the device ID.
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            // Record the opening of the second view with the name "second_view_open".
            await Countly.Instance.Views.RecordOpenViewAsync("second_view_open");

            // The "isFirstView" field should still be true and the count of events in the event repository of the Views should be zero.
            Assert.IsTrue(Countly.Instance.Views._isFirstView);
            Assert.IsFalse(Countly.Instance.Consents.CheckConsent(Consents.Views));
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StartView' method in ViewCountlyService class
        // We validate that repository is clear and, start a view with view name
        // View should be recorded with correct view name
        [Test]
        public void StartView_ValidViewName()
        {
            ViewServiceSetup();

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Dictionary<string, object> baseSegmentation = TestUtility.BaseViewTestSegmentation(viewName, true, true);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            ViewEventValidator(result, 1, 0, null, baseSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'StartView' method in ViewCountlyService class
        // We validate that repository is clear and, try to start a view with empty and view name
        // View with empty or null ViewName shouldn't start 
        [Test]
        public void StartView_EmptyNullWhitespaceViewName()
        {
            ViewServiceSetup();

            _viewService.StartView(string.Empty);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StartView(null);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StartView(" ");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StartView' method in ViewCountlyService class
        // We validate the repository is clear, start a view with view name and segmentation
        // View should be recorded with given segmentation
        [Test]
        public void StartView_WithValidSegmentation()
        {
            ViewServiceSetup();

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName, testSegmentation);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'StartView' method in ViewCountlyService class
        // We validate the repository is clear, start a view with view name and invalid custom segmentation
        // View should be recorded and invalid segmentation data types should be truncated
        [Test]
        public void StartView_WithGarbageSegmentation()
        {
            ViewServiceSetup();

            // segmentation data types are string, int, double, float, bool
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("garbageObject", new Object());

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName, segmentation);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewName, true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            Assert.NotNull(result);
            Assert.IsFalse(result.Segmentation.ContainsKey("garbageObject"));
        }

        // 'StopViewWithName', 'StopViewWithID' and 'StopAllViews' methods in ViewCountlyService class
        // We validate the repository is clear, try to stop a view which is not existing
        // Repository should remain clean and nothing should be recorded
        [Test]
        public void StopView_NoCurrentView()
        {
            ViewServiceSetup();

            _viewService.StopViewWithName("viewName");
            _viewService.StopViewWithID("viewId");
            _viewService.StopAllViews(null);

            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StopViewWithName' method in ViewCountlyService class
        // We validate the repository is clear, stop a view that is open
        // View should be stopped and a stopped view shouldn't be able to stop again
        [Test]
        public void StopViewWithName_OpenView()
        {
            ViewServiceSetup();

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result, 1, 0, null, TestUtility.BaseViewTestSegmentation(viewName, true, true), viewId, "", null, null, TestUtility.TestTimeMetrics());

            _viewService.StopViewWithName(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel resultStop = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(resultStop, 1, 0, 0, TestUtility.BaseViewTestSegmentation(viewName, false, false), viewId, "", null, null, TestUtility.TestTimeMetrics());

            // validation of view is already stopped 
            _viewService.StopViewWithName(viewName);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StopViewWithName' method in ViewCountlyService class
        // We validate the repository is clear, stop a view that is open with segmentation
        // View should be stopped and recorded with segmentation
        [Test]
        public void StopViewWithName_OpenViewWithSegmentation()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            Countly.Instance.Views._eventService._eventRepo.Clear();

            // segmentation data types are string, int, double, float, bool
            Dictionary<string, object> segmentation = TestUtility.TestSegmentation();

            _viewService.StopViewWithName(viewName, segmentation);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.AreEqual(result.Segmentation["string"], "Hello!");
            Assert.AreEqual(result.Segmentation["int"], 42);
            Assert.AreEqual(result.Segmentation["double"], 3.14);
            Assert.AreEqual(result.Segmentation["float"], 2.5f);
            Assert.AreEqual(result.Segmentation["bool"], true);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, and stop a view with view id
        // View should be stopped if a valid id is provided
        [Test]
        public void StopViewWithID_ValidID()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            string viewID = _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StopViewWithID(viewID);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, and use an invalid id to stop a view
        // Repository should remain clean and nothing should be recorded
        [Test]
        public void StopViewWithID_InvalidID()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StopViewWithID("random view id");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StopViewWithID' method in ViewCountlyService
        // We validate the repository is clear, stop a view with view id and segmentation
        // View should be stopped and segmentation should be recorded
        [Test]
        public void StopViewWithID_WithSegmentation()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            string viewID = _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            // segmentation data types are string, int, double, float, bool
            Dictionary<string, object> segmentation = TestUtility.TestSegmentation();

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.StopViewWithID(viewID, segmentation);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.AreEqual(result.Segmentation["string"], "Hello!");
            Assert.AreEqual(result.Segmentation["int"], 42);
            Assert.AreEqual(result.Segmentation["double"], 3.14);
            Assert.AreEqual(result.Segmentation["float"], 2.5f);
            Assert.AreEqual(result.Segmentation["bool"], true);
        }

        // 'PauseViewWithID' method in ViewCountlyService
        // We pause an open view with the view id
        // View should be paused and it should be recorded
        [Test]
        public void PauseViewWithID_ValidID()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            string viewID = _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.PauseViewWithID(viewID);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'PauseViewWithID' method in ViewCountlyService
        // We try to pause a view with an invalid view id
        // View should resume and nothing should be recorded
        [Test]
        public void PauseViewWithID_InvalidID()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.PauseViewWithID("random view id");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'ResumeViewWithID' method in ViewCountlyService
        // We resume a paused event with the view id
        // View should resume and resuming view shouldn't record anything
        [Test]
        public void ResumeViewWithID_ValidID()
        {
            ViewServiceSetup();

            string viewName = "viewName";

            string viewID = _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.PauseViewWithID(viewID);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.ResumeViewWithID(viewID);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StopAllViews' method in ViewCountlyService
        // We stop all views that are open and record each stopped view
        // All views should be stopped and recorded
        [Test]
        public void StopAllViews()
        {
            ViewServiceSetup();

            string viewName = "viewName";
            string viewName2 = "viewName2";

            string viewId1 = _viewService.StartView(viewName);
            Dictionary<string, object> baseSegmentation = TestUtility.BaseViewTestSegmentation(viewName, true, true);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result, 1, 0, null, baseSegmentation, viewId1, "", null, null, TestUtility.TestTimeMetrics());

            string viewId2 = _viewService.StartView(viewName2);
            Dictionary<string, object> baseSegmentation2 = TestUtility.BaseViewTestSegmentation(viewName2, true, false);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result2 = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result2, 1, 0, null, baseSegmentation2, viewId2, viewId1, null, null, TestUtility.TestTimeMetrics());

            Thread.Sleep(1000);

            _viewService.StopAllViews(null);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            Dictionary<string, object> baseSegmentation1End = TestUtility.BaseViewTestSegmentation(viewName, false, false);
            CountlyEventModel result1End = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result1End, 1, 0, 1, baseSegmentation1End, viewId1, viewId1, null, null, TestUtility.TestTimeMetrics());
        }

        // 'AddSegmentationToViewWithID' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view id 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithID()
        {
            ViewServiceSetup();
            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);

            Dictionary<string, object> baseSegmentation = TestUtility.BaseViewTestSegmentation(viewName, true, true);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result, 1, 0, null, baseSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.AddSegmentationToViewWithID(viewId, testSegmentation);
            _viewService.StopViewWithID(viewId);

            CountlyEventModel segmResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(segmResult, 1, 0, 0, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'AddSegmentationToViewWithName' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view name 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithName()
        {
            ViewServiceSetup();
            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);

            Dictionary<string, object> baseSegmentation = TestUtility.BaseViewTestSegmentation(viewName, true, true);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            Assert.NotNull(result);
            ViewEventValidator(result, 1, 0, null, baseSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.AddSegmentationToViewWithName(viewName, testSegmentation);
            _viewService.StopViewWithName(viewName);

            CountlyEventModel segmResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            Assert.NotNull(segmResult);
            ViewEventValidator(segmResult, 1, 0, 0, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'SetGlobalViewSegmentation' and 'UpdateGlobalViewSegmentation' method in ViewCountlyService
        // We set global segmentation and update it afterwards
        // Segmentation values before and after the update should be correct
        [Test]
        public void SetAndUpdateGlobalViewSegmentation()
        {
            ViewServiceSetup();
            _viewService.SetGlobalViewSegmentation(testSegmentation);

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);
            
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());

            Dictionary<string, object> segmentationUpdate = new Dictionary<string, object>();
            segmentationUpdate.Add("string", "Bye Bye!");
            segmentationUpdate.Add("New Value", 88);

            _viewService.UpdateGlobalViewSegmentation(segmentationUpdate);
            _viewService.StopViewWithName(viewName);

            CountlyEventModel secondResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            Assert.NotNull(secondResult);
            ViewEventValidator(secondResult, 1, 0, 0, segmentationUpdate, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // 'SetGlobalViewSegmentation' method in ViewCountlyService
        // We set a global segmentation with garbage value
        // Segmentation should be recorded without garbage value
        [Test]
        public void SetGlobalSegmentation_GarbageValue()
        {
            ViewServiceSetup();

            testSegmentation.Add("testObj", new object());
            testSegmentation.Add("nullObj", null);
            _viewService.SetGlobalViewSegmentation(testSegmentation);

            string viewName = "viewName";
            string viewId = _viewService.StartView(viewName);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            Assert.IsTrue(!result.Segmentation.ContainsKey("testObj"));
            Assert.IsTrue(!result.Segmentation.ContainsKey("nullObj"));
            ViewEventValidator(result, 1, 0, null, testSegmentation, viewId, "", null, null, TestUtility.TestTimeMetrics());
        }

        // Set up the view service and make sure that repository is clean
        public void ViewServiceSetup()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            _viewService = Countly.Instance.views;

            Assert.IsNotNull(_viewService);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        public void ViewEventValidator(CountlyEventModel eventModel, int? expectedCount, double? expectedSum,
            int? expectedDuration, Dictionary<string, object>? expectedSegmentation,
            string? expectedEventId, string? expectedPreviousViewId, string? expectedCurrentViewId,
            string? expectedPreviousEventId, Dictionary<string, object> expectedTimeMetrics = null)
        {
            Assert.AreEqual(eventModel.Key, viewEventKey);
            Assert.AreEqual(eventModel.Count, expectedCount);
            Assert.AreEqual(eventModel.Sum, expectedSum);
            Assert.AreEqual(eventModel.Duration, expectedDuration);

            if(expectedSegmentation != null) {
                foreach (var kvp in expectedSegmentation) {
                    Assert.IsTrue(eventModel.Segmentation.ContainsKey(kvp.Key));
                    Assert.AreEqual(eventModel.Segmentation[kvp.Key], kvp.Value);
                }
            }
            
            Assert.AreEqual(eventModel.EventID, expectedEventId);
            Assert.AreEqual(eventModel.PreviousViewID, expectedPreviousViewId);
            Assert.AreEqual(eventModel.CurrentViewID, expectedCurrentViewId);
            Assert.AreEqual(eventModel.PreviousEventID, expectedPreviousEventId);

            // Check time metrics if provided
            if (expectedTimeMetrics != null) {
                foreach (var kvp in expectedTimeMetrics) {
                    if (kvp.Key == "timestamp") {
                        Assert.IsTrue(Mathf.Abs(eventModel.Timestamp - (long)kvp.Value) < 6000);
                    } else if (kvp.Key == "hour") {
                        Assert.AreEqual(eventModel.Hour, kvp.Value);
                    } else if (kvp.Key == "dow") {
                        Assert.AreEqual(eventModel.DayOfWeek, kvp.Value);
                    }
                }
            }
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
