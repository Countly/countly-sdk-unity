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

import org.json.JSONException;
import org.json.JSONObject;

import java.util.Map;

import static ly.count.unity.push_fcm.CountlyPushPlugin.EXTRA_ACTION_INDEX;
import static ly.count.unity.push_fcm.CountlyPushPlugin.EXTRA_MESSAGE;

public class NotificationBroadcastReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        CountlyPushPlugin.log("NotificationBroadcastReceiver::onReceive", CountlyPushPlugin.LogLevel.DEBUG);

        Bundle bundle = intent.getExtras();

        if (bundle == null) {
            CountlyPushPlugin.log("bundle is null", CountlyPushPlugin.LogLevel.DEBUG);
            return;
        }

        int index = bundle.getInt(CountlyPushPlugin.EXTRA_ACTION_INDEX, 0);
        CountlyPushPlugin.Message message = bundle.getParcelable(EXTRA_MESSAGE);

        String messageId = message.getId();

        CountlyPushPlugin.log("Message ID: " + messageId, CountlyPushPlugin.LogLevel.DEBUG);

        if (!MessageStore.isInitialized()) {
            MessageStore.init(context);
        }

        if (!messageId.isEmpty()) {
            boolean flag = MessageStore.storeMessageData(messageId, Integer.toString(index));
            CountlyPushPlugin.log("StoreMessageData: " + flag, CountlyPushPlugin.LogLevel.DEBUG);
        }

        Intent notificationIntent = new Intent(context, UnityPlayerActivity.class);
        notificationIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

        Uri uri = index == 0 ? message.getLink() : message.getButtons().get(index - 1).getLink();
        if (uri != null) {
            Intent i = new Intent(Intent.ACTION_VIEW, uri);
            i.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            i.putExtra(EXTRA_ACTION_INDEX, index);
            context.startActivity(i);
            CountlyPushPlugin.log("URI: " + uri.toString(), CountlyPushPlugin.LogLevel.DEBUG);
        } else {
            context.startActivity(notificationIntent);
        }

        NotificationManager notificationManager =
                (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.cancel(messageId, 0);

        CountlyPushPlugin.log("Index: " + index, CountlyPushPlugin.LogLevel.DEBUG);

        try {
            JSONObject jsonObject = new JSONObject(message.getData());
            jsonObject.put("click_index", index);
            UnityPlayer.UnitySendMessage(CountlyPushPlugin.UNITY_ANDROID_BRIDGE, "OnNotificationClicked", jsonObject.toString());
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
}