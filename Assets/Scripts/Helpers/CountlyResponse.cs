namespace Assets.Scripts.Helpers
{
    public struct CountlyResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string Data { get; set; }
    }
}
