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

package com.github.imkira.unitycountly;

import com.unity3d.player.UnityPlayer;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.pm.PackageInfo;
import android.telephony.TelephonyManager;
import java.util.Locale;

public class UnityCountly {
  public static String getAppVersion() {
    try {
      Context context = getApplicationContext();

      if (context == null) {
        return null;
      }

      String packageName = context.getPackageName();

      if (packageName == null) {
        return null;
      }

      PackageManager packageManager = context.getPackageManager();

      if (packageManager == null) {
        return null;
      }

      PackageInfo info = packageManager.getPackageInfo(packageName, 0);

      if (info == null) {
        return null;
      }

      return info.versionName;
    }
    catch (Exception e) {
      // do nothing
    }

    return null;
  }

  public static String getLocaleDescription() {
    Locale locale = Locale.getDefault();

    if (locale == null) {
      return null;
    }

    return locale.getLanguage() + "_" + locale.getCountry();
  }

  public static String getCarrierName() {
    try {
      Context context = getApplicationContext();

      if (context == null) {
        return null;
      }

      TelephonyManager manager =
        (TelephonyManager)context.getSystemService(Context.TELEPHONY_SERVICE);

      if (manager == null) {
        return null;
      }

      return manager.getNetworkOperatorName();
    }
    catch (Exception e) {
      // do nothing
    }

    return null;
  }

  private static Activity getCurrentActivity() {
    return UnityPlayer.currentActivity;
  }

  private static Context getApplicationContext() {
    Activity activity = getCurrentActivity();

    if (activity == null) {
      return null;
    }

    return  activity.getApplicationContext();
  }
}
