using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BLAZAM.Jobs
{
    public class JobStep : JobStepBase, IJobStep 
    {
        public Func<JobStep?,bool>? Action { get; }
        public Func<JobStep?,Task<bool>>? AsyncAction { get; }

        public JobStep(string name, Func<JobStep?,bool> action)
        {
            Name = name;
            Action = action;
        }
        public JobStep(string name, Func<JobStep?,Task<bool>> asyncAction)
        {
            Name = name;
            AsyncAction = asyncAction;
        }

        public JobStep(string name, Func<JobStep?,bool> action, WindowsImpersonation identity) : this(name, action)
        {
            Identity = identity;
        }
        public void Cancel()
        {
            if (Progress == null || Progress < 100)
            {
                cancellationTokenSource.Cancel();
                EndTime = DateTime.Now;
                Result = JobResult.Cancelled;
                Progress = 100;
            }
        }

        public override bool Run()
        {
            var cancelToken = cancellationTokenSource.Token;
            try
            {
                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }

                StartTime = DateTime.Now;
                Result = JobResult.Running;
                OnProgressUpdated?.Invoke(0);
                //We can't track progress from outside the action
                //so trying to say we can is pointless
                //if (Progress == 0)
                //{
                //    OnProgressUpdated?.Invoke(0);
                //}
                //else
                //{
                //    Progress = 0;
                //}


                bool actionResult=false;
                if (Action != null)
                {
                    actionResult = Action.Invoke(this);
                }else if (AsyncAction != null)
                {
                    Task.Run(async () =>
                    {

                        actionResult = await AsyncAction.Invoke(this);
                    }).Wait();
                }
                else
                {
                    throw new ApplicationException("Step: " + Name + " had no action provided!");
                }


                if (cancelToken.IsCancellationRequested)
                {
                    Cancel();
                    return false;
                }
                if (actionResult)
                {
                    Result = JobResult.Passed;
                }
                else
                {
                    Result = JobResult.Failed;
                }
                EndTime = DateTime.Now;
                Progress = 100;
                return Result == JobResult.Passed;

            }
            catch (Exception ex)
            {
                Result = Result = JobResult.Failed;
                Exception = ex;
                EndTime = DateTime.Now;
                Progress = 100;

                return false;
            }
        }
    }
}