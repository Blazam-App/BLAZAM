using BLAZAM.Gui.Helper;
using BLAZAM.Jobs;

namespace BLAZAM.Tests.Jobs
{
    public class JobTests
    {

        private static IJob TestJob => JobHelpers.TestJob;
        [Fact]
        public void Job_Stops_On_Error_When_Stop_Enabled()
        {
            // Arrange
            var testJob = TestJob;
            testJob.StopOnFailedStep = true;

            // Act
            var result = testJob.Run();

            // Assert
            Assert.True(!result && testJob.FailedSteps.Count == 1 && testJob.PassedSteps.Count == 1);
        }
        [Fact]
        public void Steps_Cancelled_After_Error_When_Stop_Enabled()
        {
            // Arrange
            var testJob = TestJob;
            testJob.StopOnFailedStep = true;

            // Act
            _ = testJob.Run();

            // Assert
            Assert.True(testJob.Steps[1].Result == JobResult.Failed && testJob.Steps[2].Result == JobResult.Cancelled && testJob.Steps[3].Result == JobResult.Cancelled);
        }
        [Fact]
        public void Nested_Job_Runs()
        {
            // Arrange
            var testJob = TestJob;

            // Act
            _ = testJob.Run();
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
            _ = testJob.Run();

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
