namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public delegate int IntervalCalculation(int attempt);

    public struct BackoffInterval
    {
        public const int MaxBackoffMilliseconds = 30000;
        public const int MinBackoffMilliseconds = 50;

        private readonly IntervalCalculation intervalCalculation;

        public BackoffInterval(TimeSpan interval) : this(_ => (int)interval.TotalMilliseconds) { }

        public BackoffInterval(int intervalMilliseconds) : this(_ => intervalMilliseconds) { }

        public BackoffInterval(IntervalCalculation intervalCalculation)
        {
            for (var i = 0; i < RetryTimes.MaxRetries; i++)
            {
                var interval = intervalCalculation(i + 1);
                if (interval < MinBackoffMilliseconds)
                {
                    throw new ArgumentOutOfRangeException(nameof(intervalCalculation), interval,
                        $"On retry count {i + 1} the calulated interval deceeded the minimum backoff " +
                        $"interval of {MinBackoffMilliseconds} milliseconds.");
                }
                if (interval > MaxBackoffMilliseconds)
                {
                    throw new ArgumentOutOfRangeException(nameof(intervalCalculation), interval,
                        $"On retry count {i + 1} the calulated interval exceeded the maximum backoff " +
                        $"interval of {MaxBackoffMilliseconds} milliseconds.");
                }
            }

            this.intervalCalculation = intervalCalculation;
        }

        /// <summary>
        /// Delay the running task for the specified interval.
        /// </summary>
        /// <returns></returns>
        public async Task Backoff(int retryCount)
        {
            if (retryCount > RetryTimes.MaxRetries)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount,
                    $"Value must be between 1 and {RetryTimes.MaxRetries} (inclusive).");
            }

            var interval = this.intervalCalculation(retryCount);
            await Task.Delay(interval).ConfigureAwait(true);
        }

        public override bool Equals(object obj) => 
            obj is BackoffInterval interval &&
                EqualityComparer<IntervalCalculation>.Default.Equals(this.intervalCalculation, interval.intervalCalculation);

        public override int GetHashCode() => 
            HashCode.Combine(this.intervalCalculation);

        public static implicit operator BackoffInterval(TimeSpan interval)
            => new BackoffInterval(interval);

        public static implicit operator BackoffInterval(int intervalMilliseconds)
            => new BackoffInterval(intervalMilliseconds);

        public static implicit operator BackoffInterval(IntervalCalculation intervalCalculation)
            => new BackoffInterval(intervalCalculation);
    }
}
