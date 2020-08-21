namespace Micron.Retry
{
    using System.Threading.Tasks;
    using Xunit;

    public class BackoffIntervalTests
    {
        [Fact]
        public async Task Can_specify_interval_calculation()
        {
            var callCount = 0;

            int calc(int attempt)
            {
                callCount++;
                return attempt * attempt * BackoffInterval.MinBackoffMilliseconds;
            };

            var interval = new BackoffInterval(calc);

            // the BackoffInterval constructor validates the intervalCalculation 
            // function, which increments callCount. Reset it here to validate 
            // the number of actual backoff retries.
            callCount = 0;

            for (var i = 0; i < RetryTimes.MaxRetries; i++)
            {
                await interval.BackoffAsync(i + 1);
            }

            Assert.Equal(5, callCount);
        }
    }
}
