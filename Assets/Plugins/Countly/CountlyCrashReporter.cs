using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Countly
{
  public static class CrashReporter {
	
	
    public class CountlyCrashReport {
	public CrashReport error;
	public Dictionary<string, string> parameters;
	public Dictionary<string, string>  custom;

	public CountlyCrashReport(CrashReport unityReport) {
	  error = unityReport;
	  parameters = new Dictionary<string, string>() 
	  {
		{"_os", ""},
		{"_os_version", ""},
		{"_manufacture", ""},
		{"_device", ""},
		{"_resolution", ""},
		{"_app_version", ""},
		{"_cpu", ""},
		{"_opengl", ""},
		{"_rem_current", ""},
		{"_ram_total", ""},
		{"_disk_current", ""},
		{"_disk_total", ""},
        {"_bat", ""},
		{"_orientation", ""},
		{"_root", ""},
		{"_online", ""},
        {"_muted", ""},
		{"_background", ""},
		{"_name", ""},
		{"_error", ""},
		{"_nonfatal", ""},
        {"_logs", ""},
		{"_run", ""}
	  };
      DeviceInfo info = new DeviceInfo();
	  info.Update();
	  parameters["_os"] = info.OSName;
	  parameters["_os_version"] = info.OSVersion;
	  parameters["_app_version"] = CountlyManager.Instance.appVersion;
	  parameters["_error"] = error.text;
	  parameters["_online"] = Application.internetReachability.ToString();
	  parameters["_opengl"] = SystemInfo.graphicsDeviceVersion;
	  parameters["_cpu"] = SystemInfo.processorType;
	  parameters["_device"] = info.Device.ToString();
	  parameters["_resolution"] = info.Resolution;
	  parameters["_ram_total"] = SystemInfo.systemMemorySize.ToString();
	  parameters["_run"] = Time.realtimeSinceStartup.ToString();
	  custom = new Dictionary<string, string>();
	}

	public CountlyCrashReport(string errorText) {
	  parameters = new Dictionary<string, string>() 
	  {
		{"_os", ""},
		{"_os_version", ""},
		{"_manufacture", ""},
		{"_device", ""},
		{"_resolution", ""},
		{"_app_version", ""},
		{"_cpu", ""},
		{"_opengl", ""},
		{"_rem_current", ""},
		{"_ram_total", ""},
		{"_disk_current", ""},
		{"_disk_total", ""},
		{"_bat", ""},
		{"_orientation", ""},
		{"_root", ""},
		{"_online", ""},
		{"_muted", ""},
		{"_background", ""},
		{"_name", ""},
		{"_error", ""},
		{"_nonfatal", ""},
		{"_logs", ""},
		{"_run", ""}
	  };
	  DeviceInfo info = new DeviceInfo();
	  info.Update();
	  parameters["_os"] = info.OSName;
	  parameters["_os_version"] = info.OSVersion;
	  parameters["_app_version"] = CountlyManager.Instance.appVersion;
	  parameters["_error"] = errorText;
	  parameters["_online"] = Application.internetReachability.ToString();
	  parameters["_opengl"] = SystemInfo.graphicsDeviceVersion;
	  parameters["_cpu"] = SystemInfo.processorType;
	  parameters["_device"] = info.Device.ToString();
	  parameters["_resolution"] = info.Resolution;
	  parameters["_ram_total"] = SystemInfo.systemMemorySize.ToString();
	  parameters["_run"] = Time.realtimeSinceStartup.ToString();
	  custom = new Dictionary<string, string>();
	}
  }

	public static Dictionary<string, int> reported;

	public static List<CountlyCrashReport> reports {
	  get {
	    return _reports;
	  }
	  private set {
	    _reports = value;
	  }
	}

    public static void Clear() {
	  reports.Clear();
	}

	public static void Init() {
	  reports = new List<CountlyCrashReport>();
	  reported = new Dictionary<string, int>();
	  Application.logMessageReceived += exceptionHandler; 
	}

	public static void UpdateReports() {
		for (int i = 0; i < reported.Count; i++) {
		  reports[i].custom["_count"] = reported[reports[i].parameters["_logs"]].ToString();
		}
	}

	static List<CountlyCrashReport> _reports;
		
    // Use this for initialization
    public static bool fetchReports () {
	  CrashReport[] fetchedReports = CrashReport.reports;
	  if (fetchedReports.Length > 0) {
		reports = new List<CountlyCrashReport>();

		for (int i = 0; i< fetchedReports.Length; i++) {
		  reports.Add(new CountlyCrashReport(fetchedReports[i]));
		}
		return true;
	  }
	  else return false;
    }

	static void exceptionHandler(string error, string stack, LogType type) {
		switch (type) {
	      case LogType.Error:
		  case LogType.Exception:
			if (reported.ContainsKey(stack)) {
			  reported[stack]++;
			}
			else {
			  reports.Add(new CountlyCrashReport(error));
			  reports[reports.Count-1].parameters["_logs"] = stack;
			  reported.Add(stack, 1);
			}
		  break;
		}
	} 

	public static StringBuilder JSONSerializeReport(CountlyCrashReport report)
	{
	  StringBuilder builder = new StringBuilder();
			// open metrics object
	  builder.Append("{");
			
	  foreach (KeyValuePair<string, string> pair in report.parameters) {
		if (pair.Value != null && pair.Value != "") {
		  builder.Append("\"" + pair.Key + "\":\"" + pair.Value + "\",");
		}
	  }
			
			if (report.custom != null && report.custom.Count > 0) {
				builder.Append("\"custom\":{");
				foreach (KeyValuePair<string, string> pair in report.custom) {
					builder.Append("\"" + pair.Key + "\":\"" + pair.Value + "\",");
					
				}
				builder.Length = builder.Length-1;
				builder.Append("}");
			}
			else {
				builder.Length = builder.Length-1;
			}
			
			
			
			builder.Append("}");
			
			return builder;
	}
	
	// Update is called once per frame
  }
}
