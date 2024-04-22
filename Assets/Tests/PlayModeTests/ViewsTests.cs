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

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // Validating how Views Service performs if no "views" consent is given.
        // If no consent is given, Views Service shouldn't record anything
        [Test]
        public async void ViewsConsent()
        {
            CountlyConfiguration config = TestUtility.CreateNoConsentConfig();
            Countly cly = Countly.Instance;
            cly.Init(config);
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordOpenViewAsync' method in ViewCountlyService
        // We validate the limit of the view's name size with configuring MaxKeyLength.
        // View Name length should be equal to the "MaxKeyLength"
        [Test]
        public async void ViewNameLimit()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider())
                .SetMaxKeyLength(5);
            Countly cly = Countly.Instance;

            cly.Init(config);
            IViewModule views = cly.Views;
            IViewIDProvider iDGenerator = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await views.RecordOpenViewAsync("open_view");
            await views.RecordCloseViewAsync("close_view");
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("open_", true, true), iDGenerator.GetCurrentViewId(), null, iDGenerator.GetCurrentViewId(), null, TestUtility.TestTimeMetrics());

            model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("close", false, false), null, null, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordCloseViewAsync' method in ViewCountlyService.
        // We close a view. Verify that the event is correctly recorded in the Views repository
        // If a valid view name is provided, it should be recorded and EventModel should be validated
        [Test]
        public async void RecordCloseViewAsync()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            IViewModule views = cly.Views;
            IViewIDProvider iDGenerator = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await views.RecordCloseViewAsync("close_view");
            TestUtility.ValidateRQEQSize(cly, 2, 1);

            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("close_view", false, false), null, null, null, null, TestUtility.TestTimeMetrics());
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
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await views.RecordCloseViewAsync(null);
            await views.RecordCloseViewAsync("");
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
            IViewModule views = cly.Views;
            IViewIDProvider iDGenerator = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await views.RecordOpenViewAsync("open_view");
            await views.RecordCloseViewAsync("open_view_2");
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("open_view", true, true), iDGenerator.GetCurrentViewId(), null, iDGenerator.GetCurrentViewId(), null, TestUtility.TestTimeMetrics());
            model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("open_view_2", false, false), null, null, null, null, TestUtility.TestTimeMetrics());
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
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await views.RecordOpenViewAsync(null);
            await views.RecordOpenViewAsync("");
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
            IViewModule views = cly.Views;
            IViewIDProvider iDGenerator = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> segmentations = new Dictionary<string, object> {
                { "key1", "value1" },
                { "key2", null }, // invalid value
                { "key3", "" }, // invalid value
                { "key4", new object() }, // invalid value
            };

            await Countly.Instance.Views.RecordOpenViewAsync("open_view", segmentations);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("open_view", true, true), iDGenerator.GetCurrentViewId(), null, iDGenerator.GetCurrentViewId(), null, TestUtility.TestTimeMetrics());

            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.IsFalse(model.Segmentation.ContainsKey("key2"));
            Assert.IsFalse(model.Segmentation.ContainsKey(""));

            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("open_view_2", true, false), iDGenerator.GetCurrentViewId(), iDGenerator.GetPreviousViewId(), iDGenerator.GetCurrentViewId(), null, TestUtility.TestTimeMetrics());
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
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            TestUtility.ValidateRQEQSize(cly, 3, 0);

            await Countly.Instance.Views.RecordCloseViewAsync("open_view");
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
            IViewModule views = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Dictionary<string, object> action = new Dictionary<string, object>
            {
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
        // "isFirstView" field should be true again after changing the device ID.
        [Test]
        public async void StartField_AfterDeviceIdChangeWithoutMerge()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly cly = Countly.Instance;
            cly.Init(config);
            IViewModule views = cly.Views;
            IViewIDProvider iDGenerator = cly.Views;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(model, 1, null, null, TestUtility.BaseViewTestSegmentation("first_view", true, true), iDGenerator.GetCurrentViewId(), null, iDGenerator.GetCurrentViewId(), null, TestUtility.TestTimeMetrics());

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new device id");
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            await Countly.Instance.Views.RecordOpenViewAsync("second_view_open");
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            Assert.IsFalse(Countly.Instance.Consents.CheckConsent(Consents.Views));
        }
        #endregion

        [SetUp]
        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            TestUtility.TestCleanup();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}