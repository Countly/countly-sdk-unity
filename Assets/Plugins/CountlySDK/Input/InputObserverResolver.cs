using CountlySDK.Input.Impls;

namespace CountlySDK.Input
{
	public static class InputObserverResolver
	{

		public static IInputObserver Resolve()
		{
			IInputObserver observer = null;
#if !UNITY_EDITOR && UNITY_ANDROID || UNITY_IOS
			observer = new MobileInputObserver();
#else
			observer = new DefaultInputObserver();
#endif
			return observer;
		}
		
	}
}