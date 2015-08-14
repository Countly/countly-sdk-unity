/*
 * Copyright (c) 2014 Mario Freitas (imkira@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Countly
{
  public interface LogListener {
		void log(string logText);
  }
  
	public class Manager : MonoBehaviour
  {
    public string appHost = "https://cloud.count.ly";
    public string appKey;
	public string appVersion = "1.0";
    public bool allowDebug = false;
    public float updateInterval = 60f;
    public int eventSendThreshold = 10;
    public int queueLimit = 1024;
	public int maxRetries = 5;
    public bool queueUsesStorage = true;
	public bool manualReports = false;
	public Profile userProfile;

	public bool updateDataOnResume = false;

    public const string SDK_VERSION = "2.0";

    protected DeviceInfo _deviceInfo = null;

    protected bool _isReady = false;
    protected bool _isRunning = false;
    protected bool _isSuspended = true;
    protected double _sessionLastSentAt = 0.0;
    protected double _unsentSessionLength = 0f;
	public bool canReceivePush = false;
	public string[] sender_IDs;

    protected StringBuilder _connectionStringBuilder = null;
    protected bool _isProcessingConnection = false;
    protected Queue _connectionQueue = null;
    protected Queue ConnectionQueue
    {
      get
      {
        if (_connectionQueue == null)
        {
          _connectionQueue = new Queue(128, queueLimit, queueUsesStorage);
        }
        return _connectionQueue;
      }
    }

	protected LogListener logListener;


    protected StringBuilder _eventStringBuilder = null;
    protected List<Event> _eventQueue = null;
	protected Queue logQ;
    protected List<Event> EventQueue
    {
      get
      {
        if (_eventQueue == null)
        {
          _eventQueue = new List<Event>(16);
        }
        return _eventQueue;
      }
    }

	public void setLogListener(LogListener logListener) {
		this.logListener = logListener;
	}

    public void Init(string appKey)
    {
	  Log ("Start init...");
      if (string.IsNullOrEmpty(appKey) == true)
      {
		Log ("Empty app key, exit from init");
        return;
      }

	  if (canReceivePush) {
				GCM.SetMessageCallback((Dictionary<string, object> table) => {
					string[] keys = new string[1024];
					table.Keys.CopyTo(keys, 0);
					CountlyManager.Emit("[CLY]_push_open", 1, new Dictionary<string, string>() {
						{"i", table[keys[1]].ToString()}
					});});
	  }
	
	 
      this.appKey = appKey;

      if ((_isRunning == true) ||
          (_isReady == false))
      {
		Log ("Exit from init. isRunning & isReady states: " + _isRunning + ", " + _isReady);
        return;
      }

      Log("Initialize: " + appKey);

      _isRunning = true;
      Resume();
	  
      StartCoroutine(RunTimer());
    }


    public void RecordEvent(Event e)
    {
      bool wasEmpty = (ConnectionQueue.Count <= 0);

      EventQueue.Add(e);
      FlushEvents(eventSendThreshold);

      if (wasEmpty == true)
      {
        ProcessConnectionQueue();
      }
    }

#region Unity Methods
    protected void Start()
    {
	  logQ = new Queue(128, 128, false);

	  Log ("Start Countly Manager instance");

	  if (canReceivePush) {
		GCM.Initialize();
	  }
	  CrashReporter.Init();

      _isReady = true;
      Init(appKey);
    }

    protected void OnApplicationPause(bool pause)
    {
      if (_isRunning == false)
      {
        return;
      }

      if (pause == true)
      {
        Log("OnApplicationPause -> Background");
        Suspend();
      }
      else
      {
        Log("OnApplicationPause -> Foreground");
        Resume();
      }
    }

    protected void OnApplicationQuit()
    {
      if (_isRunning == false)
      {
        return;
      }

      Log("OnApplicationQuit");
      Suspend();
    }
#endregion

#region Session Methods
    protected void BeginSession()
    {
		Log ("Start session");
	  if (canReceivePush) {
		GCM.Register(sender_IDs);		
	  }

      StringBuilder builder = InitConnectionDataStringBuilder();

      // compute metrics
	  _deviceInfo.JSONSerializeMetrics(builder);
      string metricsString = builder.ToString();

			builder = InitConnectionData(_deviceInfo);

      builder.Append("&sdk_version=");
      AppendConnectionData(builder, SDK_VERSION);

      builder.Append("&begin_session=1");

	  if (canReceivePush) {
		builder.Append("&token_session=1");
		builder.Append("&test_mode=0");
		builder.Append("&test_token="+GCM.GetRegistrationId());
	  }

      builder.Append("&metrics=");
	  AppendConnectionData(builder, metricsString);

      ConnectionQueue.Enqueue(builder.ToString());
      ProcessConnectionQueue();
    }

	public void UpdateProfile() {
		StringBuilder builder = InitConnectionData(_deviceInfo);

		builder.Append("&user_details=");
		AppendConnectionData(builder, userProfile.JSONSerializeProfile().ToString());
		
		ConnectionQueue.Enqueue(builder.ToString());
		ProcessConnectionQueue();
	}

	public void Attribute(string campaign_id) {
		StringBuilder builder = new StringBuilder(1024);
			
		builder.Append("at/"+campaign_id);
			Log (builder.ToString());
			ConnectionQueue.Enqueue(builder.ToString());
			ProcessConnectionQueue(true);
	}

	public void SendReport(int id) {
		SendReportWithoutCoroutineCall(id);
		ProcessConnectionQueue();
	}

	public void SendReportWithoutCoroutineCall(int id) {
		if (CrashReporter.reports == null || CrashReporter.reports.Count == 0) {
			Log("No crash reports found");
			return;
		}

		StringBuilder builder = InitConnectionData(_deviceInfo);
		
		builder.Append("&crash=");
		string report = CrashReporter.JSONSerializeReport(CrashReporter.reports[id]).ToString();
		AppendConnectionData(builder, report);
		
		ConnectionQueue.Enqueue(builder.ToString());
	}
	
	
	protected void UpdateSession(long duration)
    {
	  Log ("Update session");
      StringBuilder builder = InitConnectionData(_deviceInfo);

      builder.Append("&session_duration=");
      AppendConnectionData(builder, duration.ToString());

      ConnectionQueue.Enqueue(builder.ToString());
			ProcessConnectionQueue();
    }

    protected void EndSession(long duration)
    {
	  Log ("End session");
			StringBuilder builder = InitConnectionData(_deviceInfo);

      builder.Append("&end_session=1");

      builder.Append("&session_duration=");
      AppendConnectionData(builder, duration.ToString());
	  
	  Log ("Requesting session end");

	  try {
		WebRequest www = WebRequest.Create(appHost + "/i?" +builder.ToString());
	    www.GetResponse().Close();
	  }
	  catch (System.Exception e) {
	    Log (string.Format("Request failed: {0}", e));
	  }
    }

    protected void RecordEvents(List<Event> events)
    {
	  StringBuilder builder = InitConnectionData(_deviceInfo);

      builder.Append("&events=");
      string eventsString = JSONSerializeEvents(events);
      AppendConnectionData(builder, eventsString);

      ConnectionQueue.Enqueue(builder.ToString());
    }

#endregion

    protected IEnumerator RunTimer()
    {
      while (true)
      {
        yield return new WaitForSeconds(updateInterval / 4); //it's better to do 4 small tasks than one big task

        if (_isSuspended == true)
        {
          continue;
        }

        // device info may have changed
        UpdateDeviceInfo();
		yield return new WaitForSeconds(updateInterval / 4);

		//check and send any pending crash reports
		if (!manualReports && CrashReporter.fetchReports()) {
			CountlyManager.SendReports();
		}
		yield return new WaitForSeconds(updateInterval / 4);
		
        // record any pending events
        FlushEvents(0);
		yield return new WaitForSeconds(updateInterval / 4);

        long duration = TrackSessionLength();
        UpdateSession(duration);
      }
    }

    protected void Resume()
    {
      // already in unsuspeded state?
      if (_isSuspended == false)
      {
		Log ("In suspended state, exit from Resume()");
        return;
      }

      Log("Resuming...");

      _isSuspended = false;
      _sessionLastSentAt = Utils.GetCurrentTime();

	  UpdateDeviceInfo();	
      BeginSession();
		
    }

    protected void Suspend()
    {
      // already in suspended state?
      if (_isSuspended == true)
      {
        return;
      }

      Log("Suspending...");

      _isSuspended = true;
	  if (canReceivePush) {
		GCM.Unregister();
	  }

      // device info may have changed
      UpdateDeviceInfo();

      // record any pending events
      FlushEvents(0);

      long duration = TrackSessionLength();
      EndSession(duration);
    }

#region Utility Methods
	public void ProcessConnectionQueue(bool request = false)
	{
	  if ((_isProcessingConnection == true) ||
	      (ConnectionQueue.Count <= 0))
      {
	    return;
	  }
			
	  _isProcessingConnection = true;
	  StartCoroutine(_ProcessConnectionQueue(request));
	}

    protected IEnumerator _ProcessConnectionQueue(bool request)
    {
	  Log("Start send requests");
	  int retry = 0;
      while (ConnectionQueue.Count > 0)
      {
        string data = ConnectionQueue.Peek();
		string urlString;
		
		if (!request) {
        	urlString = appHost + "/i?" + data;
		} else {
			urlString = appHost + "/" + data;
		}

        Log("Request started: <" + WWW.UnEscapeURL(urlString) + ">");

        WWW www = new WWW(urlString)
        {
          threadPriority = ThreadPriority.Low
        };

        yield return www;

        if (string.IsNullOrEmpty(www.error) == false && retry < maxRetries)
        {
          Log("Request failed: " + www.error);
		  Log ("Wait 5 seconds before try again");
		  yield return new WaitForSeconds(5f); //wait 5 seconds before try to send analytics again
		  retry++;
        }
		else {
		  ConnectionQueue.Dequeue();
		  if (retry >=maxRetries) {
		    Log(string.Format ("Request failed after {0} retries", retry));
		  }
		  else {
			Log("Request successful: <" + www.text + ">");
		  }
		  retry = 0;
	    }

		if (ConnectionQueue.Count > 0) { //if we have more requests
			Log ("Wait 0.2 sec before exit");
			yield return new WaitForSeconds(0.2f); //don't allow to send more than 5 request per second
		}
	  }

      _isProcessingConnection = false;
		Log ("End send requests");
    }

    protected DeviceInfo UpdateDeviceInfo()
    {
		try {
			if (_deviceInfo == null) {
				_deviceInfo = new DeviceInfo();
			}
			_deviceInfo.Update();
			Log ("Succesfully updated device info");
			return _deviceInfo;
		} catch(System.Exception ex) {
			Log ("Error during obtaining device info: " + ex.StackTrace.ToString());
		}
     return null;
    }

    protected void FlushEvents(int threshold)
    {
      List<Event> eventQueue = EventQueue;


      // satisfy minimum number of eventQueue
      if ((eventQueue.Count <= 0) ||
          (eventQueue.Count < threshold))
      {
        return;
      }

      RecordEvents(eventQueue);
      eventQueue.Clear();
    }

    protected long TrackSessionLength()
    {
      double now = Utils.GetCurrentTime();

      if (now > _sessionLastSentAt)
      {
        _unsentSessionLength += now - _sessionLastSentAt;
      }

      // duration should be integer
      long duration = (long)_unsentSessionLength;

      // sanity check
      if (duration < 0)
      {
        duration = 0;
      }

      // keep decimal part
      _unsentSessionLength -= duration;

      return duration;
    }

    protected StringBuilder InitConnectionDataStringBuilder()
    {
      if (_connectionStringBuilder == null)
      {
        _connectionStringBuilder = new StringBuilder(1024);
      }
      else
      {
        _connectionStringBuilder.Length = 0;
      }

      return _connectionStringBuilder;
    }

    protected StringBuilder InitConnectionData(DeviceInfo info)
    {
      StringBuilder builder = InitConnectionDataStringBuilder();

      builder.Append("app_key=");
      AppendConnectionData(builder, appKey);
	
      builder.Append("&device_id=");
      AppendConnectionData(builder, info.UDID);

	  builder.Append("&timestamp=");
			
	  long timestamp = (long)Utils.GetCurrentTime();
	  builder.Append(timestamp);

      return builder;
    }

    protected void AppendConnectionData(StringBuilder builder, string val)
    {
      if (string.IsNullOrEmpty(val) != true)
      {
        builder.Append(Utils.EscapeURL(val));
      }
    }

    protected StringBuilder InitEventStringBuilder()
    {
      if (_eventStringBuilder == null)
      {
        _eventStringBuilder = new StringBuilder(1024);
      }
      else
      {
        _eventStringBuilder.Length = 0;
      }

      return _eventStringBuilder;
    }

    protected string JSONSerializeEvents(List<Event> events)
    {
      StringBuilder builder = InitEventStringBuilder();

      // open array of events
      builder.Append("[");

      bool first = true;

      foreach (Event e in events)
      {
        if (first == true)
        {
          first = false;
        }
        else
        {
          builder.Append(",");
        }

        e.JSONSerialize(builder);
      }

      // close array of events
      builder.Append("]");

      return builder.ToString();
    }

    protected void Log(string str)
    {
      if (allowDebug == true)
      {
        Debug.Log(str);

		logQ.Enqueue(str);
		if (logListener != null) {
			while(logQ.Count > 0) {
				string logItem = logQ.Peek();
				logListener.log(logItem);
				logQ.Dequeue();
			}
			
		}
      }
    }
#endregion
  }
}

public class CountlyManager : Countly.Manager
{
  protected static Countly.Manager _instance = null;
  public static Countly.Manager Instance
  {
    get
    {
      if (_instance == null)
      {
        GameObject singleton = GameObject.Find("CountlyManager");

        if (singleton != null)
        {
          _instance = singleton.GetComponent<Countly.Manager>();
        }
      }

      return _instance;
    }
  }

  protected void Awake()
  {
    if ((_instance != this) && (_instance != null))
    {
      Log("Duplicate manager detected. Destroying...");
      Destroy(gameObject);
      return;
    }

    _instance = this;
    DontDestroyOnLoad(gameObject);
  }

  public static void Attribute(string campaign_id) {
	Instance.Attribute(campaign_id);
  }


  public static Countly.Profile GetProfile() {
    Instance.userProfile = new Countly.Profile();
	Instance.userProfile.Init();
	return Instance.userProfile;
  }

 
  public static void SendProfile() {
    Instance.UpdateProfile();
  }

  public static void SendReports() {
	for (int i = 0; i < Countly.CrashReporter.reports.Count; i++) {
		Instance.SendReportWithoutCoroutineCall(i);
	}
	
	Instance.ProcessConnectionQueue();
	Countly.CrashReporter.Clear();
  }
	
  public static new void Init(string appKey = null)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      if (appKey == null)
      {
        appKey = instance.appKey;
      }
      instance.Init(appKey);
    }
  }

  public static void Emit(string key, long count)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      Countly.Event e = new Countly.Event();

      e.Key = key;
      e.Count = count;

      instance.RecordEvent(e);
    }
  }

  public static void Emit(string key, long count, double sum)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      Countly.Event e = new Countly.Event();

      e.Key = key;
      e.Count = count;
      e.Sum = sum;

      instance.RecordEvent(e);
    }
  }

  public static void Emit(string key, long count,
      Dictionary<string, string> segmentation)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      Countly.Event e = new Countly.Event();

      e.Key = key;
      e.Count = count;
      e.Segmentation = segmentation;

      instance.RecordEvent(e);
    }
  }

  public static void Emit(string key, long count, double sum,
      Dictionary<string, string> segmentation)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      Countly.Event e = new Countly.Event();

      e.Key = key;
      e.Count = count;
      e.Sum = sum;
      e.Segmentation = segmentation;

      instance.RecordEvent(e);
    }
  }


  public static void Emit(Countly.Event e)
  {
    Countly.Manager instance = Instance;

    if (instance != null)
    {
      instance.RecordEvent(e);
    }
  }
}
