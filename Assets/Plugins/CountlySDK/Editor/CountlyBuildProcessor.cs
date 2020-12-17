using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace Plugins.CountlySDK.Editor
{
    public class CountlyBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
            string filePath = "/Plugins/Android/Notifications/libs/countly_notifications.jar";
            if (!File.Exists(Application.dataPath + "" + filePath))
            {
                Debug.LogError("[Countly] notifications.jar not found at: " + filePath);
            }
#endif
        }
    }
}
