using System.Collections.Generic;

namespace Plugins.Countly
{
    public interface ICountlyUtils
    {
        string GetUniqueDeviceId();

        /// <summary>
        ///     Gets the base url to make requests to the Countly server.
        /// </summary>
        /// <returns></returns>
        string GetBaseInputUrl();

        /// <summary>
        ///     Gets the base url to make remote configrequests to the Countly server.
        /// </summary>
        /// <returns></returns>
        string GetBaseOutputUrl();

        /// <summary>
        ///     Gets the base url to make remote configrequests to the Countly server.
        /// </summary>
        /// <returns></returns>
        string GetRemoteConfigOutputUrl();

        /// <summary>
        ///     Gets the least set of paramas required to be sent along with each request.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> GetBaseParams();

        /// <summary>
        ///     Gets the least set of app key and device id required to be sent along with remote config request,
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> GetAppKeyAndDeviceIdParams();

        bool IsNullEmptyOrWhitespace(string input);

        /// <summary>
        ///     Validates the picture format. The Countly server supports a specific set of formats only.
        /// </summary>
        /// <param name="pictureUrl"></param>
        /// <returns></returns>
        bool IsPictureValid(string pictureUrl);

        string GetStringFromBytes(byte[] bytes);
    }
}