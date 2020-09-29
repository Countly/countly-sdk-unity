package ly.count.unity.push_fcm;

import android.app.Notification;
import android.net.Uri;
import android.os.Parcelable;

import java.net.URL;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class CountlyPushPlugin {
    public static final String TAG = "[CountlyPluginPush]";
    public static final String UNITY_ANDROID_BRIDGE = "[Android] Bridge";
    public static final String CHANNEL_ID = "ly.count.unity.sdk.CountlyPush.CHANNEL_ID";
    public static final String EXTRA_MESSAGE = "ly.count.android.sdk.CountlyPush.message";
    public static final String EXTRA_ACTION_INDEX = "ly.count.android.sdk.CountlyPush.Action";

    public static ModulePush.Message decodeMessage(Map<String, String> data) {
        ModulePush.Message message = new ModulePush.Message(data);
        return message.id == null ? null : message;
    }

}
