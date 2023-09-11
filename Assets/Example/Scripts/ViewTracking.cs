using Notifications;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewTracking : MonoBehaviour
{
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         SceneManager.LoadScene(sceneBuildIndex: 0);
    //     }
    // }

    public async void RecordViewA()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View A");

    }

    public async void RecordViewAWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "a", "12"},
            { "b", "10"}
        };

        await Countly.Instance.Views.RecordOpenViewAsync("View A with segmentation", segmentation: segments);

    }

    public async void RecordViewB()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View B");

    }

    public async void RecordViewBWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "a", "12"},
            { "b", "10"}
        };

        await Countly.Instance.Views.RecordOpenViewAsync("View B with segmentation", segmentation: segments);

    }

    public async void RecordViewC()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View C");

    }

    public async void RecordViewCWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "a", "12"},
            { "b", "10"}
        };

        await Countly.Instance.Views.RecordOpenViewAsync("View C with segmentation", segmentation: segments);

    }


}
