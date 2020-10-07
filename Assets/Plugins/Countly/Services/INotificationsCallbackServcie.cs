using System.Threading.Tasks;
using Notifications;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface INotificationsCallbackServcie
    {
        void RemoveListener(int instanceId);
        void SendMessageToListeners(string data);
        void AddListener(int instanceId, INotificationListener listener);
    }
}