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
                .Retry(5, backoff => backoff
                    .Interval(attempt => attempt * attempt * 50));

            Assert.Equal(5, configuration.RetryTimes);
            Assert.True(configuration.Condition(new ArgumentException()));
            Assert.False(configuration.Condition(new ArgumentNullException()));
            Assert.False(configuration.Condition(new Exception()));
        }
    }
}
