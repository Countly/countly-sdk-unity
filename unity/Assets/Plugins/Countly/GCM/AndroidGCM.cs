using UnityEngine;
using System.Collections;

public class AndroidGCM: IGCM {
	private static string ANDROID_GCM_CLASS_NAME = "hupp.tech.countly.android.gcm.plugin.CountlyMessaging";
	
	private AndroidJavaClass gcm;	

	public void Init(string projectNumber) {
		GetGCM().CallStatic("init", projectNumber);
	}

	public void SetLoggingEnabled(bool enabled) {
		GetGCM().CallStatic("setLoggingEnabled", enabled);
	}

	private AndroidJavaClass GetGCM() {
		if (gcm == null) {
			gcm = new AndroidJavaClass(ANDROID_GCM_CLASS_NAME);
		}
		return gcm;
	}
}
