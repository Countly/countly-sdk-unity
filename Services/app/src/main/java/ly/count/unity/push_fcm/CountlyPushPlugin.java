package ly.count.unity.push_fcm;
import java.util.Map;

public class CountlyPushPlugin {
    public static final String TAG = "[CountlyPluginPush]";
    public static final String UNITY_ANDROID_BRIDGE = "[Android] Bridge";
    public static final String CHANNEL_ID = "ly.count.unity.sdk.CountlyPush.CHANNEL_ID";
    public static final String EXTRA_MESSAGE = "ly.count.android.sdk.CountlyPush.message";
    public static final String EXTRA_ACTION_INDEX = "ly.count.android.sdk.CountlyPush.Action";

    /**
     * Decode message from {@code RemoteMessage#getData()} map into {@link ModulePush.Message}.
     *
     * @param data map to decode
     * @return message instance or {@code null} if cannot decode
     */
    public static ModulePush.Message decodeMessage(Map<String, String> data) {
        ModulePush.Message message = new ModulePush.Message(data);
        return message.id == null ? null : message;
    }

}
