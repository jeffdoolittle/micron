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
            if (condition is null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var configureRetries = new ConfigureRetries();
            configureRetries.configuration.Condition = condition;
            var configurer = new ExceptionRetryConfigurer(configureRetries);
            return configurer;
        }

        public static IExceptionRetryExpression OnException<TException>(Func<Exception, bool> condition = null)
            where TException : Exception
        {
            if (condition is null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var configureRetries = new ConfigureRetries();

            condition = ex =>
               {
                   if (ex is TException)
                   {
                       return condition?.Invoke(ex as TException) ?? false;
                   }
                   return condition?.Invoke(ex) ?? false;
               };

            configureRetries.configuration.Condition = condition;
            var configurer = new ExceptionRetryConfigurer(configureRetries);
            return configurer;
        }

        private class ExceptionRetryConfigurer :
            IExceptionRetryExpression,
            IBackoffIntervalExpression
        {
            private readonly ConfigureRetries parent;

            public ExceptionRetryConfigurer(ConfigureRetries parent)
                => this.parent = parent;

            public IExceptionRetryConfiguration Retry(RetryTimes times, BackoffInterval backoff)
            {
                this.parent.configuration.RetryTimes = times;
                this.parent.configuration.BackoffInterval = backoff;
                return this.parent.configuration;
            }

            public IExceptionRetryConfiguration Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff)
            {
                this.parent.configuration.RetryTimes = times;
                configureBackoff(this);
                return this.parent.configuration;
            }

            public void Interval(IntervalCalculation intervalCalculation) =>
                this.parent.configuration.BackoffInterval = intervalCalculation;
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
        void Interval(IntervalCalculation intervalCalculation);
    }
}
