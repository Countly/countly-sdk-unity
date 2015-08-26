package hupp.tech.countly.android;

import com.unity3d.player.UnityPlayer;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.pm.PackageInfo;
import android.telephony.TelephonyManager;
import java.util.Locale;

public class DeviceInfo {
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
