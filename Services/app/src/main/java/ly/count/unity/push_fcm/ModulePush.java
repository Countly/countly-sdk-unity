package ly.count.unity.push_fcm;

import android.content.Context;
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

/**
 * Messaging support module.
 */

public class ModulePush {
    static final String KEY_ID = "c.i";
    static final String KEY_LINK = "c.l";
    static final String KEY_MEDIA = "c.m";
    static final String KEY_BUTTONS = "c.b";
    static final String KEY_BUTTONS_LINK = "l";
    static final String KEY_BUTTONS_TITLE = "t";

    static final String KEY_SOUND = "sound";
    static final String KEY_BADGE = "badge";
    static final String KEY_TITLE = "title";
    static final String KEY_MESSAGE = "message";

    static class Message implements Parcelable {
        final String id;
        private final String title, message, sound;
        private final Integer badge;
        private final Uri link;
        private final URL media;
        private final List<Button> buttons;
        private final Map<String, String> data;

        Message(Map<String, String> data) {
            this.data = data;
            this.id = data.get(KEY_ID);
            this.title = data.get(KEY_TITLE);
            this.message = data.get(KEY_MESSAGE);
            this.sound = data.get(KEY_SOUND);

            Log.d("Countly", "constructed: " + id);
            Integer b = null;
            try {
                b = data.containsKey(KEY_BADGE) ? Integer.parseInt(data.get(KEY_BADGE)) : null;
            } catch (NumberFormatException e) {
                Log.w("Countly", "Bad badge value received, ignoring");
            }
            this.badge = b;

            Uri uri = null;
            if (data.get(KEY_LINK) != null) {
                try {
                    uri = Uri.parse(data.get(KEY_LINK));
                } catch (Throwable e) {
                    Log.w("Countly", "Cannot parse message link", e);
                }
            }
            this.link = uri;

            URL u = null;
            try {
                u = data.containsKey(KEY_MEDIA) ? new URL(data.get(KEY_MEDIA)) : null;
            } catch (MalformedURLException e) {
                Log.w("Countly", "Bad media value received, ignoring");
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
                                    Log.w("Countly", "Cannot parse message link", e);
                                }
                            }

                            this.buttons.add(new Button(this, i + 1, btn.getString(KEY_BUTTONS_TITLE), uri));
                        }
                    }
                } catch (Throwable e) {
                    Log.w("Countly", "Failed to parse buttons JSON", e);
                }
            }
        }

        public String getId() {
            return id;
        }

        public String getTitle() {
            return title;
        }

        public String getMessage() {
            return message;
        }

        public String getSound() {
            return sound;
        }

        public Integer getBadge() {
            return badge;
        }

        public Uri getLink() {
            return link;
        }

        public URL getMedia() {
            return media;
        }

        public List<Button> getButtons() {
            return buttons;
        }

        public Set<String> getDataKeys() {
            return data.keySet();
        }

        public boolean has(String key) {
            return data.containsKey(key);
        }

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
            Log.d("Countly", "written: " + data.get(KEY_ID));
        }

        public static final Parcelable.Creator<Message> CREATOR = new Parcelable.Creator<Message>() {

            public Message createFromParcel(Parcel in) {
                Map<String, String> map = new HashMap<>();
                in.readMap(map, ClassLoader.getSystemClassLoader());
                Log.d("Countly", "read: " + map.get(KEY_ID));
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

            public int getIndex() {
                return index;
            }

            public String getTitle() {
                return title;
            }

            public Uri getLink() {
                return link;
            }

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
