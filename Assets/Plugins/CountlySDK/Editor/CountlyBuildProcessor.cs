#if ENABLE_VSTU

#if UNITY_ANDROID
using System.IO;
using UnityEngine;
#endif

#if !UNITY_2017
using UnityEditor.Build.Reporting;
#endif
using UnityEditor;
using UnityEditor.Build;

namespace Plugins.CountlySDK.Editor
{

    internal abstract class AbstractProcessBuild {

        public virtual void CheckIfJarFileExist()
        {
#if UNITY_ANDROID
            string directoryPath = "/Plugins/Android/Notifications/";
            string filePath = "/Plugins/Android/Notifications/libs/countly_notifications.jar";
            if (!File.Exists(Application.dataPath + "" + filePath)) {
                if (Directory.Exists(Application.dataPath + directoryPath) && !File.Exists(Application.dataPath + "" + filePath)) {
                    Debug.LogError("[CountlyBuildProcessor] notifications.jar not found at: " + filePath);
                }
            }
#endif
        }
    }

#if UNITY_2017
    internal class CountlyBuildProcessor : AbstractProcessBuild, IPreprocessBuild
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            CheckIfJarFileExist();
        }
    }
#else
  internal class CountlyBuildProcessor : AbstractProcessBuild, IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            CheckIfJarFileExist();
        }
    }
#endif


}
#endif