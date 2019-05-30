using System;

namespace Notifications
{
	public interface INotificationsService
	{
		void GetToken(Action<string> result);
	}
}