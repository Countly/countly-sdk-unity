using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// interface for SDK users
public interface IViewModule
{
    /// <summary>
    /// Starts a view which would not close automatically
    /// </summary>
    /// <param name="viewName"></param>
    /// <returns></returns>
    string StartView(string viewName);

    /// <summary>
    /// Starts a view which would not close automatically
    /// </summary>
    /// <param name="viewName">name of the view</param>
    /// <param name="viewSegmentation">segmentation that will be added to the view, set 'null' if none should be added</param>
    /// <returns></returns>
    string StartView(string viewName, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Starts a view which would be closed automaticly
    /// </summary>
    /// <param name="viewName"></param>
    /// <returns></returns>
    string StartAutoStoppedView(string viewName);

    /// <summary>
    /// Starts a view which would be closed automaticly
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="viewSegmentation"></param>
    /// <returns></returns>
    string StartAutoStoppedView(string viewName, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Stops a view with the given name if it was open
    /// </summary>
    /// <param name="viewName">name of the view</param>
    void StopViewWithName(string viewName);

    /// <summary>
    /// Stops a view with the given name if it was open
    /// </summary>
    /// <param name="viewName">name of the view</param>
    /// <param name="viewSegmentation">view segmentation</param>
    void StopViewWithName(string viewName, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Stops a view with the given ID if it was open
    /// </summary>
    /// <param name="viewID">ID of the view</param>
    void StopViewWithID(string viewID);

    /// <summary>
    /// Stops a view with the given ID if it was open
    /// </summary>
    /// <param name="viewID">ID of the view</param>
    /// <param name="viewSegmentation">view segmentation</param>
    void StopViewWithID(string viewID, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Pauses a view with the given ID
    /// </summary>
    /// <param name="viewID">ID of the view</param>
    void PauseViewWithID(string viewID);

    /// <summary>
    /// Resumes a view with the given ID
    /// </summary>
    /// <param name="viewID">ID of the view</param>
    void ResumeViewWithID(string viewID);

    /// <summary>
    /// Stops all views and records a segmentation if set
    /// </summary>
    /// <param name="viewSegmentation">view segmentation</param>
    void StopAllViews(Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Set a segmentation to be recorded with all views
    /// </summary>
    /// <param name="viewSegmentation">global view segmentation</param>
    void SetGlobalViewSegmentation(Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Updates the segmentation of a view with view id
    /// </summary>
    /// <param name="viewID">ID of the view</param>
    /// <param name="viewSegmentation">view segmentation</param>
    void AddSegmentationToViewWithID(string viewID, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Updates the segmentation of a view with view name
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="viewSegmentation"></param>
    void AddSegmentationToViewWithName(string viewName, Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Updates the global segmentation
    /// </summary>
    /// <param name="viewSegmentation"></param>
    void UpdateGlobalViewSegmentation(Dictionary<string, object> viewSegmentation);

    /// <summary>
    /// Start tracking a view
    /// </summary>
    /// <param name="name">name of the view</param>
    /// <returns></returns>
    [Obsolete("RecordOpenViewAsync(string name, IDictionary<string, object> segmentation = null) is deprecated, this is going to be removed in the future.")]
    Task RecordOpenViewAsync(string name, IDictionary<string, object> segmentation = null);

    /// <summary>
    /// Stop tracking a view
    /// </summary>
    /// <param name="name of the view"></param>
    /// <returns></returns>
    [Obsolete("RecordCloseViewAsync(string name) is deprecated, this is going to be removed in the future.")]
    Task RecordCloseViewAsync(string name);

    /// <summary>
    /// Reports a particular action with the specified details
    /// </summary>
    /// <param name="type"> type of action</param>
    /// <param name="x">x-coordinate</param>
    /// <param name="y">y-coordinate</param>
    /// <param name="width">width of screen</param>
    /// <param name="height">height of screen</param>
    /// <returns></returns>
    [Obsolete("ReportActionAsync(string type, int x, int y, int width, int height) is deprecated, this is going to be removed in the future.")]
    Task ReportActionAsync(string type, int x, int y, int width, int height);
}