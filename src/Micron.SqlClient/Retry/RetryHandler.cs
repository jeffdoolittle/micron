namespace Micron.SqlClient.Retry
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class RetryHandler : IRetryHandler
    {
        private readonly RetryTimes retryTimes;
        private readonly BackoffInterval backoffInterval;
        private readonly Func<Exception, bool>[] conditions;

        public RetryHandler(params Func<Exception, bool>[] conditions)
        {
            this.retryTimes = RetryTimes.MaxRetries;
            this.backoffInterval = BackoffInterval.MinBackoffMilliseconds;
            this.conditions = conditions;
        }

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
                    if (tries++ > this.retryTimes.RetryCount)
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

        public static IRetryTimesExpression OnException(Func<Exception, bool> condition)
                  => new RetryConfigurer().OnException(condition);

        public static IRetryTimesExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : Exception => new RetryConfigurer().OnException<TException>(condition);

        private class RetryConfigurer :
            IConditionExpression,
            IRetryTimesExpression,
            IBackoffIntervalExpression
        {
            private readonly ExceptionRetryConfiguration configuration;

            public RetryConfigurer() =>
                this.configuration = new ExceptionRetryConfiguration();

            public IRetryTimesExpression OnException(Func<Exception, bool> condition = null)
                => this.OnException<Exception>(condition);

            public IRetryTimesExpression OnException<TException>(Func<TException, bool> condition = null)
                where TException : Exception
            {
                Func<Exception, bool> nonGenericCondition;

                if (condition == null)
                {
                    nonGenericCondition = ex => true;
                }
                else
                {
                    nonGenericCondition = ex => condition(ex as TException);
                }

                this.configuration.Condition = nonGenericCondition;
                return this;
            }

            public IRetryHandler Retry(RetryTimes times, BackoffInterval backoff)
            {
                this.configuration.RetryTimes = times;
                this.configuration.BackoffInterval = backoff;
                return new RetryHandler(this.configuration.RetryTimes,
                    this.configuration.BackoffInterval,
                    this.configuration.Condition);
            }

            public IRetryHandler Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff)
            {
                this.configuration.RetryTimes = times;
                configureBackoff(this);
                return new RetryHandler(this.configuration.RetryTimes,
                    this.configuration.BackoffInterval,
                    this.configuration.Condition);
            }

            public void Interval(IntervalCalculation intervalCalculation) =>
                this.configuration.BackoffInterval = intervalCalculation;
        }

        private class ExceptionRetryConfiguration
        {
            public Func<Exception, bool> Condition { get; set; }

            public RetryTimes RetryTimes { get; set; }

            public BackoffInterval BackoffInterval { get; set; }
        }
    }

    public interface IConditionExpression
    {
        IRetryTimesExpression OnException(Func<Exception, bool> condition);
        IRetryTimesExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : Exception;
    }

    public interface IRetryTimesExpression
    {
        IRetryHandler Retry(RetryTimes times, BackoffInterval backoff);

        IRetryHandler Retry(RetryTimes times,
            Action<IBackoffIntervalExpression> configureBackoff);
    }

    public interface IBackoffIntervalExpression
    {
        void Interval(IntervalCalculation intervalCalculation);
    }
}
