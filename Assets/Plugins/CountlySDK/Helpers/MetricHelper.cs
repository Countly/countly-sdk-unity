using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Plugins.CountlySDK.Helpers
{
    public class MetricHelper
    {
        public Dictionary<string, string> overridenMetrics;

        public MetricHelper()
        {

        }

        public MetricHelper(Dictionary<string, string> overridenMetrics)
        {
            this.overridenMetrics = overridenMetrics;
        }

        public string OS
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("OS")) {
                    return overridenMetrics["OS"];
                }
                return Constants.UnityPlatform;
            }
        }

        public string OSVersion
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("OSVersion")) {
                    return overridenMetrics["OSVersion"];
                }
                return SystemInfo.operatingSystem;
            }
        }

        public string Device
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Device")) {
                    return overridenMetrics["Device"];
                }
                return SystemInfo.deviceModel;
            }
        }

        public string Resolution
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Resolution")) {
                    return overridenMetrics["Resolution"];
                }
                return Screen.currentResolution.ToString();
            }
        }

        public string AppVersion
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("appVersion")) {
                    return overridenMetrics["appVersion"];
                }
                return Application.version;
            }
        }

        public string Density
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Density")) {
                    return overridenMetrics["Density"];
                }
                return Screen.dpi.ToString();
            }
        }

        public string Locale
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Locale")) {
                    return overridenMetrics["Locale"];
                }
                return Application.systemLanguage.ToString();
            }
        }

        public string Carrier
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Carrier")) {
                    return overridenMetrics["Carrier"];
                }
                return null;
            }
        }

        public string Store
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Store")) {
                    return overridenMetrics["Store"];
                }
                return null;
            }
        }

        public string Browser
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("Browser")) {
                    return overridenMetrics["Browser"];
                }
                return null;
            }
        }

        public string BrowserVersion
        {
            get {
                if (overridenMetrics != null && overridenMetrics.ContainsKey("browserVersion")) {
                    return overridenMetrics["browserVersion"];
                }
                return null;
            }
        }

        public JObject buildMetricJSON()
        {
            //todo this should include custom metrics
            return null;
        }
    }
}

