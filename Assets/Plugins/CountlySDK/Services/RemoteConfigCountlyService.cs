using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.CountlySDK.Services
{
    public class RemoteConfigCountlyService : AbstractBaseService
    {
        private readonly CountlyUtils _countlyUtils;
        private readonly Dao<ConfigEntity> _configDao;
        private readonly CountlyConfiguration _configuration;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        /// <summary>
        /// Get the remote config values.
        /// </summary>
        public Dictionary<string, object> Configs { private set; get; }

        private readonly StringBuilder _requestStringBuilder = new StringBuilder();

        internal RemoteConfigCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, CountlyUtils countlyUtils, Dao<ConfigEntity> configDao, ConsentCountlyService consentService) : base(logHelper, consentService)
        {
            Log.Debug("[RemoteConfigCountlyService] Initializing.");

            _configDao = configDao;
            _countlyUtils = countlyUtils;
            _configuration = configuration;
            _requestCountlyHelper = requestCountlyHelper;

            if (_consentService.CheckConsentInternal(Consents.RemoteConfig)) {
                Configs = FetchConfigFromDB();
            } else {
                _configDao.RemoveAll();
            }

        }

        /// <summary>
        ///     Fetch fresh remote config values from server and initialize <code>Configs</code>
        /// </summary>
        internal async Task<CountlyResponse> InitConfig()
        {
            Log.Debug("[RemoteConfigCountlyService] InitConfig");

            if (_configuration.EnableTestMode) {
                return new CountlyResponse { IsSuccess = true };
            }

            return await Update();
        }

        /// <summary>
        ///     Fetch locally stored remote config values.
        /// </summary>
        /// <returns>Stored Remote config</returns>
        private Dictionary<string, object> FetchConfigFromDB()
        {
            Dictionary<string, object> config = null;
            List<ConfigEntity> allConfigs = _configDao.LoadAll();
            if (allConfigs != null && allConfigs.Count > 0) {
                config = Converter.ConvertJsonToDictionary(allConfigs[0].Json);
            }

            Log.Debug("[RemoteConfigCountlyService] FetchConfigFromDB : Configs = " + config);

            return config;
        }

        /// <summary>
        ///     Fetch fresh remote config values from server and store locally.
        /// </summary>
        /// <returns></returns>
        public async Task<CountlyResponse> Update()
        {
            Log.Info("[RemoteConfigCountlyService] Update");

            if (!_consentService.CheckConsentInternal(Consents.RemoteConfig)) {
                return new CountlyResponse {
                    IsSuccess = false
                };
            }

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    { "method", "fetch_remote_config" }
                };

            string url = BuildGetRequest(requestParams);


            CountlyResponse response = await Task.Run(() => _requestCountlyHelper.GetAsync(url));
            if (response.IsSuccess) {
                _configDao.RemoveAll();
                ConfigEntity configEntity = new ConfigEntity {
                    Id = _configDao.GenerateNewId(),
                    Json = response.Data
                };
                _configDao.Save(configEntity);
                Configs = Converter.ConvertJsonToDictionary(response.Data);

                Log.Debug("[RemoteConfigCountlyService] UpdateConfig: " + response.ToString());

            }

            return response;
        }

        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams">request's parameters</param>
        /// <returns></returns>
        private string BuildGetRequest(Dictionary<string, object> queryParams)
        {
            _requestStringBuilder.Clear();
            //Metrics added to each request
            foreach (KeyValuePair<string, object> item in _countlyUtils.GetAppKeyAndDeviceIdParams()) {
                _requestStringBuilder.AppendFormat((item.Key != "app_key" ? "&" : string.Empty) + "{0}={1}",
                    UnityWebRequest.EscapeURL(item.Key), UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
            }


            //Query params supplied for creating request
            foreach (KeyValuePair<string, object> item in queryParams) {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null) {
                    _requestStringBuilder.AppendFormat("&{0}={1}", UnityWebRequest.EscapeURL(item.Key),
                        UnityWebRequest.EscapeURL(Convert.ToString(item.Value)));
                }
            }

            _requestStringBuilder.Insert(0, _countlyUtils.ServerOutputUrl);
            return _requestStringBuilder.ToString();
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {
            if (updatedConsents.Contains(Consents.RemoteConfig) && !newConsentValue) {
                Configs = null;
                _configDao.RemoveAll();
            }
        }
        #endregion
    }
}