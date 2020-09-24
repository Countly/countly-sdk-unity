using System;

namespace Notifications.Impls
{
	public class EditorNotificationsService : INotificationsService
	{
        public void GetMessage(Action result)
        {
            result.Invoke();
        }

        public void GetToken(Action<string> result) => result.Invoke("FakeToken");
	}
}