using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserProfileModule
{
    public void Increment(string key);
    public void IncrementBy(string key, double value);
    public void Max(string key, double value);
    public void Min(string key, double value);
    public void Multiply(string key, double value);
    public void Pull(string key, string value);
    public void Push(string key, string value);
    public void PushUnique(string key, string value);
    public void Save();
    public void SetOnce(string key, string value);
    public void SetData(Dictionary<string, object> userData);
    public void SetProperties(Dictionary<string, object> data);
    public void SetProperty(string key, object value);
}