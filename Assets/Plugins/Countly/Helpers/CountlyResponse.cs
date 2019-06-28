namespace Plugins.Countly.Helpers
{
    public struct CountlyResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(IsSuccess)}: {IsSuccess}, {nameof(ErrorMessage)}: {ErrorMessage}, {nameof(Data)}: {Data}";
        }
    }
}
