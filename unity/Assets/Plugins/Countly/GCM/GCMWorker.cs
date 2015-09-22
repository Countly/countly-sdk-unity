using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Countly;

public class GCMWorker {
	private Dictionary<string, IGCM> gcmDict = new Dictionary<string, IGCM>();
	private LogListener logListener;

	protected Queue<string> logQ;
	protected bool loggingEnabled;

	public GCMWorker() {
		gcmDict = new Dictionary<string, IGCM>();
		gcmDict.Add("android", new AndroidGCM());
		gcmDict.Add ("ios", new iOSGCM()); 
		gcmDict.Add ("default", new StubGCM());
		
	
		logQ = new Queue<string>();
	}

	public GCMWorker SetLogListener(LogListener logListener) {
		this.logListener = logListener;
		FlushLog();
		return this;
	}

	/**
	* You should call this method to obtain GCM register id
	*/
	public void Init(string projectId) {
		getPlatformDependentGCM().Init(projectId);
		
		Log ("Init GCM on " + GetOS() + " operating system");
	}

	public void SetLoggingEnabled(bool enabled) {
		this.loggingEnabled = enabled;
		getPlatformDependentGCM().SetLoggingEnabled(enabled);
	}

	private IGCM getPlatformDependentGCM() {
		return gcmDict[GetOS ()];
	}

	public string GetOS() {
		string osName = SystemInfo.operatingSystem;
		if (osName.ToLower().Contains ("android")) {
			return "android";
		} else if (osName.ToLower().Contains ("ios")) {
			return "ios";
		} else {
			return "default";
		}
	}

	private void Log(string message) {
		if (!loggingEnabled) {
			return;
		}

		if(logQ.Count < 100) {
			logQ.Enqueue(message);
		}
	
		FlushLog ();
	}
	
	private void FlushLog() {
		if (logListener != null) {
			while(logQ.Count > 0) {
				logListener.log (logQ.Dequeue());
			}
		}
	}
}
