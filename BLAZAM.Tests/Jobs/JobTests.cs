using BLAZAM.Common.Exceptions;
using BLAZAM.Jobs;

namespace BLAZAM.Tests.Jobs
{
    public class JobTests
    {

        private IJob TestJob
        {
            get
            {
                var job = new Job("Test Job");
                var job2 = new Job("Nested Job");
                var step1 = new JobStep("Regular Step Passes", (step) => { Task.Delay(200).Wait(); return true; });
                var step2 = new JobStep("Regular Step Fails", (step) => { Task.Delay(200).Wait(); return false; });
                var step3 = new JobStep("Regular Step Throws", (step) => { Task.Delay(200).Wait(); throw new AppException("Test exception"); return false; });
                var step4 = new JobStep("Nested Step Passes", (step) => { Task.Delay(200).Wait(); return true; });
                var step5 = new JobStep("Nested Step Fails", (step) => { Task.Delay(200).Wait(); return false; });
                var step6 = new JobStep("Nested Step Throws", (step) => { Task.Delay(200).Wait(); throw new AppException("Test exception"); return false; });

                job.Steps.Add(step1);
                job.Steps.Add(step2);
                job.Steps.Add(step3);
                job2.Steps.Add(step4);
                job2.Steps.Add(step5);
                job2.Steps.Add(step6);
                job.Steps.Add(job2);
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
        public void Steps_Cancelled_After_Error_When_Stop_Enabled()
        {
            // Arrange
            var testJob = TestJob;
            testJob.StopOnFailedStep = true;

            // Act
            var result = testJob.Run();

            // Assert
            Assert.True(testJob.Steps[1].Result == JobResult.Failed && testJob.Steps[2].Result == JobResult.Cancelled && testJob.Steps[3].Result == JobResult.Cancelled);
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
                );
        }

    }
}
