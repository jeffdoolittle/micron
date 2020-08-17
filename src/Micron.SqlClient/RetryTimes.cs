namespace Micron.SqlClient
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

        public static implicit operator RetryTimes(int retryCount) => new RetryTimes(retryCount);
    }
}
