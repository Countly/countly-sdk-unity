package hupp.tech.countly.android.gcm.plugin;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

/**
 * Helper class to interact with Unity Player. Interacts with CountlyManager game object
 */
public class UnityInteraction {
    public static final String GAME_OBJECT_NAME = "CountlyManager";

    public static final String REGISTER_ID_METHOD = "OnRegisterId";
    public static final String PUSH_INTERACTION_METHOD = "OnPushInteracion";
    public static final String SEND_MESSAGE_ID_METHOD = "OnMessageId";

    public static void sendRegisterId(String registerId) {
        log("Send register id to Unity: " + registerId);

        if (UnityPlayer.currentActivity == null) {
            log("Unity isn't running now");
            return;
        }

        try {
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, REGISTER_ID_METHOD, registerId);
            log("Register id to Unity successfully sent!");
        } catch(Exception ex) {
            ex.printStackTrace();
        }
    }

    public static void sendMessageId(String messageId) {
        log("Send message id to Unity: " + messageId);

        if (UnityPlayer.currentActivity == null) {
            log("Unity isn't running now");
            return;
        }

        try {
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, SEND_MESSAGE_ID_METHOD, messageId);
            log("Message id to Unity successfully sent!");
        } catch(Exception ex) {
            ex.printStackTrace();
        }
    }

    public static void sendPushInteraction(String interaction) {
        log("Send interaction to Unity: " + interaction);

        if (UnityPlayer.currentActivity == null) {
            log("Unity isn't running now");
            return;
        }

        try {
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, PUSH_INTERACTION_METHOD, interaction);
            log("Push interaction to Unity successfully sent!");
        } catch(Exception ex) {
            ex.printStackTrace();
        }
    }

    public static void sendPushOpen() {
        sendPushInteraction("[CLY]_push_open");
    }

    public static void sendPushAction() {
        sendPushInteraction("[CLY]_push_action");
    }

    public static void sendPushSent() {
        sendPushInteraction("[CLY]_push_sent");
    }

    private static void log(String text) {
        if (CountlyMessaging.isLoggingEnabled()) {
            Log.d(CountlyMessaging.TAG, text);
        }
    }
}
