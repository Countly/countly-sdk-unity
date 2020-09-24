using System;

namespace Notifications
{
	public interface INotificationsService
	{
        void GetMessage(Action result);
        void GetToken(Action<string> result);
		

	}
}