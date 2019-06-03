using Plugins.Countly;
using Plugins.Countly.Impl;
using UnityEngine;

public class CountlyEntryPoint : MonoBehaviour
{
	public Countly countly;
	public CountlyWrapper countlyWrapper;

	private ICountly _countly;
	
	private void Awake ()
	{
#if  !UNITY_EDITOR
		_countly = Instantiate(countly);      
#else
		_countly = Instantiate(countlyWrapper);
#endif
		SendEvents();
	}


	private void SendEvents()
	{
		_countly.Events.RecordEventAsync("Test event");
	}
}
