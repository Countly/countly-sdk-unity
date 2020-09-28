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

    /**
     * Decode message from {@code RemoteMessage#getData()} map into {@link Message}.
     *
     * @param data map to decode
     * @return message instance or {@code null} if cannot decode
     */
    public static Message decodeMessage(Map<String, String> data) {
        ModulePush.MessageImpl message = new ModulePush.MessageImpl(data);
        return message.id == null ? null : message;
    }

    public interface Message extends Parcelable {
        /**
         * Countly internal message ID
         *
         * @return id string or {@code null} if no id in the message
         */
        String id();

        /**
         * Title of message
         *
         * @return title string or {@code null} if no title in the message
         */
        String title();

        /**
         * Message text itself
         *
         * @return message string or {@code null} if no message specified
         */
        String message();

        /**
         * Message sound. Default message is sent as "default" string, other sounds are
         * supposed to be sent as URI of sound from app resources.
         *
         * @return sound string or {@code null} if no sound specified
         */
        String sound();

        /**
         * Message badge if any
         *
         * @return message badge number or {@code null} if no badge specified
         */
        Integer badge();

        /**
         * Default message link to open
         *
         * @return message link Uri or {@code null} if no link specified
         */
        Uri link();

        /**
         * Message media URL to jpeg or png image
         *
         * @return message media URL or {@code null} if no media specified
         */
        URL media();

        /**
         * List of buttons to display along this message if any
         *
         * @return message buttons list or empty list if no buttons specified
         */
        List<Button> buttons();

        /**
         * Set of data keys sent in this message, includes all standard keys like "title" or "message"
         *
         * @return message data keys set
         */
        Set<String> dataKeys();

        /**
         * Check whether data contains the key specified
         *
         * @param key key String to look for
         * @return {@code true} if key exists in the data, {@code false} otherwise
         */
        boolean has(String key);

        /**
         * Get data associated with the key specified
         *
         * @param key key String to look for
         * @return value String for the key or {@code null} if no such key exists in the data
         */
        String data(String key);
    }

    interface Button {
        /**
         * Button index, starts from 1
         *
         * @return index of this button
         */
        int index();

        /**
         * Button title
         *
         * @return title of this button
         */
        String title();

        /**
         * Button link
         *
         * @return link of this button
         */
        Uri link();

        /**
         * Optional method to return icon code
         *
         * @return int resource code for {@link Notification.Action#getSmallIcon()}
         */
        int icon();
    }
}
