using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Services;

namespace Assets.Tests.PlayModeTests
{
    public class ViewsTests
    {
        private IViewCountlyService _viewService;

        [SetUp]
        public void SetUp()
        {
            TestUtility.TestCleanup();
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
            _viewService.StartView(viewName);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.IsTrue(result.Segmentation.ContainsValue(viewName));
        }

        // 'StartView' method in ViewCountlyService class
        // We validate that repository is clear and, try to start a view with empty and view name
        // View with empty or null ViewName shouldn't start 
        [Test]
        public void StartView_EmptyNullViewName()
        {
            ViewServiceSetup();

            _viewService.StartView(string.Empty);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StartView(null);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'StartView' method in ViewCountlyService class
        // We validate the repository is clear, start a view with view name and segmentation
        // View should be recorded with given segmentation
        [Test]
        public void StartView_WithValidSegmentation()
        {
            ViewServiceSetup();

            // segmentation data types are string, int, double, float, bool
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

            string viewName = "viewName";
            _viewService.StartView(viewName, segmentation);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.AreEqual(result.Segmentation["string"], "Hello!");
            Assert.AreEqual(result.Segmentation["int"], 42);
            Assert.AreEqual(result.Segmentation["double"], 3.14);
            Assert.AreEqual(result.Segmentation["float"], 2.5f);
            Assert.AreEqual(result.Segmentation["bool"], true);
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
            _viewService.StartView(viewName, segmentation);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.IsFalse(result.Segmentation.ContainsKey("garbageObject"));
        }

        // 'StopViewWithName' method in ViewCountlyService class
        // We validate the repository is clear, try to stop a view which is not existing
        // Repository should remain clean and nothing should be recorded
        [Test]
        public void StopViewWithName_NoCurrentView()
        {
            ViewServiceSetup();

            _viewService.StopViewWithName("viewName");
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

            _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StopViewWithName(viewName);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            // validation of view is already stopped 
            _viewService.StopViewWithName(viewName);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);
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
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

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
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

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

            _viewService.StartView(viewName);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StartView(viewName2);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            _viewService.StopAllViews(null);
            Assert.AreEqual(4, Countly.Instance.Views._eventService._eventRepo.Count);

            // double checking for stopping a view that's already stopped, nothing should happen
            _viewService.StopViewWithName(viewName);
            Assert.AreEqual(4, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        // 'SetGlobalViewSegmentation' method in ViewCountlyService
        // We set a global segmentation that's going to be recorded
        // Segmentation should be recorded without providing it again
        [Test]
        public void SetGlobalSegmentation()
        {
            ViewServiceSetup();
            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

            _viewService.SetGlobalViewSegmentation(segmentation);

            string viewName = "viewName";
            _viewService.StartView(viewName);

            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.AreEqual(result.Segmentation["string"], "Hello!");
            Assert.AreEqual(result.Segmentation["int"], 42);
            Assert.AreEqual(result.Segmentation["double"], 3.14);
            Assert.AreEqual(result.Segmentation["float"], 2.5f);
            Assert.AreEqual(result.Segmentation["bool"], true);
        }

        // 'AddSegmentationToViewWithID' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view id 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithID()
        {
            ViewServiceSetup();

            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

            string viewName = "viewName";
            string viewID = _viewService.StartView(viewName);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.IsFalse(result.Segmentation.ContainsKey("string"));
            Assert.IsFalse(result.Segmentation.ContainsKey("int"));
            Assert.IsFalse(result.Segmentation.ContainsKey("double"));
            Assert.IsFalse(result.Segmentation.ContainsKey("float"));
            Assert.IsFalse(result.Segmentation.ContainsKey("bool"));

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.AddSegmentationToViewWithID(viewID, segmentation);
            _viewService.StopViewWithID(viewID);

            CountlyEventModel segmResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(segmResult);

            Assert.AreEqual(segmResult.Segmentation["string"], "Hello!");
            Assert.AreEqual(segmResult.Segmentation["int"], 42);
            Assert.AreEqual(segmResult.Segmentation["double"], 3.14);
            Assert.AreEqual(segmResult.Segmentation["float"], 2.5f);
            Assert.AreEqual(segmResult.Segmentation["bool"], true);
        }

        // 'AddSegmentationToViewWithName' method in ViewCountlyService
        // We add segmentation to a view that's currently open, by using it's view name 
        // Segmentation should be recorded correctly
        [Test]
        public void AddSegmentationToViewWithName()
        {
            ViewServiceSetup();

            Dictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("string", "Hello!");
            segmentation.Add("int", 42);
            segmentation.Add("double", 3.14);
            segmentation.Add("float", 2.5f);
            segmentation.Add("bool", true);

            string viewName = "viewName";
            _viewService.StartView(viewName);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.IsFalse(result.Segmentation.ContainsKey("string"));
            Assert.IsFalse(result.Segmentation.ContainsKey("int"));
            Assert.IsFalse(result.Segmentation.ContainsKey("double"));
            Assert.IsFalse(result.Segmentation.ContainsKey("float"));
            Assert.IsFalse(result.Segmentation.ContainsKey("bool"));

            Countly.Instance.Views._eventService._eventRepo.Clear();

            _viewService.AddSegmentationToViewWithName(viewName, segmentation);
            _viewService.StopViewWithName(viewName);

            CountlyEventModel segmResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(segmResult);
            Assert.AreEqual(segmResult.Segmentation["string"], "Hello!");
            Assert.AreEqual(segmResult.Segmentation["int"], 42);
            Assert.AreEqual(segmResult.Segmentation["double"], 3.14);
            Assert.AreEqual(segmResult.Segmentation["float"], 2.5f);
            Assert.AreEqual(segmResult.Segmentation["bool"], true);
        }

        [Test]
        public void UpdateGlobalViewSegmentation()
        {
            ViewServiceSetup();

            Dictionary<string, object> globalSegmentation = new Dictionary<string, object>();
            globalSegmentation.Add("string", "Hello!");
            globalSegmentation.Add("int", 42);
            globalSegmentation.Add("double", 3.14);
            globalSegmentation.Add("float", 2.5f);
            globalSegmentation.Add("bool", true);

            _viewService.SetGlobalViewSegmentation(globalSegmentation);

            string viewName = "viewName";
            _viewService.StartView(viewName);

            CountlyEventModel result = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.NotNull(result);
            Assert.IsTrue(result.Segmentation.ContainsKey("string"));
            Assert.IsTrue(result.Segmentation.ContainsKey("int"));
            Assert.IsTrue(result.Segmentation.ContainsKey("double"));
            Assert.IsTrue(result.Segmentation.ContainsKey("float"));
            Assert.IsTrue(result.Segmentation.ContainsKey("bool"));

            Dictionary<string, object> segmentationUpdate = new Dictionary<string, object>();
            segmentationUpdate.Add("string", "Bye Bye!");
            segmentationUpdate.Add("New Value", 88);

            _viewService.UpdateGlobalViewSegmentation(segmentationUpdate);

            _viewService.StopViewWithName(viewName);
            CountlyEventModel secondResult = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            Assert.NotNull(secondResult);
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("string"));
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("int"));
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("double"));
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("float"));
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("bool"));
            Assert.IsTrue(secondResult.Segmentation.ContainsKey("New Value"));
        }

        // Set up the view service and make sure that repository is clean
        public void ViewServiceSetup()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            _viewService = Countly.Instance.views;

            Assert.IsNotNull(_viewService);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
