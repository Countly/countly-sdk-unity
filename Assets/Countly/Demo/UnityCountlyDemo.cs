using UnityEngine;
using System.Collections.Generic;
using Countly;

public class UnityCountlyDemo : MonoBehaviour
{

  private void Awake()
  {
		//CountlyManager.Init("ac302c5fa092565c034108c09cb2c3c315be2233");
  }

  public void EmitPurchase()
  {
    double price = 100;
    CountlyManager.Emit("purchase", 1, price,
      new Dictionary<string, string>()
      {
        {"purchase_id", "product01"},
      });
  }

  public void EmitCrazyEvent()
  {
    CountlyManager.Emit("UTF8こんにちはWorld", 1, 10.25,
      new Dictionary<string, string>()
      {
        {"demo1", "demo2"},
        {"demo3", "Handles UTF8-テスト JSON\"\nstrings"},
        {"demo4", "1"}
      });
  }

  private void OnGUI()
  {
    Rect rect;

    rect = new Rect(10, Screen.height - 20, Screen.width - 20, 150);
	GUILayout.BeginArea(rect);
	  GUILayout.BeginHorizontal();
        if (GUILayout.Button("Emit purchase event"))
        {
          Debug.Log("Emitting purchase event...");

          EmitPurchase();
        }


        if (GUILayout.Button("Emit crazy event"))
        {
          Debug.Log("Emitting crazy event...");

          EmitCrazyEvent();
        }
	  GUILayout.EndHorizontal();
	GUILayout.EndArea();

		GUILayout.BeginVertical();
		CountlyManager.Instance.userProfile.name = GUILayout.TextField(CountlyManager.Instance.userProfile.name);
		CountlyManager.Instance.userProfile.username = GUILayout.TextField(CountlyManager.Instance.userProfile.username);
		CountlyManager.Instance.userProfile.byear = GUILayout.TextField(CountlyManager.Instance.userProfile.byear);
		  if (GUILayout.Button("Send Profile")) {
			CountlyManager.SendProfile();
		  }
			
			if (GUILayout.Button("Generate crash report")) {
			  if (!CrashReporter.fetchReports()) {
				CrashReporter.reports.Add(new CrashReporter.CountlyCrashReport("Test report"));
				CountlyManager.SendReports();
			  }
	        }
		GUILayout.EndVertical();

  }
}
