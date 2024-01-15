using BLAZAM.Common.Data;
using System.Runtime.CompilerServices;

namespace BLAZAM.Jobs
{
    public class JobStep : IJobStep
    {
        public virtual string Name { get; set; }
        public Exception Exception { get; private set; }
        public WindowsImpersonation Identity { get; set; }
        public Func<bool> Action { get; set; }
        public bool? Result { get; set; } = null;

        public JobStep(string name, Func<bool> action)
        {
            Name = name;
            Action = action;
        }

        public JobStep(string name, Func<bool> action, WindowsImpersonation identity) : this(name, action)
        {
            Identity = identity;
        }

        public bool Run()
        {
            try
            {
                Result = Action.Invoke();
                return (bool)Result;

            }
            catch (Exception ex)
            {
                Exception = ex;
                return false;
            }
        }
    }
}