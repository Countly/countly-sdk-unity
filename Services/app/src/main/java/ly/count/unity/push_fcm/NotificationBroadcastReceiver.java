package ly.count.unity.push_fcm;

import android.app.NotificationManager;
import android.net.Uri;
import android.util.Log;
import android.os.Bundle;
import android.content.Intent;
import android.content.Context;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import android.content.BroadcastReceiver;
import static ly.count.unity.push_fcm.CountlyPushPlugin.EXTRA_ACTION_INDEX;
import static ly.count.unity.push_fcm.CountlyPushPlugin.EXTRA_MESSAGE;

public class NotificationBroadcastReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d(CountlyPushPlugin.TAG, "onReceive");

        Bundle bundle = intent.getExtras();

        if(bundle == null) {
            Log.e(CountlyPushPlugin.TAG, "bundle is null");
            return;
        }

        int index = bundle.getInt(CountlyPushPlugin.EXTRA_ACTION_INDEX, 0);
        CountlyPushPlugin.Message message = bundle.getParcelable(EXTRA_MESSAGE);

        String messageId = message.getId();
        Log.d(CountlyPushPlugin.TAG, "Message ID: " + messageId);

        if (!MessageStore.isInitialized()) {
            MessageStore.init(context);
        }

        if (!messageId.isEmpty()) {
            boolean flag = MessageStore.storeMessageData(messageId, Integer.toString(index));
            Log.d(CountlyPushPlugin.TAG, "StoreMessageData: " + flag);
        }

        Intent notificationIntent = new Intent(context, UnityPlayerActivity.class);
        notificationIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

        Uri uri = index == 0 ? message.getLink() : message.getButtons().get(index -1).getLink();
        if (uri != null) {
            Intent i = new Intent(Intent.ACTION_VIEW, uri);
            i.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            i.putExtra(EXTRA_ACTION_INDEX, index);
            context.startActivity(i);
            Log.d(CountlyPushPlugin.TAG, "URI: " + uri.toString());
        }
        else {
            context.startActivity(notificationIntent);
        }

        NotificationManager notificationManager =
                (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.cancel(messageId, 0);

        Log.d(CountlyPushPlugin.TAG, "Index: " + index);
        UnityPlayer.UnitySendMessage(CountlyPushPlugin.UNITY_ANDROID_BRIDGE, "onMessageReceived", messageId);
    }
}