using System;
using System.Threading;
using Plugins.CountlySDK;
using UnityEngine;

public class CountlyMainThreadHandler : MonoBehaviour
{
    private static CountlyMainThreadHandler _instance;
    private Thread mainThread;
    private Action _queuedAction;
    private readonly object lockObject = new object(); // For thread safety

    public static CountlyMainThreadHandler Instance
    {
        get {
            if (_instance == null) {
                GameObject gameObject = Countly.Instance.gameObject;
                _instance = gameObject.AddComponent<CountlyMainThreadHandler>();
            }
            return _instance;
        }
        internal set {
            _instance = value;
        }
    }

    public bool IsMainThread()
    {
        return Thread.CurrentThread.ManagedThreadId == mainThread.ManagedThreadId;
    }

    public void RunOnMainThread(Action action)
    {
        // Check if we are on the main thread
        if (IsMainThread()) {
            action.Invoke();
        } else {
            // Queue the action to be executed on the main thread
            lock (lockObject) {
                _queuedAction = action;
            }
        }
    }

    private void Awake()
    {
        mainThread = Thread.CurrentThread;
    }

    private void Update()
    {
        // Execute any queued action on the main thread
        if (_queuedAction != null) {
            lock (lockObject) {
                _queuedAction.Invoke();
                _queuedAction = null;
            }
        }
    }
}
