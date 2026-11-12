using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    /// <summary>
    /// The SDK health check: counts the warnings and errors the SDK logged, the last request the server refused,
    /// backoffs, method usage ("fu") and log codes ("lc") since the last health check the server accepted, and sends
    /// them once per initialization. Logs at debug level only, as every warning and error the SDK logs is counted here.
    /// </summary>
    internal class HealthCheckCountlyService
    {
        internal const int CountCap = 65535;
        internal const int MaxErrorMessageLength = 1000;
        internal const string StorageKey = "HEALTH_CHECK";
        internal const string AutomaticCrashesStorageKey = "HEALTH_CHECK_AUTOMATIC_CRASHES";

        internal const string AreaSessions = "sessions";
        internal const string AreaEvents = "events";
        internal const string AreaViews = "views";
        internal const string AreaCrashes = "crashes";

        private readonly object _lock = new object();
        private readonly CountlyLogHelper _log;
        private long _countLogWarning;
        private long _countLogError;
        private int _statusCode = -1;
        private string _errorMessage = "";
        private long _countBackoffRequest;
        private int _consecutiveBackoffRequest;
        private int _consecutiveBackoffRequestCounter;
        private readonly Dictionary<string, Dictionary<string, int>> _methodUsage = new Dictionary<string, Dictionary<string, int>>();
        private readonly Dictionary<string, int> _logCodes = new Dictionary<string, int>();
        private volatile bool _recordingStoppedForCrash;
        private Func<Consents, bool> _consentGiven;
        private bool _healthCheckSent;

        /// <summary>
        /// Creates the counters and adds the state a previous run stored, with the automatically detected crashes that
        /// run counted as "crashes" / "record:a". Call on the main thread, as the state is kept in PlayerPrefs.
        /// </summary>
        /// <param name="log">the SDK logger, used at debug level only</param>
        internal HealthCheckCountlyService(CountlyLogHelper log)
        {
            _log = log;
            SetupInitialCounters(PlayerPrefs.GetString(StorageKey, ""));

            int automaticCrashes = PlayerPrefs.GetInt(AutomaticCrashesStorageKey, 0);
            if (automaticCrashes > 0) {
                PlayerPrefs.DeleteKey(AutomaticCrashesStorageKey);
                lock (_lock) {
                    AddUsage(AreaCrashes, "record:a", automaticCrashes);
                }
                SaveState();
            }
        }

        /// <summary>
        /// Sets which feature has consent now; true for every feature while no consent is required.
        /// </summary>
        internal void SetConsentProvider(Func<Consents, bool> consentGiven)
        {
            _consentGiven = consentGiven;
        }

        private void SetupInitialCounters(string initialState)
        {
            if (string.IsNullOrEmpty(initialState)) {
                return;
            }

            JObject stored;
            try {
                stored = JObject.Parse(initialState);
            } catch (Exception ex) when (ex is JsonException || ex is InvalidCastException) {
                ClearAndSave();
                lock (_lock) {
                    AddLogCode("e503", 1);
                }
                _log.Debug($"[HealthCheckCountlyService] SetupInitialCounters, the stored health check state could not be read and is reset: [{ex.Message}]");
                return;
            }

            lock (_lock) {
                _countLogWarning += StoredCount(stored, "LWar");
                _countLogError += StoredCount(stored, "LErr");
                if (_statusCode == -1 && stored["RStatC"]?.Type == JTokenType.Integer) {
                    _statusCode = stored["RStatC"].Value<int>();
                }
                if (_errorMessage.Length == 0 && stored["REMsg"]?.Type == JTokenType.String) {
                    _errorMessage = stored["REMsg"].Value<string>();
                }
                _countBackoffRequest += StoredCount(stored, "BReq");
                _consecutiveBackoffRequest = (int)Math.Max(_consecutiveBackoffRequest, StoredCount(stored, "CBReq"));

                if (stored["MU"] is JObject usage) {
                    foreach (JProperty area in usage.Properties()) {
                        if (!(area.Value is JObject methods)) {
                            continue;
                        }
                        foreach (JProperty method in methods.Properties()) {
                            if (method.Value.Type == JTokenType.Integer) {
                                AddUsage(area.Name, method.Name, method.Value.Value<long>());
                            }
                        }
                    }
                }

                if (stored["LC"] is JObject codes) {
                    foreach (JProperty code in codes.Properties()) {
                        if (code.Value.Type == JTokenType.Integer) {
                            AddLogCode(code.Name, code.Value.Value<long>());
                        }
                    }
                }
            }

            _log.Debug("[HealthCheckCountlyService] SetupInitialCounters, loaded the stored health check state");
        }

        private static long StoredCount(JObject stored, string key)
        {
            JToken value = stored[key];
            return value != null && value.Type == JTokenType.Integer ? Math.Max(0, value.Value<long>()) : 0;
        }

        /// <summary>
        /// Counts one warning the SDK logged.
        /// </summary>
        internal void LogWarning()
        {
            lock (_lock) {
                _countLogWarning++;
            }
        }

        /// <summary>
        /// Counts one error the SDK logged.
        /// </summary>
        internal void LogError()
        {
            lock (_lock) {
                _countLogError++;
            }
        }

        /// <summary>
        /// Keeps the answer of the server to a queued request it did not accept. Only the most recent one is kept. A
        /// status code outside 1..999 is ignored, and the body is cut to 1000 characters.
        /// </summary>
        internal void LogFailedNetworkRequest(int statusCode, string errorResponse)
        {
            if (statusCode <= 0 || statusCode >= 1000) {
                return;
            }

            string response = errorResponse ?? "";
            lock (_lock) {
                _statusCode = statusCode;
                _errorMessage = response.Length > MaxErrorMessageLength ? response.Substring(0, MaxErrorMessageLength) : response;
            }
        }

        /// <summary>
        /// Counts a backoff of the request queue and extends the current run of backoffs in a row. "cbom" holds the
        /// longest run right away. Records log code w320, and w321 when the backoff continues a run.
        /// </summary>
        internal void LogBackoffRequest()
        {
            lock (_lock) {
                bool continuesRun = _consecutiveBackoffRequestCounter > 0;
                _countBackoffRequest++;
                _consecutiveBackoffRequestCounter++;
                _consecutiveBackoffRequest = Math.Max(_consecutiveBackoffRequest, _consecutiveBackoffRequestCounter);
                if (!_recordingStoppedForCrash) {
                    AddLogCode("w320", 1);
                    if (continuesRun) {
                        AddLogCode("w321", 1);
                    }
                }
            }
        }

        /// <summary>
        /// Ends the current run of backoffs in a row, when a queued request was answered in time.
        /// </summary>
        internal void LogConsecutiveBackoffRequest()
        {
            lock (_lock) {
                _consecutiveBackoffRequestCounter = 0;
            }
        }

        /// <summary>
        /// Counts one call of a method for the "fu" map. A call the SDK made by itself carries ":a", a deprecated method
        /// ":d". Nothing is counted while consent is required and the feature of the area has no consent, or while an
        /// automatically detected crash is reported.
        /// </summary>
        internal void RecordUsage(string area, string method)
        {
            if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(method) || _recordingStoppedForCrash || !HasConsentForUsageArea(area)) {
                return;
            }

            lock (_lock) {
                AddUsage(area, method, 1);
            }
        }

        /// <summary>
        /// Counts occurrences of a log code for the "lc" map, a level letter and three digits such as "w740".
        /// </summary>
        internal void RecordLogCode(string code, int count = 1)
        {
            if (string.IsNullOrEmpty(code) || count <= 0 || _recordingStoppedForCrash) {
                return;
            }

            lock (_lock) {
                AddLogCode(code, count);
            }
        }

        /// <summary>
        /// Counts one occurrence of a log code that a public method of a feature area records. Like method usage, it is
        /// ignored while consent is required and the feature of the area has no consent.
        /// </summary>
        internal void RecordLogCode(string code, string area)
        {
            if (!HasConsentForUsageArea(area)) {
                return;
            }

            RecordLogCode(code, 1);
        }

        /// <summary>
        /// Makes method usage and log codes ignore their calls while an automatically detected crash is reported.
        /// </summary>
        internal void StopRecordingForCrash()
        {
            _recordingStoppedForCrash = true;
        }

        /// <summary>
        /// Lets method usage and log codes count again once an automatically detected crash was reported. A Unity app
        /// keeps running after an exception, so counting goes on once the report is queued.
        /// </summary>
        internal void ResumeRecordingAfterCrash()
        {
            _recordingStoppedForCrash = false;
        }

        /// <summary>
        /// Adds one automatically detected crash to the count the next start reports, without the counters. Call on
        /// the main thread.
        /// </summary>
        internal static void IncrementAutomaticCrashCount()
        {
            PlayerPrefs.SetInt(AutomaticCrashesStorageKey, PlayerPrefs.GetInt(AutomaticCrashesStorageKey, 0) + 1);
            PlayerPrefs.Save();
        }

        private bool HasConsentForUsageArea(string area)
        {
            Func<Consents, bool> consentGiven = _consentGiven;
            if (consentGiven == null) {
                return false;
            }

            switch (area) {
                case AreaSessions:
                    return consentGiven(Consents.Sessions);
                case AreaEvents:
                    return consentGiven(Consents.Events);
                case AreaViews:
                    return consentGiven(Consents.Views);
                case AreaCrashes:
                    return consentGiven(Consents.Crashes);
                default:
                    return false;
            }
        }

        private void AddUsage(string area, string method, long count)
        {
            if (count <= 0) {
                return;
            }

            if (!_methodUsage.TryGetValue(area, out Dictionary<string, int> methods)) {
                methods = new Dictionary<string, int>();
                _methodUsage[area] = methods;
            }
            methods.TryGetValue(method, out int current);
            methods[method] = (int)Math.Min(current + count, CountCap);
        }

        private void AddLogCode(string code, long count)
        {
            if (count <= 0) {
                return;
            }

            _logCodes.TryGetValue(code, out int current);
            _logCodes[code] = (int)Math.Min(current + count, CountCap);
        }

        private JObject ToJson(bool forStorage)
        {
            JObject usage = new JObject();
            foreach (KeyValuePair<string, Dictionary<string, int>> area in _methodUsage) {
                usage[area.Key] = JObject.FromObject(area.Value);
            }

            return new JObject {
                [forStorage ? "LErr" : "el"] = _countLogError,
                [forStorage ? "LWar" : "wl"] = _countLogWarning,
                [forStorage ? "RStatC" : "sc"] = _statusCode,
                [forStorage ? "REMsg" : "em"] = _errorMessage,
                [forStorage ? "BReq" : "bom"] = _countBackoffRequest,
                [forStorage ? "CBReq" : "cbom"] = _consecutiveBackoffRequest,
                [forStorage ? "MU" : "fu"] = usage,
                [forStorage ? "LC" : "lc"] = JObject.FromObject(_logCodes)
            };
        }

        /// <summary>
        /// The health check parameter as it is sent. "fu" and "lc" are always present, empty when nothing was counted,
        /// so the server can tell an SDK that reports them from one that does not.
        /// </summary>
        internal JObject Snapshot()
        {
            lock (_lock) {
                return ToJson(false);
            }
        }

        /// <summary>
        /// Takes away what an accepted health check reported, so what was counted while it was on its way is reported
        /// next time. The last failed request and the longest backoff run are cleared only when they did not change since.
        /// </summary>
        internal void ClearSentValues(JObject sent)
        {
            lock (_lock) {
                _countLogError = Math.Max(0, _countLogError - (sent["el"]?.Value<long>() ?? 0));
                _countLogWarning = Math.Max(0, _countLogWarning - (sent["wl"]?.Value<long>() ?? 0));
                _countBackoffRequest = Math.Max(0, _countBackoffRequest - (sent["bom"]?.Value<long>() ?? 0));

                if (_consecutiveBackoffRequest == (sent["cbom"]?.Value<int>() ?? -1)) {
                    _consecutiveBackoffRequest = 0;
                }

                if (_statusCode == (sent["sc"]?.Value<int>() ?? -2) && _errorMessage == (sent["em"]?.Value<string>() ?? "")) {
                    _statusCode = -1;
                    _errorMessage = "";
                }

                if (sent["fu"] is JObject sentUsage) {
                    foreach (JProperty area in sentUsage.Properties()) {
                        if (!(area.Value is JObject sentMethods) || !_methodUsage.TryGetValue(area.Name, out Dictionary<string, int> counted)) {
                            continue;
                        }
                        foreach (JProperty method in sentMethods.Properties()) {
                            Subtract(counted, method.Name, method.Value.Value<int>());
                        }
                        if (counted.Count == 0) {
                            _methodUsage.Remove(area.Name);
                        }
                    }
                }

                if (sent["lc"] is JObject sentCodes) {
                    foreach (JProperty code in sentCodes.Properties()) {
                        Subtract(_logCodes, code.Name, code.Value.Value<int>());
                    }
                }
            }

            SaveState();
        }

        private static void Subtract(Dictionary<string, int> counts, string key, int reported)
        {
            if (!counts.TryGetValue(key, out int current)) {
                return;
            }
            int left = current - reported;
            if (left > 0) {
                counts[key] = left;
            } else {
                counts.Remove(key);
            }
        }

        /// <summary>
        /// Stores the counters in PlayerPrefs, on the main thread, as PlayerPrefs can only be used there.
        /// </summary>
        internal void SaveState()
        {
            string state;
            lock (_lock) {
                state = ToJson(true).ToString(Formatting.None);
            }

            RunOnMainThread(() => {
                PlayerPrefs.SetString(StorageKey, state);
                PlayerPrefs.Save();
            });
        }

        /// <summary>
        /// Resets every counter to its default and clears the stored state.
        /// </summary>
        internal void ClearAndSave()
        {
            lock (_lock) {
                _countLogWarning = 0;
                _countLogError = 0;
                _statusCode = -1;
                _errorMessage = "";
                _countBackoffRequest = 0;
                _consecutiveBackoffRequest = 0;
                _consecutiveBackoffRequestCounter = 0;
                _methodUsage.Clear();
                _logCodes.Clear();
            }

            RunOnMainThread(() => PlayerPrefs.SetString(StorageKey, ""));
        }

        private static void RunOnMainThread(Action action)
        {
            CountlyMainThreadHandler handler = CountlyMainThreadHandler.Instance;
            if (handler.IsMainThread()) {
                action();
            } else {
                handler.RunOnMainThread(action);
            }
        }

        /// <summary>
        /// Sends the health check once per initialization as an immediate request to /i, unless it is disabled. It
        /// carries the base request parameters, the app version as metrics while the sessions consent allows metrics,
        /// and the counters. Only what it reported is taken away from the counters, and only once the server accepted
        /// it with a 2xx answer that is a JSON object with a "result".
        /// </summary>
        internal async Task SendHealthCheckAsync(CountlyConfiguration configuration, RequestCountlyHelper requestHelper, ConsentCountlyService consents)
        {
            if (!configuration.IsHealthCheckEnabled()) {
                _log.Debug("[HealthCheckCountlyService] SendHealthCheckAsync, the health check is disabled, nothing is sent");
                return;
            }

            lock (_lock) {
                if (_healthCheckSent) {
                    _log.Debug("[HealthCheckCountlyService] SendHealthCheckAsync, the health check was already sent");
                    return;
                }
                _healthCheckSent = true;
            }

            JObject snapshot = Snapshot();
            Dictionary<string, object> requestParams = new Dictionary<string, object> {
                { "hc", snapshot.ToString(Formatting.None) }
            };

            string appVersion = configuration.metricHelper?.AppVersion;
            if (!string.IsNullOrEmpty(appVersion) && consents.CheckConsentInternal(Consents.Sessions)) {
                requestParams.Add("metrics", new JObject { ["_app_version"] = appVersion }.ToString(Formatting.None));
            }

            _log.Debug("[HealthCheckCountlyService] SendHealthCheckAsync, sending the health check");
            CountlyResponse response = await requestHelper.SendImmediateRequestAsync(requestParams);
            if (response.IsSuccess) {
                _log.Debug("[HealthCheckCountlyService] SendHealthCheckAsync, the server accepted the health check");
                ClearSentValues(snapshot);
            } else {
                _log.Debug("[HealthCheckCountlyService] SendHealthCheckAsync, the server did not accept the health check, the counters are kept for the next start");
            }
        }
    }
}
