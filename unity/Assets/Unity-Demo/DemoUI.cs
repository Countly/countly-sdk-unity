using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Countly;

public class DemoUI : MonoBehaviour {

	int id = 0;
	Vector2 scrollPos;
	Profile profile;
	Dictionary<string, string> segmentation;
	string key = "";
	int count = 0;
	double price = 0;
	string newKey = "";

	void Start() {
		segmentation = new Dictionary<string, string>();
		profile = CountlyManager.Instance.GetProfile(); //Get link to user profile 

		profile.custom.Add("Surname", "Smith");			
		profile.custom.Add("Additional info", "Any text here");
		//Here we add custom values to user profile
	}

	void OnGUI() {
	GUILayout.BeginArea(new Rect(20, 20, Screen.width/4-20, Screen.height-40));
		if (GUILayout.Button("Sessions")) id = 0;
		if (GUILayout.Button("Profile")) id = 1;
		if (GUILayout.Button("Events")) id = 2;
		if (GUILayout.Button("Reports")) id = 3;
	GUILayout.EndArea();
	
		GUILayout.BeginArea(new Rect(Screen.width/4, 20, Screen.width*.75f-20, Screen.height-40));
	  scrollPos = GUILayout.BeginScrollView(scrollPos);		
	  switch (id) {
	    case 0:
	  		GUILayout.Label("Sessions");
			GUILayout.Label("By default, Countly manages sessions automatically. You just have to specify your app host and key.\n If you want to initialize sessions manually, you can call CountlyManager.Init(\"your key\") from any of your classes.");
		break;
	    case 1:
			GUILayout.Label("Profile");
			GUILayout.Label("Here's the example of some profile settings.");
				//Display some of profile default values
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name");
			profile.name = GUILayout.TextField(profile.name); 
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Birth Year");
			profile.byear = GUILayout.TextField(profile.byear);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Gender");
			profile.gender = GUILayout.TextField(profile.gender);
			GUILayout.EndHorizontal();
			GUILayout.Label("Custom values:");
				//Display user defined values
			LayoutKeys(profile.custom);	
			GUILayout.Label("These are defined by user.");
			GUILayout.Label("If you leave any of the values blank, they will not be sent.");
			if (GUILayout.Button("Send profile")) {
				CountlyManager.Instance.SendProfile(); // use this to send profile to server
			}
		break;
	    case 2:
			GUILayout.Label("Events");
			GUILayout.Label("Using Countly event sistem, you can inform server about purchases or any other actions performed by user.");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Key");
			key = GUILayout.TextField(key);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("Count: {0}", count));
			count = (int)GUILayout.HorizontalSlider(count, 1, 5);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("Price: {0}", price));
			price = double.Parse(GUILayout.TextField(price.ToString("0.000")));
			GUILayout.EndHorizontal();
			GUILayout.Label("Segmentation:");
			LayoutKeys(segmentation);
			if (segmentation.Count < 5 ) {
				newKey = GUILayout.TextField(newKey);
				if (newKey != "" && GUILayout.Button(string.Format("Add key: {0}", newKey)))
				segmentation.Add(newKey,"");
			}
				GUILayout.Label("All the parameters except event key are optional.");
			if (GUILayout.Button("Send event")) {
				CountlyManager.Emit(key, count, price, segmentation); //Send the event with selected parameters to server
			}
			
		break;
	    case 3:
			GUILayout.Label("Reports");
			GUILayout.Label("Countly will automatically send latest crash report logged by unity on start of each session. \bHowever, you can add more info to reports by generating them yourself. Here's an example report.");
			if (CrashReporter.reports.Count > 0) {
			LayoutKeys(CrashReporter.reports[0].parameters);
			GUILayout.Label("Crash reports can also include user-defined keys.");
			LayoutKeys(CrashReporter.reports[0].custom);
			GUILayout.Label("All parameters except _error are optional.");
			if (CrashReporter.reports[0].custom.Count < 5 ) {
				newKey = GUILayout.TextField(newKey);
				if (newKey != "" && GUILayout.Button(string.Format("Add key: {0}", newKey)))
					CrashReporter.reports[0].custom.Add(newKey,"");
			}
				if (GUILayout.Button("Send report")) {
					CrashReporter.SendLastReport(); //Sends the last available report to the server
				}
			}
			else if (GUILayout.Button("Create report")) {
				CrashReporter.reports.Add(new CrashReporter.CountlyCrashReport("Error")); // Manually creating a report with error value "Error"
			}
		break;
	  }
	  GUILayout.EndScrollView();
	GUILayout.EndArea();
				

	}

	bool LayoutKeys(Dictionary<string, string> parameters) {
		if (parameters == null || parameters.Keys.Count == 0) {
			return false;
		}
	  string[] keys = new string[parameters.Keys.Count];
	  parameters.Keys.CopyTo(keys, 0);
	  
	  for (int i = 0; i < keys.Length; i++) {
		GUILayout.BeginHorizontal();
		GUILayout.Label(keys[i]);
		parameters[keys[i]] = GUILayout.TextField(parameters[keys[i]]);
		GUILayout.EndHorizontal();
	  }
		return true;
	}
	
}
