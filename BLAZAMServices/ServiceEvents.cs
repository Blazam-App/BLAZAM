namespace BLAZAM.Services
{
    public static class ServiceEvents
    {
        public static AppEvent<Guid, string> MFARequested { get; set; }

        public static void InvokeMFARequested(Guid id, string uri)
        {
            MFARequested?.Invoke(id, uri);
        }

        public static AppEvent<string, string> MFACallbackReceived { get; set; }

        public static void InvokeMFACallbackReceived(string state, string code)
        {
            MFACallbackReceived?.Invoke(state, code);
        }
    }
}
