using System.ComponentModel;

namespace Assets.Scripts.Enums
{
    public enum TestMode
    {
        // NOTE: The integer values assigned are NOT MEANT TO BE CHANGED
        [Description("Used for Android/iOS Production build")]
        ProductionToken = 0,
        [Description("Used for iOS Debug/Development/Test build")]
        iOSTestToken = 1,
        [Description("Used for Android Debug/Development/Test build")]
        AndroidTestToken = 2,
        [Description("Used for iOS AdHoc build")]
        iOSAdHocToken = 2,
    }
}
