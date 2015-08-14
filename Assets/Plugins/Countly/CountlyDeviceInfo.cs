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
using System.Text;

namespace Countly
{
  public class DeviceInfo
  {
    public string UDID {get; set;}
    public string Device {get; set;}
    public string OSName {get; set;}
    public string OSVersion {get; set;}
    public string Carrier {get; set;}
    public string Resolution {get; set;}
    public string Locale {get; set;}
    public string AppVersion {get; set;}
	public string DPI {get;set;}

    protected bool _isInitialized = false;

    public void Update()
    {
      if (_isInitialized == false)
      {
        _isInitialized = true;

        UDID = DetectUDID();

		OSVersion = GetOsVersion();
		Debug.Log("dpi: " + Screen.dpi);

		DPI = GetDPIAsStringValue();

#if UNITY_EDITOR
        Device = "Unity Editor";
        OSName = "Unity Editor";
#elif UNITY_IPHONE
        Device = "iPhone";
        OSName = "iOS";
#elif UNITY_ANDROID
        Device = "Android";
        OSName = "Android";
#elif UNITY_STANDALONE_OSX
        Device = "MAC";
        OSName = "OS X";
#elif UNITY_STANDALONE_WIN
        Device = "PC";
        OSName = "Windows";
#else
        Device = SystemInfo.deviceModel;
        OSName = "Unknown";
#endif

        
        AppVersion = Bindings.GetAppVersion();
      }

      Carrier = Bindings.GetCarrierName();
      Resolution = DetectResolution();
      Locale = Bindings.GetLocaleDescription();
    }

    public void JSONSerializeMetrics(StringBuilder builder)
    {
      // open metrics object
      builder.Append("{");

      builder.Append("\"_device\":");
      Utils.JSONSerializeString(builder, Device);

      builder.Append(",\"_os\":");
      Utils.JSONSerializeString(builder, OSName);

      builder.Append(",\"_os_version\":");
      Utils.JSONSerializeString(builder, OSVersion);

	  builder.Append(",\"_density\":");
      Utils.JSONSerializeString(builder, DPI);

      if (Carrier != null)
      {
        builder.Append(",\"_carrier\":");
        Utils.JSONSerializeString(builder, Carrier);
      }

      builder.Append(",\"_resolution\":");
      Utils.JSONSerializeString(builder, Resolution);

      if (Locale != null)
      {
        builder.Append(",\"_locale\":");
        Utils.JSONSerializeString(builder, Locale);
      }

      if (AppVersion != null)
      {
        builder.Append(",\"_app_version\":");
        Utils.JSONSerializeString(builder, AppVersion);
      }

      // close metrics object
      builder.Append("}");
    }

    public static string DetectUDID()
    {
      return SystemInfo.deviceUniqueIdentifier;
    }

    public static string DetectResolution()
    {
      Resolution resolution = Screen.currentResolution;
      return resolution.width + "x" + resolution.height;
    }

	public static string GetOsVersion() {
		string version = "0";
		string osName = SystemInfo.operatingSystem;
		Debug.Log("os: " + osName);
		string[] parts = osName.Split(' ');
		if (osName.Contains("Mac")) {
			return GetNextTokenAfter(parts, "X");
		} else if (osName.Contains ("Android")) {
			return GetNextTokenAfter (parts, "OS");
		} else if (osName.Contains ("Windows")) {
			return GetNextTokenAfter(parts, "Windows");
		}

		return osName;
	}

	public static string GetDPIAsStringValue() {
		float dpi = Screen.dpi;
		if (dpi <= 120f) {
			return "LDPI";
		} else if (dpi > 120 && dpi <= 160) {
			return "MDPI";
		} else if (dpi > 160 && dpi <= 240) {
			return "HDPI";
		} else if (dpi > 240 && dpi <= 320) {
			return "XHDPI";
		} else if (dpi > 320 && dpi <= 480) {
			return "XXHDPI";
		} else if (dpi > 480) {
			return "XXXHDPI";
		}

		return dpi + "";
	}

	private static string GetNextTokenAfter(string[] items, string token) {
		for(int i = 0; i < items.Length; i++) {
			if (items[i] == token && i < items.Length - 1) {
				return items[i + 1];
			}
		}
		return "unknown";
	}
  }
}
