/* 
 *  File: Class1.cs
 *  
 *  Copyright © 2020 Jeff Doolittle.
 *  All rights reserved.
 *  
 *  Licensed under the BSD 3-Clause License. See LICENSE in project root folder for full license text.
 */

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
