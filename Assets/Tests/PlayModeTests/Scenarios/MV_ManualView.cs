using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using System.Threading;

namespace Assets.Tests.PlayModeTests.Scenarios
{
    public class MV_ManualView
    {
        Dictionary<string, object> testSegmentation;
        Countly cly;

        // Constructor to initialize the Countly SDK and views
        public MV_ManualView()
        {
            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;
            testSegmentation = TestUtility.TestSegmentation();
        }

        // Helper method to initialize the Countly SDK and views
        private Countly InitializeCountlySDK()
        {
            CountlyConfiguration config = TestUtility.CreateViewConfig(new CustomIdProvider());
            Countly.Instance.Init(config);
            return Countly.Instance;
        }

        // All public functions in ViewCountlyService
        // Null values provided as viewID/viewName parameter for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_100_badValues_null()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.AddSegmentationToViewWithID(null, null);
            views.AddSegmentationToViewWithID(null, testSegmentation);

            views.AddSegmentationToViewWithName(null, null);
            views.AddSegmentationToViewWithName(null, testSegmentation);

            views.PauseViewWithID(null);

            views.RecordCloseViewAsync(null);

            views.RecordOpenViewAsync(null, null);
            views.RecordOpenViewAsync(null, testSegmentation);

            views.ResumeViewWithID(null);

            views.SetGlobalViewSegmentation(null);
            views.SetGlobalViewSegmentation(testSegmentation);

            views.StartAutoStoppedView(null);
            views.StartAutoStoppedView(null, testSegmentation);

            views.StartView(null);
            views.StartView(null, testSegmentation);

            views.StopAllViews(null);
            views.StopAllViews(testSegmentation);

            views.StopViewWithID(null);
            views.StopViewWithID(null, testSegmentation);

            views.StopViewWithName(null);
            views.StopViewWithName(null, testSegmentation);

            views.UpdateGlobalViewSegmentation(null);
            views.UpdateGlobalViewSegmentation(testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordView', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Empty string values provided as viewID/viewName parameter for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_101_badValues_emptyString()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.AddSegmentationToViewWithID("", null);
            views.AddSegmentationToViewWithID("", testSegmentation);

            views.AddSegmentationToViewWithName("", null);
            views.AddSegmentationToViewWithName("", testSegmentation);

            views.PauseViewWithID("");

            views.RecordCloseViewAsync("");

            views.RecordOpenViewAsync("", null);
            views.RecordOpenViewAsync("", testSegmentation);

            views.ResumeViewWithID("");

            views.StartAutoStoppedView("");
            views.StartAutoStoppedView("", testSegmentation);

            views.StartView("");
            views.StartView("", testSegmentation);

            views.StopViewWithID("");
            views.StopViewWithID("", testSegmentation);

            views.StopViewWithName("");
            views.StopViewWithName("", testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Non-existing viewNames and viewIDs provided for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_102_badValues_nonExistingViews()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string name = "non existing view";

            views.AddSegmentationToViewWithID(name, null);
            views.AddSegmentationToViewWithID(name, testSegmentation);

            views.AddSegmentationToViewWithName(name, null);
            views.AddSegmentationToViewWithName(name, testSegmentation);

            views.PauseViewWithID(name);

            views.ResumeViewWithID(name);

            views.StopViewWithID(name);
            views.StopViewWithID(name, testSegmentation);

            views.StopViewWithName(name);
            views.StopViewWithName(name, testSegmentation);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartAutoStoppedView' and 'StopAllViews' functions in ViewCountlyService
        // Starting 3 auto stopping views one after another, waiting 1 sec between and stopping all views in the end
        // Auto stopped views should stop when another view starts, and should record correctly
        [Test]
        public void MV_200B_autoStoppedView_autoClose()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;
            
            string viewIdA = views.StartAutoStoppedView("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewA = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewA, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewA", true, true), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            string viewIdB = views.StartAutoStoppedView("viewB");

            CountlyEventModel viewAEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 1); // view B start event should be remaining
            CountlyEventModel viewB = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewB, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewB", true, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            string viewIdC = views.StartView("viewC");
            CountlyEventModel viewBEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewBEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewB", false, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 1); // view C start event should be remaining
            CountlyEventModel viewC = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewC, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewC", true, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.StopAllViews(null);
            CountlyEventModel viewCEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewCEnd, 1, 0, 0, TestUtility.BaseViewTestSegmentation("viewC", false, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView', 'StartAutoStoppedView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, pauses and resumes viewB and stops both afterwards
        // Both views should behave and record the events correctly
        [Test]
        public void MV_201B_autoStopped_pausedResumed()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);
            
            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            string viewIdA = views.StartView("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewA = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewA, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewA", true, true), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewIdB = views.StartAutoStoppedView("viewB");
            CountlyEventModel viewB = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewB, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewB", true, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.PauseViewWithID(viewIdB);
            CountlyEventModel viewBPause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewBPause, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewB", false, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.ResumeViewWithID(viewIdB);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 2);

            CountlyEventModel viewAEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAEnd, 1, 0, 2, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 1); 
            CountlyEventModel viewBEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewBEnd, 1, 0, 0, TestUtility.BaseViewTestSegmentation("viewB", false, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView', 'StartAutoStoppedView', 'StopViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, after stopping both starts a 3rd one
        // All views should behave and record the events correctly
        [Test]
        public void MV_202B_autoStopped_stopped()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);
            
            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            string viewIdA = views.StartAutoStoppedView("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewA = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewA, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewA", true, true), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopViewWithName("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewAEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewIdB = views.StartAutoStoppedView("viewB");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewB = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewB, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewB", true, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopViewWithID(viewIdB);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewBEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewBEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewB", false, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewIdC = views.StartAutoStoppedView("viewC");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewC = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewC, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewC", true, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewCEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewCEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewC", false, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts a view with StartView, pauses it, resumes it and stops it after 1 second
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_PausedResumed()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);            
            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            string viewIdA = views.StartView("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewA = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewA, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewA", true, true), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            
            Thread.Sleep(1000);
            
