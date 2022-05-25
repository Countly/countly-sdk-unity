using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins.CountlySDK;

namespace Assets.Tests.PlayModeTests
{
    public class TestUtility
    {
        public static void ClearSDKQueues(Countly CountlyInstance)
        {
            CountlyInstance.Views._eventService._eventRepo.Clear();
            CountlyInstance.CrashReports._requestCountlyHelper._requestRepo.Clear();
        }
    }
}
