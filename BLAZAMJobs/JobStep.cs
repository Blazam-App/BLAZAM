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
        public Func<JobStep?, bool>? Action { get; }
        public Func<JobStep?, Task<bool>>? AsyncAction { get; }

        /// <summary>
        /// Creates a new step to be added to a <see cref="IJob"/>
        /// </summary>
        /// <param name="name">The name of the step</param>
        /// <param name="action">The action to perform during step execution</param>
        /// <param name="stopOnError"></param>
        public JobStep(string name, Func<JobStep?, bool> action,bool stopOnError=false)
        {
            StopOnFailedStep = stopOnError;
            Name = name;
            Action = action;
        }

        /// <summary>
        /// Creates a new step to be added to a <see cref="IJob"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="asyncAction"></param>
        /// <param name="stopOnError"></param>
        public JobStep(string name, Func<JobStep?, Task<bool>> asyncAction, bool stopOnError = false)
        {
            StopOnFailedStep = stopOnError;

            Name = name;
            AsyncAction = asyncAction;
        }

        public JobStep(string name, Func<JobStep?, bool> action, WindowsImpersonation identity) : this(name, action)
        {
            Identity = identity;
        }
        public override void Cancel()
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
   
                bool actionResult = false;
                if (Action != null)
                {
                    actionResult = Action.Invoke(this);
                }
                else if (AsyncAction != null)
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