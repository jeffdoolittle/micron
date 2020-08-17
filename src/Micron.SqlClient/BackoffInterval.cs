/* 
 *  File: BackoffInterval.cs
 *  
 *  Copyright © 2020 Jeff Doolittle.
 *  All rights reserved.
 *  
 *  Licensed under the BSD 3-Clause License. See LICENSE in project root folder for full license text.
 */

namespace Micron.SqlClient
{
    using System;
    using System.Threading.Tasks;

    public struct BackoffInterval
    {
        public const int MaxBackoffMilliseconds = 30000;
        public const int MinBackoffMilliseconds = 250;

        private readonly Func<int, int> intervalCalculation;

        public BackoffInterval(TimeSpan interval) : this(_ => (int)interval.TotalMilliseconds) { }

        public BackoffInterval(int intervalMilliseconds) : this(_ => intervalMilliseconds) { }

        public BackoffInterval(Func<int, int> intervalCalculation)
        {
            for (var i = 0; i < RetryTimes.MaxRetries; i++)
            {
                var interval = intervalCalculation(i);
                if (interval < MinBackoffMilliseconds)
                {
                    throw new ArgumentOutOfRangeException(nameof(intervalCalculation), interval,
                        $"On retry count {i + 1} the calulated interval deceeded the minimum backoff " +
                        $"interval of {MaxBackoffMilliseconds} milliseconds.");
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

        public static implicit operator BackoffInterval(TimeSpan interval)
            => new BackoffInterval(interval);

        public static implicit operator BackoffInterval(int intervalMilliseconds)
            => new BackoffInterval(intervalMilliseconds);

        public static implicit operator BackoffInterval(Func<int, int> intervalCalculation)
            => new BackoffInterval(intervalCalculation);
    }
}
