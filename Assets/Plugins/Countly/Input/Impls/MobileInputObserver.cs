namespace Countly.Input.Impls
{
	public class MobileInputObserver : IInputObserver
	{
		public bool HasInput => UnityEngine.Input.touchCount > 0;
	}
}