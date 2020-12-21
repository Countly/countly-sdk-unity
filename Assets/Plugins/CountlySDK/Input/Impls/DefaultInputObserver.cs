namespace CountlySDK.Input.Impls
{
    public class DefaultInputObserver : IInputObserver
    {
        public bool HasInput => UnityEngine.Input.anyKeyDown;
    }
}