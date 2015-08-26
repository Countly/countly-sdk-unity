package hupp.tech.countly.android.gcm.plugin;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.SystemClock;
import android.support.v4.content.WakefulBroadcastReceiver;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.gcm.GoogleCloudMessaging;
import com.unity3d.player.UnityPlayer;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

/**
 * Countly Messaging
 *
 */
public class CountlyMessaging extends WakefulBroadcastReceiver {
    public static final String TAG = "UnityCountlyMessaging";

    protected static final String NOTIFICATION_SHOW_DIALOG = "ly.count.android.api.messaging.dialog";
    protected static final String EXTRA_MESSAGE = "ly.count.android.api.messaging.message";

    private static final String EVENT_OPEN    = "[CLY]_push_open";
    private static final String EVENT_ACTION  = "[CLY]_push_action";

    protected static String[] buttonNames = new String[]{"Open", "Review"};

    protected static final int NOTIFICATION_TYPE_UNKNOWN  = 0;
    protected static final int NOTIFICATION_TYPE_MESSAGE  = 1;
    protected static final int NOTIFICATION_TYPE_URL      = 1 << 1;
    protected static final int NOTIFICATION_TYPE_REVIEW   = 1 << 2;

    protected static final int NOTIFICATION_TYPE_SILENT           = 1 << 3;

    protected static final int NOTIFICATION_TYPE_SOUND_DEFAULT    = 1 << 4;
    protected static final int NOTIFICATION_TYPE_SOUND_URI        = 1 << 5;

    private static boolean loggingEnabled = true;

    /**
     * Action for Countly Messaging BroadcastReceiver.
     * Once message is arrived, Countly Messaging will send a broadcast notification with action "APP_PACKAGE_NAME.countly.messaging"
     * to which you can subscribe via BroadcastReceiver.
     */
    public static String BROADCAST_RECEIVER_ACTION_MESSAGE = "ly.count.android.api.messaging.broadcast.message";
    public static String getBroadcastAction (Context context) {
        try {
            ComponentName name = new ComponentName(context, CountlyMessagingService.class);
            Bundle data = context.getPackageManager().getServiceInfo(name, PackageManager.GET_META_DATA).metaData;
            return data.getString("broadcast_action");
        } catch (PackageManager.NameNotFoundException ignored) {
            Log.w(TAG, "Set broadcast_action metadata for CountlyMessagingService in AndroidManifest.xml to receive broadcasts about received messages.");
            return null;
        }
    }


    private static Context context;

    /**
     * Activity used for messages displaying.
     * When message arrives, Countly displays it either as a Notification, or as a AlertDialog. In any case,
     * this activity is used as a final destination.
     */
    private static Class<? extends Activity> activityClass;

    public static void setActivity(Activity activity) {
        setActivity(activity, activity.getClass());
    }

    public static void setActivity(Activity activity, Class<? extends Activity> claz) {
        context = activity.getApplicationContext();
        activityClass = claz == null ? activity.getClass() : claz;
    }
    protected static Context getContext() { return context; }
    protected static Class<? extends Activity> getActivityClass() { return activityClass; }

    @Override
    public void onReceive (Context context, Intent intent) {
        if (isLoggingEnabled()) {
            Log.i(TAG, "Starting service @ " + SystemClock.elapsedRealtime());
        }

        ComponentName comp = new ComponentName(context.getPackageName(), CountlyMessagingService.class.getName());
        startWakefulService(context, intent.setComponent(comp));
        setResultCode(Activity.RESULT_OK);
    }


    private static final String PREFERENCES_NAME = "ly.count.android.api.messaging";
    private static final String PROPERTY_REGISTRATION_ID = "ly.count.android.api.messaging.registration.id";
    private static final String PROPERTY_REGISTRATION_VERSION = "ly.count.android.api.messaging.version";
    private static final String PROPERTY_APPLICATION_TITLE = "ly.count.android.api.messaging.app.title";
    private static final String PROPERTY_SERVER_URL = "ly.count.android.api.messaging.server.url";
    private static final String PROPERTY_APP_KEY = "ly.count.android.api.messaging.app.key";
    private static final String PROPERTY_DEVICE_ID = "ly.count.android.api.messaging.device.id";
    private static final String PROPERTY_ACTIVITY_CLASS = "ly.count.android.api.messaging.activity.class";
    private static final int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;
    private static GoogleCloudMessaging gcm;

    public static void init(String senderId) {
        init(UnityPlayer.currentActivity, senderId);
    }

    public static void init(Activity activity, String sender) {
        init(activity, activity.getClass(), sender, buttonNames);
    }

