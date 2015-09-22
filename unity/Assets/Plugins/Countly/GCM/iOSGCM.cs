using UnityEngine;
using System.Collections;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;

public class iOSGCM : IGCM {

	

	public void Init(string projectId) {
		UnityEngine.iOS.NotificationServices.RegisterForNotifications(
			NotificationType.Alert | 
			NotificationType.Badge | 
			NotificationType.Sound);
	}

	public void SetLoggingEnabled(bool enabled) {
		
	}
}
