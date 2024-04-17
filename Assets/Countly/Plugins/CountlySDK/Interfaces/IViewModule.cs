using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// interface for SDK users
public interface IViewModule
{
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