using BLAZAM.FileSystem;
using BLAZAM.Gui.UI.Outputs;
using BLAZAM.Jobs;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Jobs
{
    public class JobTests
    {

        private IJob TestJob
        {
            get
            {
                IJob job = new Job("Test Job");
                IJob job2 = new Job("Nested Job");
                IJobStep step1 = new JobStep("Regular Step Passes", () => { Task.Delay(200).Wait(); return true; });
                IJobStep step2 = new JobStep("Regular Step Fails", () => { Task.Delay(200).Wait(); return false; });
                IJobStep step3 = new JobStep("Regular Step Throws", () => { Task.Delay(200).Wait(); throw new ApplicationException("Test exception"); });
                IJobStep step4 = new JobStep("Nested Step Passes", () => { Task.Delay(200).Wait(); return true; });
                IJobStep step5 = new JobStep("Nested Step Fails", () => { Task.Delay(200).Wait(); return false; });
                IJobStep step6 = new JobStep("Nested Step Throws", () => { Task.Delay(200).Wait(); throw new ApplicationException("Test exception"); });

                job.Steps.Add(step1);
                job.Steps.Add(step2);
                job.Steps.Add(step3);
                job2.Steps.Add(step4);
                job2.Steps.Add(step5);
                job2.Steps.Add(step6);
                job.Steps.Add((IJobStep)job2);
                return job;
            }
        }
        [Fact]
        public void Job_Stops_On_Error_When_Stop_Enabled()
        {
            // Arrange
            var testJob = TestJob;
            testJob.StopOnFailedStep = true;

            // Act
            var result = testJob.Run();

            // Assert
            Assert.True(result == false && testJob.FailedSteps.Count == 1 && testJob.PassedSteps.Count == 1);
        }
        [Fact]
        public void Steps_Canceled_After_Error_When_Stop_Enabled()
        {
            // Arrange
            var testJob = TestJob;
            testJob.StopOnFailedStep = true;

            // Act
            var result = testJob.Run();

            // Assert
            Assert.True(testJob.Steps[1].Result == JobResult.Failed && testJob.Steps[2].Result==JobResult.Cancelled && testJob.Steps[3].Result== JobResult.Cancelled);
        }
        [Fact]
        public void Nested_Job_Runs()
        {
            // Arrange
            var testJob = TestJob;

            // Act
            var result = testJob.Run();
            var subjobStep1Result = ((IJob)testJob.Steps[3]).Steps[0].Result;
            var subjobStep3Result = ((IJob)testJob.Steps[3]).Steps[2].Result;
            // Assert
            Assert.True(testJob.Steps[3] is IJob && subjobStep1Result == JobResult.Passed && subjobStep3Result == JobResult.Failed);
        }

        [Fact]
        public void Job_Elapsed_Time_Functional()
        {
            // Arrange
            var testJob = TestJob;

            // Act
            var result = testJob.Run();

            // Assert
            Assert.True(testJob.StartTime != null
                && testJob.StartTime != DateTime.MinValue
                && testJob.EndTime != null
                && testJob.EndTime != DateTime.MinValue
                && testJob.ElapsedTime.HasValue
                && testJob.ElapsedTime.Value.TotalMilliseconds > 500
                ) ;
        }

    }
}
