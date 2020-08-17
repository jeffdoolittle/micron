namespace Micron.SqlClient.Retry
{
    using System;
    using Xunit;

    public class ExceptionRetryConfigurationTests
    {
        [Fact]
        public void Can_configure_exception_retries()
        {
            var configuration = ConfigureRetries
                .OnException<ArgumentNullException>()
                .Matching(ex => true)
                .Retry(5, 50);

            Assert.Equal(5, configuration.RetryTimes);
            Assert.True(configuration.Condition(new ArgumentNullException()));
        }

        [Fact]
        public void Can_configure_exception_retries_with_backoff_function()
        {
            IntervalCalculation calc = attempt => attempt * attempt * 50;
            var interval = new BackoffInterval(calc);

            var configuration = ConfigureRetries
                .OnException<ArgumentNullException>()
                .Matching(ex => true)
                .Retry(5, calc);

            Assert.Equal(5, configuration.RetryTimes);
            Assert.True(configuration.Condition(new ArgumentNullException()));
        }
    }
}
