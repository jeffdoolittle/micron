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
    using System.Linq;
    using System.Threading.Tasks;

    public class RetryHandler
    {
        private readonly RetryTimes retryTimes;
        private readonly BackoffInterval backoffInterval;
        private readonly Func<Exception, bool>[] conditions;

        public RetryHandler(RetryTimes retryTimes, BackoffInterval backoffInterval,
            params Func<Exception, bool>[] conditions)
        {
            this.retryTimes = retryTimes;
            this.backoffInterval = backoffInterval;
            this.conditions = conditions;
        }

        public async Task Execute(Func<Task> action)
        {
            var tries = 0;
            do
            {
                try
                {
                    await action().ConfigureAwait(true);
                    break;
                }
                catch (Exception ex)
                {
                    if (++tries > this.retryTimes.RetryCount)
                    {
                        throw;
                    }
                    if (!this.conditions.Any(condition => condition(ex)))
                    {
                        throw;
                    }
                    await this.backoffInterval.Backoff(tries).ConfigureAwait(true);
                }
            } while (true);
        }
    }
}