    public static void init(Activity activity, Class<? extends Activity> activityClass, String sender, String[] buttonNames) {
        if(isLoggingEnabled()) {
            Log.d(TAG, "Init...");
        }

        setActivity(activity, activityClass);

        if (gcm != null) {
            return;
        }

        if (buttonNames != null) {
            CountlyMessaging.buttonNames = buttonNames;
        }

        if (checkPlayServices(activity) ) {
            if (isLoggingEnabled()) {
                Log.d(TAG, "Updating registration id");
            }
            gcm = GoogleCloudMessaging.getInstance(activity);
            registerInBackground(activity, sender);
        } else {
            if (isLoggingEnabled()) {
                Log.w(TAG, "No valid Google Play Services APK found.");
            }
        }
    }

    public static void storeConfiguration(Context context, String serverURL, String appKey, String deviceID) {
        String label = context.getString(context.getApplicationInfo().labelRes);

        if (isLoggingEnabled()) {
            Log.i(TAG, "Storing configuration: " + label + ", " + serverURL + ", " + appKey + ", " + deviceID);
        }

        SharedPreferences.Editor editor = getGCMPreferences(context).edit()
                .putString(PROPERTY_APPLICATION_TITLE, label)
                .putString(PROPERTY_SERVER_URL, serverURL)
                .putString(PROPERTY_APP_KEY, appKey)
                .putString(PROPERTY_DEVICE_ID, deviceID);

        if (activityClass != null) {
            editor.putString(PROPERTY_ACTIVITY_CLASS, activityClass.getName());
        }

        editor.commit();
    }

    private static void registerInBackground(final Context context, final String sender) {
        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    String registrationId = gcm.register(sender);
                    storeRegistrationId(context, registrationId);
                    UnityInteraction.sendRegisterId(registrationId);
                } catch (IOException ex) {
                    Log.e(TAG, "Failed to register for GCM identificator: " + ex.getMessage());
                }
                return null;
            }
        }.execute(null, null, null);
    }

    private static void storeRegistrationId(Context context, String regId) {
        int appVersion = getAppVersion(context);
        if (isLoggingEnabled()) {
            Log.i(TAG, "Saving regId " + regId + " for app version " + appVersion);
        }
        getGCMPreferences(context).edit().putString(PROPERTY_REGISTRATION_ID, regId).putInt(PROPERTY_REGISTRATION_VERSION, appVersion).commit();
    }


    private static SharedPreferences getGCMPreferences(Context context) {
        return context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_PRIVATE);
    }

    private static boolean checkPlayServices(Activity activity) {
        int resultCode = GooglePlayServicesUtil.isGooglePlayServicesAvailable(activity);
        if (resultCode != ConnectionResult.SUCCESS) {
            if (GooglePlayServicesUtil.isUserRecoverableError(resultCode)) {
                GooglePlayServicesUtil.getErrorDialog(resultCode, activity, PLAY_SERVICES_RESOLUTION_REQUEST).show();
            } else {
                Log.w(TAG, "Unable to install Play Services.");
            }
            return false;
        }
        return true;
    }

    private static String getRegistrationId(Activity activity) {
        final SharedPreferences preferences = getGCMPreferences(activity);
        String registrationId = preferences.getString(PROPERTY_REGISTRATION_ID, "");

        if (registrationId.isEmpty()) {
            if (isLoggingEnabled()) {
                Log.i(TAG, "Registration not found.");
            }
            return "";
        }

        int registeredVersion = preferences.getInt(PROPERTY_REGISTRATION_VERSION, Integer.MIN_VALUE);
        int currentVersion = getAppVersion(activity.getApplicationContext());
        if (registeredVersion != currentVersion) {
            if (isLoggingEnabled()) {
                Log.i(TAG, "App version changed.");
            }
            return "";
        }

        return registrationId;
    }

    private static int getAppVersion(Context context) {
        try {
            PackageInfo packageInfo = context.getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0);
            return packageInfo.versionCode;
        } catch (PackageManager.NameNotFoundException e) {
            // should never happen
            throw new RuntimeException("Could not get package name: " + e);
        }
    }

    protected static String getAppTitle(Context context) {
        return getGCMPreferences(context).getString(PROPERTY_APPLICATION_TITLE, "");
    }

    public static void recordMessageOpen(String messageId) {
        Map<String, String> segmentation = new HashMap<String, String>();
        segmentation.put("i", messageId);

        UnityInteraction.sendMessageId(messageId);
        UnityInteraction.sendPushOpen();
    }

    public static void recordMessageAction(String messageId) {
        Map<String, String> segmentation = new HashMap<String, String>();
        segmentation.put("i", messageId);

        UnityInteraction.sendMessageId(messageId);
        UnityInteraction.sendPushAction();
    }

    public static void setLoggingEnabled(boolean enabled) {
        loggingEnabled = enabled;
    }

    public static boolean isLoggingEnabled(){
        return loggingEnabled;
    }
}
