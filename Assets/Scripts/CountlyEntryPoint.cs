using System.Collections;
using Plugins.Countly;
using Plugins.Countly.Impl;
using UnityEngine;

public class CountlyEntryPoint : MonoBehaviour
{
	public Plugins.Countly.Impl.Countly countly;
	public CountlyWrapper countlyWrapper;

	private ICountly _countly;
	
	private void Awake ()
	{
#if  !UNITY_EDITOR
		_countly = Instantiate(countly);      
#else
		_countly = Instantiate(countlyWrapper);
#endif
		
		StartCoroutine(SendEvents());
	}
	

	private IEnumerator SendEvents()
	{
		yield return new WaitForSeconds(1);
		_countly.Events.RecordEventAsync("Test event");
		_countly.Views.RecordOpenViewAsync("Menu", true);
		yield return new WaitForSeconds(4);
		_countly.Views.RecordCloseViewAsync("Menu", true);
	}
}
