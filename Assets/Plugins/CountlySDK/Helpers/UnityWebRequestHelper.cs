using System;
using System.Collections;
using Plugins.CountlySDK.Helpers;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestHelper : MonoBehaviour
{
    private static UnityWebRequestHelper instance;
    bool isCurrentlyProcessing = false;

    // Public property to access the instance
    public static UnityWebRequestHelper Instance
    {
        get {
            if (instance == null) {
                instance = FindObjectOfType<UnityWebRequestHelper>();

                if (instance == null) {
                    GameObject singletonObject = new GameObject("UnityWebRequestHelper");
                    instance = singletonObject.AddComponent<UnityWebRequestHelper>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public void StartRequestRoutine(bool isPostEnabled, string url, string data)
    {
        if (isCurrentlyProcessing) {
            return;
        }

        isCurrentlyProcessing = true;

        if (isPostEnabled) {
            StartCoroutine(PostRoutine(url, data));
        } else {
            StartCoroutine(GetRoutine(url));
        }
    }

    public IEnumerator GetRoutine(string url)
    {
        CountlyResponse countlyResponse = new CountlyResponse();

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;


            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    countlyResponse.ErrorMessage = webRequest.error;
                    isCurrentlyProcessing = false;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    countlyResponse.ErrorMessage = webRequest.error;
                    isCurrentlyProcessing = false;
                    break;
                case UnityWebRequest.Result.Success:
                    int code = (int)webRequest.responseCode;
                    string res = webRequest.downloadHandler.text;
                    countlyResponse.Data = res;
                    countlyResponse.StatusCode = code;
                    countlyResponse.IsSuccess = true;

                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    isCurrentlyProcessing = false;
                    break;
            }
        }
    }

    public IEnumerator PostRoutine(string url, string data)
    {
        CountlyResponse countlyResponse = new CountlyResponse();

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, data)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    countlyResponse.ErrorMessage = webRequest.error;
                    isCurrentlyProcessing = false;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    countlyResponse.ErrorMessage = webRequest.error;
                    isCurrentlyProcessing = false;
                    break;
                case UnityWebRequest.Result.Success:
                    int code = (int)webRequest.responseCode;
                    string res = webRequest.downloadHandler.text;
                    countlyResponse.Data = res;
                    countlyResponse.StatusCode = code;
                    countlyResponse.IsSuccess = true;

                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    isCurrentlyProcessing = false;
                    break;
            }
        }
    }

    // Ensure the instance is destroyed when the game is stopped in the Unity editor
    private void OnApplicationQuit()
    {
        instance = null;
    }
}