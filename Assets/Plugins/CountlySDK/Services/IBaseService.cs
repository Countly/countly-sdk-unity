
namespace Plugins.CountlySDK.Services
{
    internal interface IBaseService
    {
        void DeviceIdChanged(string deviceId, bool merged);
    }

}
