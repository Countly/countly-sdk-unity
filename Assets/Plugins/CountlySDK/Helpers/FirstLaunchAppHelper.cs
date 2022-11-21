using System;
using UnityEngine;

namespace Plugins.CountlySDK.Helpers
{
    internal static class FirstLaunchAppHelper
    {
        private static bool? _firstLaunchApp;

        public static void Process()
        {
            if (!PlayerPrefs.HasKey(Constants.FirstAppLaunch)) {
                PlayerPrefs.SetInt(Constants.FirstAppLaunch, 1);
                PlayerPrefs.Save();
                _firstLaunchApp = true;
            } else {
                PlayerPrefs.SetInt(Constants.FirstAppLaunch, 0);
                PlayerPrefs.Save();
                _firstLaunchApp = false;
            }
        }

        public static bool IsFirstLaunchApp
        {
            get {
                if (!_firstLaunchApp.HasValue) {
                    throw new ArgumentException("FirstLaunchAppHelper.Process should be called when session begins");
                }

                return _firstLaunchApp.Value;
            }
        }
    }
}