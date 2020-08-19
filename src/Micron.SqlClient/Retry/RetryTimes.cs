namespace Micron.SqlClient.Retry
{
    using System;

    public struct RetryTimes
    {
        public const int MaxRetries = 5;

        public RetryTimes(int retryCount)
        {
            if (retryCount < 1 || retryCount > MaxRetries)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount,
                    $"Value must be between 1 and {MaxRetries} (inclusive).");
            }

            this.RetryCount = retryCount;
        }

        public int RetryCount { get; }

        public override bool Equals(object? obj) => 
            obj is RetryTimes times &&
                this.RetryCount == times.RetryCount;

        public override int GetHashCode() => 
            HashCode.Combine(this.RetryCount);

        public static implicit operator RetryTimes(int retryCount) => new RetryTimes(retryCount);
    }
}
