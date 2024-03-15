using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Assets.Tests.PlayModeTests.Scenarios
{
    public class MV_ManualView
    {
        readonly IViewModule views;
        readonly Dictionary<string, object> testSegmentation;
        readonly Countly cly;

        // Constructor to initialize the Countly SDK and views
        public MV_ManualView()
        {
            cly = InitializeCountlySDK();
            views = cly.Views;
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

            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'RecordView', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Empty string values provided as viewID/viewName parameter for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_101_badValues_emptyString()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);

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

            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', 'StopViewWithID', 'AddSegmentationToViewWithID', 'AddSegmentationToViewWithName' in ViewCountlyService
        // Non-existing viewNames and viewIDs provided for each function, with and without segmentation
        // Nothing should crash and nothing should be recorded
        [Test]
        public void MV_102_badValues_nonExistingViews()
        {
            TestUtility.ValidateRQEQSize(cly, 0, 0);
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

            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'StartAutoStoppedView' and 'StopAllViews' functions in ViewCountlyService
        // Starting 3 auto stopping views one after another, waiting 1 sec between and stopping all views in the end
        // Auto stopped views should stop when another view starts, and should record correctly
        [Test]
        public void MV_200B_autoStoppedView_autoClose()
        {

        }

        // 'StartView', 'StartAutoStoppedView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, pauses and resumes viewB and stops both afterwards
        // Both views should behave and record the events correctly
        [Test]
        public void MV_201B_autoStopped_pausedResumed()
        {

        }

        // 'StartView', 'StartAutoStoppedView', 'StopViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts 2 views with StartView and StartAutoStoppedView, after stopping both starts a 3rd one
        // All views should behave and record the events correctly
        [Test]
        public void MV_202B_autoStopped_stopped()
        {

        }

        // 'StartView', 'PauseViewWithID', 'ResumeViewWithID' and 'StopAllViews' functions in ViewCountlyService
        // Starts a view with StartView, pauses it, resumes it and stops it after 1 second
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_PausedResumed()
        {

        }

        // 'StartView', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Starts a view with StartView, stops it with StopViewWithName, starts another view, and stops it with StopViewWithID
        // View events should be recorded correctly
        [Test]
        public void MV_203_startView_stopped()
        {

        }

        // 'RecordOpenViewAsync', 'StartAutoStoppedView', 'StartView', 'PauseViewWithID', 'ResumeViewWithID', 'StopViewWithName', and 'StopViewWithID' functions in ViewCountlyService
        // Testing the behavior when calling these functions with no user consent to record view-related events
        // Expecting that no view events are recorded, as the functions are called without user consent
        [Test]
        public void MV_300A_callingWithNoConsent_legacy()
        {

        }

        [SetUp][TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
