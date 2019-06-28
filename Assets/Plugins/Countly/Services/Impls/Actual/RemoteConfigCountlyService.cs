using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class RemoteConfigCountlyService : IRemoteConfigCountlyService
    {
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly ICountlyUtils _countlyUtils;
        private readonly Dao<ConfigEntity> _configDao;

        public  Dictionary<string, object> Configs { private set; get; } 

        private readonly StringBuilder _requestStringBuilder = new StringBuilder();
        
        public RemoteConfigCountlyService(RequestCountlyHelper requestCountlyHelper, ICountlyUtils countlyUtils, Dao<ConfigEntity> configDao)
        {
            _requestCountlyHelper = requestCountlyHelper;
            _countlyUtils = countlyUtils;
            _configDao = configDao;
        }

        public async Task<CountlyResponse> InitConfig()
        {
            var requestParams =
                new Dictionary<string, object>
                {
                    { "method", "fetch_remote_config" }
                };

            var url = BuildGetRequest(requestParams);
            
            
            var response =  await Task.Run(() => _requestCountlyHelper.GetAsync(url));
            if (response.IsSuccess)
            {
                _configDao.RemoveAll();
                var configEntity = new ConfigEntity
                {
                    Id = _configDao.GenerateNewId(),
                    Json = response.Data
                };
                _configDao.Save(configEntity);
                Configs = Converter.ConvertJsonToDictionary(response.Data);
            }
            else
            {
                var allConfigs = _configDao.LoadAll();
                if (allConfigs != null && allConfigs.Count > 0)
                {
                    Configs = Converter.ConvertJsonToDictionary(allConfigs[0].Json);
                    Debug.Log("Configs: " + Configs.Count);
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
            foreach (var item in _countlyUtils.GetAppKeyAndDeviceIdParams())
            {
                _requestStringBuilder.AppendFormat((item.Key != "app_key" ? "&" : string.Empty) + "{0}={1}",
                    UnityWebRequest.EscapeURL(item.Key), UnityWebRequest.EscapeURL(Convert.ToString(item.Value))); 
            }


            //Query params supplied for creating request
            foreach (var item in queryParams)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                {
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
      
            _requestStringBuilder.Insert(0, _countlyUtils.GetRemoteConfigOutputUrl());
            return _requestStringBuilder.ToString();
        }

    }
}