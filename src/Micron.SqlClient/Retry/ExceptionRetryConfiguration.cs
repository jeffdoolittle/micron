namespace Micron.SqlClient.Retry
{
    using System;

    public class ConfigureRetries
    {
        private readonly ExceptionRetryConfiguration configuration;

        private ConfigureRetries()
            => this.configuration = new ExceptionRetryConfiguration();

        public static IExceptionFilterExpression OnException()
        {
            var configureRetries = new ConfigureRetries();
            var configurer = new ExceptionRetryConfigurer(configureRetries.configuration);
            return configurer;
        }

        public static IExceptionFilterExpression OnException<TException>()
            where TException : Exception
        {
            var configureRetries = new ConfigureRetries();
            var configurer = new ExceptionRetryConfigurer(configureRetries.configuration);
            return configurer;
        }

        private class ExceptionRetryConfigurer :
            IExceptionFilterExpression,
            IExceptionRetryExpression,
            IBackoffIntervalExpression
        {
            private readonly ExceptionRetryConfiguration configuration;

            public ExceptionRetryConfigurer(ExceptionRetryConfiguration configuration)
                => this.configuration = configuration;

            public IExceptionRetryExpression Matching(Func<Exception, bool> condition)
            {
                this.configuration.Condition = condition;
                return this;
            }

            public IExceptionRetryExpression Matching<TException>(Func<TException, bool> condition)
                where TException : Exception
            {
                this.configuration.Condition = ex =>
                {
                    if (ex is TException)
                    {
                        return condition(ex as TException);
                    }
                    return false;
                };
                return this;
            }

            public IExceptionRetryConfiguration Retry(RetryTimes times, BackoffInterval backoff)
            {
                this.configuration.RetryTimes = times;
                this.configuration.BackoffInterval = backoff;
                return this.configuration;
            }

            public IExceptionRetryConfiguration Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff)
            {
                this.configuration.RetryTimes = times;
                configureBackoff(this);
                return this.configuration;
            }

            public IExceptionRetryConfiguration Interval(IntervalCalculation intervalCalculation)
            {
                this.configuration.BackoffInterval = intervalCalculation;
                return this.configuration;
            }
        }

        private class ExceptionRetryConfiguration : IExceptionRetryConfiguration
        {
            public Func<Exception, bool> Condition { get; set; }

            public RetryTimes RetryTimes { get; set; }

            public BackoffInterval BackoffInterval { get; set; }
        }
    }

    public interface IExceptionRetryConfiguration
    {
        Func<Exception, bool> Condition { get; }
        RetryTimes RetryTimes { get; }
        BackoffInterval BackoffInterval { get; }
    }

    public interface IExceptionFilterExpression : IExceptionRetryExpression
    {
        IExceptionRetryExpression Matching(Func<Exception, bool> condition);

        IExceptionRetryExpression Matching<TException>(Func<TException, bool> condition)
            where TException : Exception;
    }

    public interface IExceptionRetryExpression
    {
        IExceptionRetryConfiguration Retry(RetryTimes times, BackoffInterval backoff);

        IExceptionRetryConfiguration Retry(RetryTimes times,
            Action<IBackoffIntervalExpression> configureBackoff);
    }

    public interface IBackoffIntervalExpression
    {
        IExceptionRetryConfiguration Interval(IntervalCalculation intervalCalculation);
    }
}
