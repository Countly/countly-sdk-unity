package hupp.tech.countly.android.gcm.plugin;

import android.app.Activity;
import android.os.Bundle;

public class MainActivity extends Activity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        CountlyMessaging.setLoggingEnabled(true);
        CountlyMessaging.init(this, "660433174855");
    }

}
