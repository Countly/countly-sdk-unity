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
        private readonly CountlyConfiguration _config;
        private readonly CountlyUtils _countlyUtils;
        private readonly Dao<ConfigEntity> _configDao;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        public Dictionary<string, object> Configs { private set; get; }

        private readonly StringBuilder _requestStringBuilder = new StringBuilder();

        internal RemoteConfigCountlyService(CountlyConfiguration config, RequestCountlyHelper requestCountlyHelper, CountlyUtils countlyUtils, Dao<ConfigEntity> configDao, ConsentCountlyService consentService) : base(consentService)
        {
            _config = config;
            _configDao = configDao;
            _countlyUtils = countlyUtils;
            _requestCountlyHelper = requestCountlyHelper;

            Configs = FetchConfigFromDB();
        }

        internal async Task<CountlyResponse> InitConfig()
        {
            if (_config.EnableTestMode) {
                return new CountlyResponse { IsSuccess = true };
            }

            return await Update();
        }

        private Dictionary<string, object> FetchConfigFromDB()
        {
            Dictionary<string, object> config = null;
            List<ConfigEntity> allConfigs = _configDao.LoadAll();
            if (allConfigs != null && allConfigs.Count > 0) {
                config = Converter.ConvertJsonToDictionary(allConfigs[0].Json);

                if (_config.EnableConsoleLogging) {
                    Debug.Log("Configs: " + config.ToString());
                }
            }

            return config;
        }

        /// <summary>
        ///     Fetch fresh remote config from server and store locally.
        /// </summary>
        /// <returns></returns>
        public async Task<CountlyResponse> Update()
        {
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

                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] RemoteConfigCountlyService UpdateConfig: " + response.ToString());
                }
            }

            return response;
        }

        /// <summary>
        ///     Builds request URL using ServerUrl, AppKey, DeviceID and supplied queryParams parameters.
        ///     The data is appended in the URL.
        /// </summary>
        /// <param name="queryParams"></param>
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

            //Not sure if we need checksum here

            //            if (!string.IsNullOrEmpty(_config.Salt))
            //            {
            //                // Create a SHA256   
            //                using (var sha256Hash = SHA256.Create())
            //                {
            //                    var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(_requestStringBuilder + _config.Salt));
            //                    _requestStringBuilder.Insert(0, _countly.GetBaseUrl());
            //                    return _requestStringBuilder.AppendFormat("&checksum256={0}", Impl.Countly.GetStringFromBytes(data)).ToString();
            //                }
            //            }

            _requestStringBuilder.Insert(0, _countlyUtils.ServerOutputUrl);
            return _requestStringBuilder.ToString();
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {

        }
        #endregion
    }
}