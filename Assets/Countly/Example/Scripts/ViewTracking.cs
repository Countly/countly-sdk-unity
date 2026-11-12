using Plugins.CountlySDK;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewTracking : MonoBehaviour
{
    #region Legacy Calls
    public async void RecordViewA()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View A");
    }

    public async void RecordViewAWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "Platform", "Windows"},
            { "Engine", "Unity"}
        };
        await Countly.Instance.Views.RecordOpenViewAsync("View A with segmentation", segments);
    }

    public async void RecordViewB()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View B");
    }

    public async void RecordViewBWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "Musician", "Album"},
            { "Director", "Movie"}
        };
        await Countly.Instance.Views.RecordOpenViewAsync("View B with segmentation", segments);
    }

    public async void RecordViewC()
    {
        await Countly.Instance.Views.RecordOpenViewAsync("View C");
    }

    public async void RecordViewCWithSeg()
    {
        Dictionary<string, object> segments = new Dictionary<string, object>{
            { "Backpack", "12L"},
            { "Handbag", "10L"}
        };
        await Countly.Instance.Views.RecordOpenViewAsync("View C with segmentation", segments);
    }

    public async void RecordCloseViewA()
    {
        await Countly.Instance.Views.RecordCloseViewAsync("View A");
    }

    public async void RecordCloseViewB()
    {
        await Countly.Instance.Views.RecordCloseViewAsync("View B");
    }

    public async void RecordCloseViewC()
    {
        await Countly.Instance.Views.RecordCloseViewAsync("View C");
    }
    #endregion

    #region View Methods
    public void StartView()
    {
        Countly.Instance.Views.StartView(GetUserInput());
    }
    
    public void StartViewWithSegmentation()
    {
        Countly.Instance.Views.StartView(GetUserInput(), CreateRandomSegments());
    }

    public void StartAutoStoppedView()
    {
        Countly.Instance.Views.StartAutoStoppedView(GetUserInput());
    }

    public void StartAutoStoppedViewWithSegmentation()
    {
        Countly.Instance.Views.StartAutoStoppedView(GetUserInput(), CreateRandomSegments());
    }

    public void StopViewWithName()
    {
        Countly.Instance.Views.StopViewWithName(GetUserInput());
    }

    public void StopViewWithNameWithSegmentation()
    {
        Countly.Instance.Views.StopViewWithName(GetUserInput(), CreateRandomSegments());
    }

    public void StopViewWithID()
    {
        Countly.Instance.Views.StopViewWithID(GetUserInput());
    }

    public void StopViewWithIDWithSegmentation()
    {
        Countly.Instance.Views.StopViewWithID(GetUserInput(), CreateRandomSegments());
    }

    public void PauseViewWithID()
    {
        Countly.Instance.Views.PauseViewWithID(GetUserInput());
    }

    public void ResumeViewWithID()
    {
        Countly.Instance.Views.ResumeViewWithID(GetUserInput());
    }

    public void StopAllViews()
    {
        Countly.Instance.Views.StopAllViews(CreateRandomSegments());
    }

    public void SetGlobalViewSegmentation()
    {
        Countly.Instance.Views.SetGlobalViewSegmentation(CreateRandomSegments());
    }

    public void AddSegmentationToViewWithID()
    {
        Countly.Instance.Views.AddSegmentationToViewWithID(GetUserInput(), CreateRandomSegments());
    }

    public void AddSegmentationToViewWithName()
    {
        Countly.Instance.Views.AddSegmentationToViewWithName(GetUserInput(), CreateRandomSegments());
    }

    public void UpdateGlobalViewSegmentation()
    {
        Countly.Instance.Views.UpdateGlobalViewSegmentation(CreateRandomSegments());
    }
    #endregion
    
    #region UtilityMethods
    private static readonly List<string> potentialKeys = new List<string>
    {
        "Platform", "Engine", "Version", "Device", "Country",
        "UserType", "Subscription", "Campaign", "Level", "Score"
    };
    private static readonly List<string> potentialValues = new List<string>
    {
        "Windows", "Unity", "1.0.0", "iPhone", "USA",
        "Guest", "Premium", "SummerSale", "Beginner", "1000"
    };
    private static readonly System.Random random = new System.Random();
    [SerializeField] private InputField inputField;

    private Dictionary<string, object> CreateRandomSegments()
    {
        var dict = new Dictionary<string, object>();
        int numberOfPairs = random.Next(2, 4); // Randomly choose between 2 and 3 pairs

        while (dict.Count < numberOfPairs)
        {
            string key = potentialKeys[random.Next(potentialKeys.Count)];
            string value = potentialValues[random.Next(potentialValues.Count)];

            if (!dict.ContainsKey(key)) // Ensure we don't add duplicate keys
            {
                dict[key] = value;
            }
        }
        return dict;
    }

    public string GetUserInput()
    {
        string eventName;
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            eventName = inputField.text;
        }
        else
        {
            eventName = "DefaultName";
        }
        return eventName;
    }
    #endregion
}