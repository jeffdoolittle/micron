namespace Micron.SqlClient.Retry
{
    using System;

    public class ConfigureRetries
    {
        private readonly ExceptionRetryConfiguration configuration;

        private ConfigureRetries()
            => this.configuration = new ExceptionRetryConfiguration();

        public static IExceptionRetryExpression OnException(Func<Exception, bool> condition)
        {
            var configureRetries = new ConfigureRetries();
            var configurer = new ExceptionRetryConfigurer(configureRetries.configuration, condition);
            return configurer;
        }

        public static IExceptionRetryExpression OnException<TException>(Func<Exception, bool> condition = null)
            where TException : Exception
        {
            var configureRetries = new ConfigureRetries();

            condition = ex =>
               {
                   if (ex is TException)
                   {
                       return condition?.Invoke(ex as TException) ?? false;
                   }
                   return condition?.Invoke(ex) ?? false;
               };

            var configurer = new ExceptionRetryConfigurer(configureRetries.configuration, condition);
            return configurer;
        }

        private class ExceptionRetryConfigurer :
            IExceptionRetryExpression,
            IBackoffIntervalExpression
        {
            private readonly ExceptionRetryConfiguration configuration;

            public ExceptionRetryConfigurer(ExceptionRetryConfiguration configuration, Func<Exception, bool> condition)
            {
                if (condition is null)
                {
                    throw new ArgumentNullException(nameof(condition));
                }

                this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                configuration.Condition = condition;
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
