package ly.count.unity.push_fcm;

import android.util.Log;
import android.os.Bundle;
import android.content.Intent;
import android.content.Context;

import com.unity3d.player.UnityPlayer;

import android.content.BroadcastReceiver;

public class NotificationBroadcastReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d(CountlyPushPlugin.TAG, "onReceive");

        Bundle bundle = intent.getExtras();
        String index = bundle.getString("index", "0");
        String messageId = bundle.getString("c.i", "");

        intent.removeExtra("c.i");

        Log.d(CountlyPushPlugin.TAG, "Message ID: " + messageId);

        if (!MessageStore.isInitialized()) {
            MessageStore.init(context);
        }

        if (!messageId.isEmpty()) {
            boolean flag = MessageStore.storeMessageData(messageId, index);
            Log.d(CountlyPushPlugin.TAG, "StoreMessageData: " + flag);
        }

        UnityPlayer.UnitySendMessage(CountlyPushPlugin.UNITY_ANDROID_BRIDGE, "onMessageReceived", messageId);
    }
}