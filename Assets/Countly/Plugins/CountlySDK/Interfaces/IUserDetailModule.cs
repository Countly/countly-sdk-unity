using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Models;

public interface IUserDetailModule
{
    Task SetUserDetailsAsync(CountlyUserDetailsModel userDetailsModel);
    void SetCustomUserDetails(Dictionary<string, object> customDetail);
    Task SaveAsync();
    void Set(string key, string value);
    void SetOnce(string key, string value);
    void Increment(string key);
    void IncrementBy(string key, double value);
    void Multiply(string key, double value);
    void Max(string key, double value);
    void Min(string key, double value);
    void Push(string key, string[] value);
    void PushUnique(string key, string[] value);
    void Pull(string key, string[] value);
    bool ContainsCustomDataKey(string key);
    object RetrieveCustomDataValue(string key);
}