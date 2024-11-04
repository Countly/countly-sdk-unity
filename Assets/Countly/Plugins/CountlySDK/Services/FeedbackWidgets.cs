using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Enums;

namespace Plugins.CountlySDK.Services
{
    public class FeedbackWidgets : AbstractBaseService
    {
        public class CountlyFeedbackWidget
        {
            public string widgetId;
            public FeedbackWidgetType type;
            public string name;
            public string[] tags;
        }
        internal readonly Countly _cly;
        public delegate void RetrieveFeedbackWidgets(List<CountlyFeedbackWidget> retrievedWidgets, string error);
        internal FeedbackWidgets(Countly countly, CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[FeedbackWidgets] Initializing.");
            _cly = countly;
        }

        #region Public API
        /// <summary>
        /// Returns the list of available feedback widgets for the current device id
        /// </summary>
        public void GetAvailableFeedbackWidgets(RetrieveFeedbackWidgets callback)
        {
            lock (_cly)
            {
                Log.Debug("[FeedbackWidgets] Trying to retrieve the available feedback widget list");
                GetAvailableFeedbackWidgetsInternal(callback);
            }
        }

        /// <summary>
        /// Download the data for a specific widget.
        /// When requesting this data, it will count as a shown widget (will increment that "shown" count in the dashboard)
        /// </summary>
        public void GetFeedbackWidgetData()
        {

        }

        /// <summary>
        /// Present a chosen feedback widget in an alert dialog
        /// </summary>
        public void PresentFeedbackWidget()
        {

        }
        #endregion

        private void GetAvailableFeedbackWidgetsInternal(RetrieveFeedbackWidgets callback)
        {
            // Implementation to retrieve the feedback widgets and call the callback with results.
            // Example:
            List<CountlyFeedbackWidget> widgets = new List<CountlyFeedbackWidget>();
            string error = null; // Or an actual error message if something goes wrong

            // Simulate some widget retrieval logic
            // ...

            callback?.Invoke(widgets, error); // Invoke the callback with results
        }
    }
}