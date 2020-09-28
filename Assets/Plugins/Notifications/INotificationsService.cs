using Plugins.Countly.Helpers;
using System;
using System.Threading.Tasks;

namespace Notifications
{
	public interface INotificationsService
	{
        void GetMessage(Action result);
        void GetToken(Action<string> result);
        Task<CountlyResponse> ReportPushActionAsync();
    }
}