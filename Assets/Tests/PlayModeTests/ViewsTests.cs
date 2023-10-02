using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;
using Assets.Tests.PlayModeTests;

namespace Tests
{
    public class ViewsTests
    {
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

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
