package ly.count.unity.push_fcm;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Build;

import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.InstanceIdResult;
import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.Map;

public class RemoteNotificationsService extends FirebaseMessagingService {

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            // Register the channel with the system; you can't change the importance
            // or other notification behaviors after this
            NotificationManager notificationManager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
            if (notificationManager != null) {
                // Create the NotificationChannel
                NotificationChannel channel =
                        new NotificationChannel(CountlyPushPlugin.CHANNEL_ID, getString(R.string.countly_hannel_name), NotificationManager.IMPORTANCE_DEFAULT);
                channel.setDescription(getString(R.string.countly_channel_description));

                channel.setLightColor(Color.GREEN);
                notificationManager.createNotificationChannel(channel);

                CountlyPushPlugin.log("NotificationChannel Created", CountlyPushPlugin.LogLevel.DEBUG);
            }
        }
    }

    public void getToken() {
        FirebaseInstanceId.getInstance().getInstanceId().addOnCompleteListener(new OnCompleteListener<InstanceIdResult>() {
            @Override
            public void onComplete(Task<InstanceIdResult> task) {
                if (!task.isSuccessful()) {
                    CountlyPushPlugin.log("getInstanceId failed", task.getException(), CountlyPushPlugin.LogLevel.DEBUG);
                    return;
                }

                // Get new Instance ID token
                String token = task.getResult().getToken();
                CountlyPushPlugin.log("Firebase token: " + token, CountlyPushPlugin.LogLevel.DEBUG);
                UnityPlayer.UnitySendMessage(CountlyPushPlugin.UNITY_ANDROID_BRIDGE, "OnTokenResult", token);
            }
        });
    }

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        Map<String, String> data = remoteMessage.getData();

        CountlyPushPlugin.log("Message id: " + remoteMessage.getMessageId(), CountlyPushPlugin.LogLevel.DEBUG);
        CountlyPushPlugin.log("Message type: " + remoteMessage.getMessageType(), CountlyPushPlugin.LogLevel.DEBUG);
        CountlyPushPlugin.log("Message from: " + remoteMessage.getFrom(), CountlyPushPlugin.LogLevel.DEBUG);
        if (!data.isEmpty()) {
            JSONObject jsonObject = new JSONObject(remoteMessage.getData());
            UnityPlayer.UnitySendMessage(CountlyPushPlugin.UNITY_ANDROID_BRIDGE, "OnNotificationReceived", jsonObject.toString());
            CountlyPushPlugin.log("Message data: " + jsonObject.toString(), CountlyPushPlugin.LogLevel.DEBUG);
        }

        if (!data.isEmpty())
            ProcessData(data);
    }

    private void ProcessData(Map<String, String> data) {

        CountlyPushPlugin.Message message = CountlyPushPlugin.decodeMessage(data);
        CountlyPushPlugin.log("Message Impl " + message.toString(), CountlyPushPlugin.LogLevel.DEBUG);
        sendNotification(message);
    }

    private void sendNotification(CountlyPushPlugin.Message message) {
        Uri notificationSound = RingtoneManager.getDefaultUri(R.raw.boing);
        NotificationManager notificationManager =
                (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
        Notification.Builder notificationBuilder;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            NotificationChannel channel = notificationManager.getNotificationChannel(CountlyPushPlugin.CHANNEL_ID);
            if (channel == null) {
                createNotificationChannel();
            }
            notificationBuilder = new Notification.Builder(this, CountlyPushPlugin.CHANNEL_ID);
        } else {
            notificationBuilder = new Notification.Builder(this);
        }

        Bitmap largeIconBitmap = BitmapFactory.decodeResource(getResources(),
                R.drawable.ic_stat);

        Intent notificationIntent = new Intent(this.getApplicationContext(), NotificationBroadcastReceiver.class);

        String messageId = message.getId();
        notificationIntent.putExtra(CountlyPushPlugin.KEY_ID, messageId);
        notificationIntent.putExtra(CountlyPushPlugin.EXTRA_MESSAGE, message);
        notificationIntent.putExtra(CountlyPushPlugin.EXTRA_ACTION_INDEX, 0);

        PendingIntent pendingIntent = PendingIntent.getBroadcast(this.getApplicationContext(), message.hashCode(), notificationIntent, PendingIntent.FLAG_CANCEL_CURRENT);

        notificationBuilder
                .setAutoCancel(true)
                .setSound(notificationSound)
                .setLargeIcon(largeIconBitmap)
                .setContentIntent(pendingIntent)
                .setSmallIcon(R.drawable.ic_stat)
                .setContentTitle(message.getTitle())
                .setContentText(message.getMessage());

        if (android.os.Build.VERSION.SDK_INT > 21) {
            notificationBuilder.setColor(getResources().getColor(R.color.color_notification));
        }

        for (int i = 0; i < message.getButtons().size(); i++) {
            CountlyPushPlugin.Message.Button button = message.getButtons().get(i);
            Intent buttonIntent = (Intent) notificationIntent.clone();
            buttonIntent.putExtra(CountlyPushPlugin.EXTRA_ACTION_INDEX, i + 1);

            if (android.os.Build.VERSION.SDK_INT > 16) {
                notificationBuilder.addAction(button.getIcon(), button.getTitle(), PendingIntent.getBroadcast(this.getApplicationContext(), message.hashCode() + i + 1, buttonIntent, PendingIntent.FLAG_CANCEL_CURRENT));
            }
        }

        notificationManager.notify(messageId, 0, notificationBuilder.build());
    }
}
