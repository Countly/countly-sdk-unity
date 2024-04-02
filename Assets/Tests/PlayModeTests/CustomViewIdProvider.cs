using System.Collections;
using System.Collections.Generic;
using Plugins.CountlySDK;
using UnityEngine;

/// <summary>
/// CustomViewIdProvider class is for testing purposes
/// </summary>
public class CustomViewIdProvider : ISafeIDGenerator
{
    private int viewCount;
    public string GenerateValue()
    {
        viewCount++;
        return "idv" + viewCount.ToString();
    }
}
