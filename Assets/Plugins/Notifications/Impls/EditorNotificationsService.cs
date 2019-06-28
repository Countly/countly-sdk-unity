using System;

namespace Notifications.Impls
{
	public class EditorNotificationsService : INotificationsService
	{
		public void GetToken(Action<string> result) => result.Invoke("FakeToken");
	}
}