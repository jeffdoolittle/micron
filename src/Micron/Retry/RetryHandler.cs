namespace Micron.Retry
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
            this.conditions = conditions ?? new Func<Exception, bool>[0];
        }

        public RetryHandler(RetryTimes retryTimes, BackoffInterval backoffInterval,
            params Func<Exception, bool>[] conditions)
        {
            this.retryTimes = retryTimes;
            this.backoffInterval = backoffInterval;
            this.conditions = conditions;
        }

        public void Execute(Action action) =>
            _ = this.Execute(() =>
                {
                    action();
                    return Task.CompletedTask;
                });

        public T Execute<T>(Func<T> function)
        {
            var tries = 0;
            do
            {
                try
                {
                    return function();
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
                    this.backoffInterval.Backoff(tries);
                }
            } while (true);
        }

        public async Task ExecuteAsync(Func<Task> action) =>
            _ = await this.ExecuteAsync(async () =>
                {
                    await action();
                    return Unit.Default;
                });

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> function)
        {
            var tries = 0;
            do
            {
                try
                {
                    return await function().ConfigureAwait(false);
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
                    await this.backoffInterval.BackoffAsync(tries).ConfigureAwait(false);
                }
            } while (true);
        }

        public static IRetryTimesExpression Catch(Action<IConditionExpression> configure)
        {
            var configurer = new RetryConfigurer();
            configure(configurer);
            return configurer;
        }

        public static IRetryTimesExpression Catch<TException>(Func<TException, bool>? condition = null)
            where TException : Exception =>
                new RetryConfigurer().OnException(condition);

        private class RetryConfigurer :
            IConditionExpression,
            IRetryTimesExpression,
            IBackoffIntervalExpression
        {
            private readonly ExceptionRetryConfiguration configuration;

            public RetryConfigurer() =>
                this.configuration = new ExceptionRetryConfiguration();

            public IRetryTimesExpression OnException(Func<Exception, bool>? condition = null)
                => this.OnException<Exception>(condition);

            public IRetryTimesExpression OnException<TException>(Func<TException, bool>? condition = null)
                where TException : Exception
            {
                Func<Exception, bool> nonGenericCondition;

                if (condition == null)
                {
                    nonGenericCondition = ex => true;
                }
                else
                {
                    nonGenericCondition = ex =>
                        ex is TException typedEx && condition(typedEx);
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
                    this.configuration.Condition ?? (ex => false));
            }

            public IRetryHandler Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff)
            {
                this.configuration.RetryTimes = times;
                configureBackoff(this);
                return new RetryHandler(this.configuration.RetryTimes,
                    this.configuration.BackoffInterval,
                    this.configuration.Condition ?? (ex => false));
            }

            public void Interval(IntervalCalculation intervalCalculation) =>
                this.configuration.BackoffInterval = intervalCalculation;
        }

        private class ExceptionRetryConfiguration
        {
            public Func<Exception, bool>? Condition { get; set; }

            public RetryTimes RetryTimes { get; set; }

            public BackoffInterval BackoffInterval { get; set; }
        }
    }

    public interface IConditionExpression
    {
        IRetryTimesExpression OnException(Func<Exception, bool> condition);
        IRetryTimesExpression OnException<TException>(Func<TException, bool>? condition = null)
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