            views.PauseViewWithID(viewIdA);

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewAPause = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAPause, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            
            Thread.Sleep(1000);
            
            views.ResumeViewWithID(viewIdA);
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            
            Thread.Sleep(1000);
            
            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewAEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'StartView', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Starts a view with StartView, stops it with StopViewWithName, starts another view, and stops it with StopViewWithID
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_stopped()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);            
            cly = InitializeCountlySDK();
            IViewModule views = cly.Views;

            string viewIdA = views.StartView("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewA = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewA, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewA", true, true), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            
            Thread.Sleep(1000);

            views.StopViewWithName("viewA");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewAEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewAEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewA", false, false), viewIdA, "", null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewIdB = views.StartView("viewB");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewB = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewB, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewB", true, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopViewWithID(viewIdB);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewBEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewBEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewB", false, false), viewIdB, viewIdA, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            string viewIdC = views.StartView("viewC");
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewC = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewC, 1, 0, null, TestUtility.BaseViewTestSegmentation("viewC", true, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            Thread.Sleep(1000);

            views.StopAllViews(null);
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            CountlyEventModel viewCEnd = cly.Events._eventRepo.Dequeue();
            TestUtility.ViewEventValidator(viewCEnd, 1, 0, 1, TestUtility.BaseViewTestSegmentation("viewC", false, false), viewIdC, viewIdB, null, null, TestUtility.TestTimeMetrics());
            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        // 'RecordOpenViewAsync', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Testing the behavior when calling these functions with no user consent to record view-related events
        // Expecting that no view events are recorded, as the functions are called without user consent
        [Test]
        public void MV_300A_callingWithNoConsent_legacy()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);
            cly = Countly.Instance;
            cly.Init(configuration);
            
            IViewModule views = cly.Views;
            testSegmentation = TestUtility.TestSegmentation();

            TestUtility.ValidateRQEQSize(cly, 2, 0);

            views.RecordOpenViewAsync("viewA");
            views.RecordOpenViewAsync("viewB", testSegmentation);
            string viewIdC = views.StartAutoStoppedView("viewC");
            string viewIdD = views.StartAutoStoppedView("viewD", testSegmentation);
            string viewIdE = views.StartView("viewE");
            string viewIdF = views.StartView("viewF", testSegmentation);

            views.PauseViewWithID(viewIdC);
            views.PauseViewWithID(viewIdD);
            views.PauseViewWithID(viewIdE);
            views.PauseViewWithID(viewIdF);

            views.ResumeViewWithID(viewIdC);
            views.ResumeViewWithID(viewIdD);
            views.ResumeViewWithID(viewIdE);
            views.ResumeViewWithID(viewIdF);

            views.StopViewWithName("viewA");
            views.StopViewWithName("viewB");
            views.StopViewWithName("viewC");
            views.StopViewWithName("viewD");
            views.StopViewWithName("viewE");
            views.StopViewWithName("viewF");

            views.StopViewWithID(viewIdC);
            views.StopViewWithID(viewIdD);
            views.StopViewWithID(viewIdE);
            views.StopViewWithID(viewIdF);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
        }

        [SetUp][TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
