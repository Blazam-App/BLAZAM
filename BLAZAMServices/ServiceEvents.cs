namespace BLAZAM.Services
{
    public static class ServiceEvents
    {
        public static AppDelegate<Guid, string> MFARequested { get; set; }

        public static void InvokeMFARequested(Guid id, string uri)
        {
            MFARequested?.Invoke(id, uri);
        }

        public static AppDelegate<string, string> MFACallbackReceived { get; set; }

        public static void InvokeMFACallbackReceived(string state, string code)
        {
            MFACallbackReceived?.Invoke(state, code);
        }
    }
}
