package ly.count.unity.push_fcm;
import android.app.Notification;
import android.net.Uri;
import android.os.Parcel;
import android.os.Parcelable;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONObject;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class CountlyPushPlugin {
    public static final String TAG = "[CountlyPluginPush]";
    public static final String UNITY_ANDROID_BRIDGE = "[Android] Bridge";
    public static final String CHANNEL_ID = "ly.count.unity.sdk.CountlyPush.CHANNEL_ID";
    public static final String EXTRA_MESSAGE = "ly.count.android.sdk.CountlyPush.message";
    public static final String EXTRA_ACTION_INDEX = "ly.count.android.sdk.CountlyPush.Action";

    public static final String KEY_ID = "c.i";
    public static final String KEY_LINK = "c.l";
    public static final String KEY_MEDIA = "c.m";
    public static final String KEY_BUTTONS = "c.b";
    public static final String KEY_BUTTONS_LINK = "l";
    public static final String KEY_BUTTONS_TITLE = "t";

    public static final String KEY_SOUND = "sound";
    public static final String KEY_BADGE = "badge";
    public static final String KEY_TITLE = "title";
    public static final String KEY_MESSAGE = "message";

    /**
     * Decode message from {@code RemoteMessage#getData()} map into {@link Message}.
     *
     * @param data map to decode
     * @return message instance or {@code null} if cannot decode
     */
    public static Message decodeMessage(Map<String, String> data) {
        Message message = new Message(data);
        return message.id == null ? null : message;
    }

    /**
     * Message object encapsulating data in {@code RemoteMessage} sent from Countly server.
     */
    static class Message implements Parcelable {
        final String id;
        private final String title, message, sound;
        private final Integer badge;
        private final Uri link;
        private final URL media;
        private final List<Button> buttons;
        private final Map<String, String> data;

        private Message(Map<String, String> data) {
            this.data = data;
            this.id = data.get(KEY_ID);
            this.title = data.get(KEY_TITLE);
            this.message = data.get(KEY_MESSAGE);
            this.sound = data.get(KEY_SOUND);

            Log.d(CountlyPushPlugin.TAG, "constructed: " + id);
            Integer b = null;
            try {
                b = data.containsKey(KEY_BADGE) ? Integer.parseInt(data.get(KEY_BADGE)) : null;
            } catch (NumberFormatException e) {
                Log.w(CountlyPushPlugin.TAG, "Bad badge value received, ignoring");
            }
            this.badge = b;

            Uri uri = null;
            if (data.get(KEY_LINK) != null) {
                try {
                    uri = Uri.parse(data.get(KEY_LINK));
                } catch (Throwable e) {
                    Log.w(CountlyPushPlugin.TAG, "Cannot parse message link", e);
                }
            }
            this.link = uri;

            URL u = null;
            try {
                u = data.containsKey(KEY_MEDIA) ? new URL(data.get(KEY_MEDIA)) : null;
            } catch (MalformedURLException e) {
                Log.w(CountlyPushPlugin.TAG, "Bad media value received, ignoring");
            }
            this.media = u;

            this.buttons = new ArrayList<>();

            String json = data.get(KEY_BUTTONS);
            if (json != null) {
                try {
                    JSONArray array = new JSONArray(json);
                    for (int i = 0; i < array.length(); i++) {
                        JSONObject btn = array.getJSONObject(i);
                        if (btn.has(KEY_BUTTONS_TITLE) && btn.has(KEY_BUTTONS_LINK)) {
                            uri = null;
                            if (btn.getString(KEY_BUTTONS_LINK) != null) {
                                try {
                                    uri = Uri.parse(btn.getString(KEY_BUTTONS_LINK));
                                } catch (Throwable e) {
                                    Log.w(CountlyPushPlugin.TAG, "Cannot parse message link", e);
                                }
                            }

                            this.buttons.add(new Button(this, i + 1, btn.getString(KEY_BUTTONS_TITLE), uri));
                        }
                    }
                } catch (Throwable e) {
                    Log.w(CountlyPushPlugin.TAG, "Failed to parse buttons JSON", e);
                }
            }
        }

        /**
         * Countly internal message ID
         *
         * @return id string or {@code null} if no id in the message
         */
        public String getId() {
            return id;
        }

        /**
         * Title of message
         *
         * @return title string or {@code null} if no title in the message
         */
        public String getTitle() {
            return title;
        }

        /**
         * Message text itself
         *
         * @return message string or {@code null} if no message specified
         */
        public String getMessage() {
            return message;
        }

        /**
         * Message sound. Default message is sent as "default" string, other sounds are
         * supposed to be sent as URI of sound from app resources.
         *
         * @return sound string or {@code null} if no sound specified
         */
        public String getSound() {
            return sound;
        }

        /**
         * Message badge if any
         *
         * @return message badge number or {@code null} if no badge specified
         */
        public Integer getBadge() {
            return badge;
        }

        /**
         * Default message link to open
         *
         * @return message link Uri or {@code null} if no link specified
         */
        public Uri getLink() {
            return link;
        }

        /**
         * Message media URL to jpeg or png image
         *
         * @return message media URL or {@code null} if no media specified
         */
        public URL getMedia() {
            return media;
        }

        /**
         * List of buttons to display along this message if any
         *
         * @return message buttons list or empty list if no buttons specified
         */
        public List<Button> getButtons() {
            return buttons;
        }

        /**
         * Set of data keys sent in this message, includes all standard keys like "title" or "message"
         *
         * @return message data keys set
         */
        public Set<String> getDataKeys() {
            return data.keySet();
        }

        /**
         * Check whether data contains the key specified
         *
         * @param key key String to look for
         * @return {@code true} if key exists in the data, {@code false} otherwise
         */
        public boolean has(String key) {
            return data.containsKey(key);
        }

        /**
         * Get data associated with the key specified
         *
         * @param key key String to look for
         * @return value String for the key or {@code null} if no such key exists in the data
         */
        public String getData(String key) {
            return data.get(key);
        }

        public int hashCode() {
            return id.hashCode();
        }

        @Override
        public int describeContents() {
            return id.hashCode();
        }

        @Override
        public void writeToParcel(Parcel dest, int flags) {
            dest.writeMap(data);
            Log.d(CountlyPushPlugin.TAG, "written: " + data.get(KEY_ID));
        }

        public static final Parcelable.Creator<Message> CREATOR = new Parcelable.Creator<Message>() {

            public Message createFromParcel(Parcel in) {
                Map<String, String> map = new HashMap<>();
                in.readMap(map, ClassLoader.getSystemClassLoader());
                Log.d(CountlyPushPlugin.TAG, "read: " + map.get(KEY_ID));
                return new Message(map);
            }

            public Message[] newArray(int size) {
                return new Message[size];
            }
        };

        static class Button {
            private final Message message;
            private final int index, icon;
            private final String title;
            private final Uri link;

            Button(Message message, int index, String title, Uri link) {
                this.message = message;
                this.index = index;
                this.title = title;
                this.link = link;
                this.icon = 0;
            }

            Button(Message message, int index, String title, Uri link, int icon) {
                this.message = message;
                this.index = index;
                this.title = title;
                this.link = link;
                this.icon = icon;
            }

            /**
             * Button index, starts from 1
             *
             * @return index of this button
             */
            public int getIndex() {
                return index;
            }

            /**
             * Button title
             *
             * @return title of this button
             */
            public String getTitle() {
                return title;
            }

            /**
             * Button link
             *
             * @return link of this button
             */
            public Uri getLink() {
                return link;
            }

            /**
             * Optional method to return icon code
             *
             * @return int resource code for {@link Notification.Action#getSmallIcon()}
             */
            public int getIcon() {
                return icon;
            }

            @Override
            public boolean equals(Object obj) {
                if (obj == null || !(obj instanceof Button)) {
                    return false;
                }
                Button b = (Button) obj;
                return b.index == index && (b.title == null ? title == null : b.title.equals(title)) && (b.link == null ? link == null : b.link.equals(link) && b.icon == icon);
            }
        }
    }

}
