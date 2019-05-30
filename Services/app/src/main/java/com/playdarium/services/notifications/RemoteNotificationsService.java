package com.playdarium.services.notifications;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.graphics.drawable.BitmapDrawable;
import android.media.RingtoneManager;
import android.net.Uri;
import android.support.v4.app.NotificationCompat.Builder;
import android.support.v4.content.ContextCompat;
import android.util.Log;

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

    private final String TAG = "RNS";
    private final String UNITY_ANDROID_BRIDGE = "[Android] Bridge";

    public void getToken() {
        FirebaseInstanceId.getInstance().getInstanceId().addOnCompleteListener(new OnCompleteListener<InstanceIdResult>() {
            @Override
            public void onComplete(Task<InstanceIdResult> task) {
                if (!task.isSuccessful()) {
                    Log.w(TAG, "getInstanceId failed", task.getException());
                    return;
                }

                // Get new Instance ID token
                String token = task.getResult().getToken();
                Log.d(TAG, "Firebase token: " + token);
                UnityPlayer.UnitySendMessage(UNITY_ANDROID_BRIDGE, "OnTokenResult", token);
            }
        });
    }

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        RemoteMessage.Notification notification = remoteMessage.getNotification();
        Map<String, String> data = remoteMessage.getData();

        Log.d(TAG, "Message id: " + remoteMessage.getMessageId());
        Log.d(TAG, "Message type: " + remoteMessage.getMessageType());
        Log.d(TAG, "Message from: " + remoteMessage.getFrom());
        if (!data.isEmpty()) {
            JSONObject jsonObject = new JSONObject(remoteMessage.getData());
            Log.d(TAG, "Message data: " + jsonObject.toString());
        }
        if (notification != null) {
            Log.d(TAG, "Message notification: " + notification);
            Log.d(TAG, "Message body: " + notification.getBody());
            Log.d(TAG, "Message body: " + notification.getTitle());
        }

        if (notification != null)
            ProcessNotification(notification);
        if (!data.isEmpty())
            ProcessData(data);
    }

    private void ProcessNotification(RemoteMessage.Notification notification) {
        String title = notification.getTitle();
        String message = notification.getBody();
        sendNotification(title, message);
    }

    private void ProcessData(Map<String, String> data) {
        String title = data.get("title");
        String message = data.get("message");
        sendNotification(title, message);
    }

    private void sendNotification(String title, String message) {
        Uri notificationSound = RingtoneManager.getDefaultUri(R.raw.boing);

        Builder notificationBuilder;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            notificationBuilder = new Builder(this, NotificationChannel.DEFAULT_CHANNEL_ID);
        } else {
            notificationBuilder = new Builder(this);
        }

        BitmapDrawable largeIconBitmap = (BitmapDrawable) ContextCompat.getDrawable(this, R.drawable.ic_stat);

        notificationBuilder
                .setSmallIcon(R.drawable.ic_stat)
                .setLargeIcon(largeIconBitmap.getBitmap())
                .setContentTitle(title)
                .setContentText(message)
                .setAutoCancel(true)
                .setSound(notificationSound)
                .setColor(ContextCompat.getColor(this, R.color.color_notification));

        NotificationManager notificationManager =
                (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

        notificationManager.notify(0, notificationBuilder.build());
    }
}
