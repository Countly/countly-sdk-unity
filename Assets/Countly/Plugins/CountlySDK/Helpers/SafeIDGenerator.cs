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
}