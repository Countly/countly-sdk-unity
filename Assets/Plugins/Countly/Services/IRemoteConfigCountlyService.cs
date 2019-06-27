using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface IRemoteConfigCountlyService
    {
        Dictionary<string, object> Configs { get; }
        Task<CountlyResponse> InitConfig();
    }
}