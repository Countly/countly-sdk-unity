using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface IRemoteConfigCountlyService
    {
        Dictionary<string, object> Configs { get; }

        [Obsolete("CrushReports is deprecated, please use Configs instead.")]
        Task<CountlyResponse> InitConfig();

        Task<CountlyResponse> Update();
    }
}