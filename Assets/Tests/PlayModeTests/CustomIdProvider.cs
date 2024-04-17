using Plugins.CountlySDK;

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
