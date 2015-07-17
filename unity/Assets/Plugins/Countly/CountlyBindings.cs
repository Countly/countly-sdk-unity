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
using System;
using System.Runtime.InteropServices;

namespace Countly
{
  public class Bindings : MonoBehaviour
  {
#if !UNITY_EDITOR && UNITY_IPHONE
    [DllImport("__Internal")]
      private static extern string _CountlyGetAppVersion();

    [DllImport("__Internal")]
      private static extern string _CountlyGetLocaleDescription();

    [DllImport("__Internal")]
      private static extern string _CountlyGetCarrierName();

#elif !UNITY_EDITOR && UNITY_ANDROID

    private static AndroidJavaClass _countly = null;
    private static AndroidJavaClass _Countly
    {
      get
      {
        if (_countly == null)
        {
          _countly =
            new AndroidJavaClass("com.github.imkira.unitycountly.UnityCountly");
        }
        return _countly;
      }
    }

    private static string _CountlyGetAppVersion()
    {
      return _Countly.CallStatic<string>("getAppVersion");
    }

    private static string _CountlyGetLocaleDescription()
    {
      return _Countly.CallStatic<string>("getLocaleDescription");
    }

    private static string _CountlyGetCarrierName()
    {
      return _Countly.CallStatic<string>("getCarrierName");
    }

#else

    private static string _CountlyGetAppVersion()
    {
      return null;
    }

    private static string _CountlyGetLocaleDescription()
    {
      return null;
    }

    private static string _CountlyGetCarrierName()
    {
      return null;
    }

#endif

    public static string GetAppVersion()
    {
      string version = _CountlyGetAppVersion();

      if (string.IsNullOrEmpty(version) == true)
      {
        version = null;
      }

      return version;
    }

    public static string GetLocaleDescription()
    {
      string description = _CountlyGetLocaleDescription();

      if (string.IsNullOrEmpty(description) == true)
      {
        description = Application.systemLanguage.ToString();
      }

      return description;
    }

    public static string GetCarrierName()
    {
      string carrier = _CountlyGetCarrierName();

      if (string.IsNullOrEmpty(carrier) == true)
      {
        carrier = null;
      }

      return carrier;
    }
  }
}
