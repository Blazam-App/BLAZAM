using BLAZAM.Common.Data;

namespace BLAZAM.Jobs
{
    public interface IJobStep
    {
        Func<bool> Action { get; set; }
        Exception Exception { get; }
        WindowsImpersonation Identity { get; set; }
        string Name { get; set; }
        bool? Result { get; set; }

        bool Run();
    }
}