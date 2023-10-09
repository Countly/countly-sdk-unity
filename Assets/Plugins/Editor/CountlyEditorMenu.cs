using UnityEngine;
using UnityEditor;

public class CountlyEditorMenu
{
    [MenuItem("Countly/SDK Document")]
    private static void SDKDocument()
    {
        Application.OpenURL("https://support.count.ly/hc/en-us/articles/360037813851-Unity");
    }
}
