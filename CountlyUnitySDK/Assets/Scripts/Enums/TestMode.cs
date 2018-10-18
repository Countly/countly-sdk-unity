using System.ComponentModel;

namespace Assets.Scripts.Enums
{
    public enum TestMode
    {
        [Description("Used for Production build")]
        ProductionToken = 0,
        [Description("Used for Debug/Development build")]
        TestToken = 1,
        [Description("Used for iOS AdHoc build")]
        iOSAdHocToken = 2
    }
}
