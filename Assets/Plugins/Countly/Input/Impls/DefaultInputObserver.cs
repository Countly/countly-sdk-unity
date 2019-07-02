namespace Countly.Input.Impls
{
	public class DefaultInputObserver : IInputObserver
	{
		public bool HasInput => UnityEngine.Input.anyKeyDown;
	}
}