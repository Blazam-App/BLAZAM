using BLAZAM.Logger;

namespace BLAZAM.Jobs
{
    public static class JobBroker
    {
        private static int maxConcurrentJobs = 6;

        private static Semaphore? _runTokens;

        public static int MaxConcurrentJobs { get => maxConcurrentJobs; set => maxConcurrentJobs = value; }

        public static bool GetRunToken()
        {

            try
            {
                if (_runTokens == null)
                {
                    _runTokens = new Semaphore(MaxConcurrentJobs, MaxConcurrentJobs);
                }

                return _runTokens.WaitOne();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Unexpected error getting job run token");
                return false;
            }

        }
        public static int ReleaseRunToken()
        {

            try
            {
                if (_runTokens == null)
                {
                    _runTokens = new Semaphore(1, MaxConcurrentJobs);
                    return MaxConcurrentJobs;
                }
                return _runTokens.Release();
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Unexpected error releasing job run token");
                return -1;
            }

        }
    }
}
