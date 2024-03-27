namespace Plugins.CountlySDK
{
    public interface ISafeIDGenerator
    {
        string GenerateValue();
    }

    public class SafeIDGenerator : ISafeIDGenerator
    {
        public string GenerateValue()
        {
            return CountlyUtils.SafeRandomVal();
        }
    }

    /// <summary>
    /// CustomIdProvider class is for testing purposes
    /// </summary>
    public class CustomIdProvider : ISafeIDGenerator
    {
        private int viewCount;

        public string GenerateValue()
        {
            viewCount++;
            return "idv" + viewCount.ToString();
        }
    }
}