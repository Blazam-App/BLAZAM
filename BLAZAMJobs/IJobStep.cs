using BLAZAM.Common.Data;

namespace BLAZAM.Jobs
{
    public interface IJobStep
    {
        Exception Exception { get; }
        WindowsImpersonation Identity { get; set; }
        string Name { get; set; }
        bool? Result { get; }

        bool Run();
    }
}