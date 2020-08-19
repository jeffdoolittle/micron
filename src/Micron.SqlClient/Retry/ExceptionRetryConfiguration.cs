namespace Micron.SqlClient.Retry
{
    using System;

    public static class ConfigureRetries
    {
        public static IExceptionRetryExpression OnException(Func<Exception, bool> condition) 
            => new ExceptionRetryConfigurer().OnException(condition);

        public static IExceptionRetryExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : Exception => new ExceptionRetryConfigurer().OnException<TException>(condition);

        private class ExceptionRetryConfigurer :
            IExceptionConditionExpression,
            IExceptionRetryExpression,
            IBackoffIntervalExpression
        {
            private readonly ExceptionRetryConfiguration configuration;

            public ExceptionRetryConfigurer() =>
                this.configuration = new ExceptionRetryConfiguration();

            public IExceptionRetryExpression OnException(Func<Exception, bool> condition)
            {
                if (condition is null)
                {
                    throw new ArgumentNullException(nameof(condition));
                }

                this.configuration.Condition = condition;
                return this;
            }

            public IExceptionRetryExpression OnException<TException>(Func<TException, bool> condition = null) 
                where TException : Exception
            {
                if (condition is null)
                {
                    throw new ArgumentNullException(nameof(condition));
                }

                Func<Exception, bool> nonGenericCondition;

                if (condition == null)
                {
                    nonGenericCondition = ex => true;
                }
                else
                {
                    nonGenericCondition = ex =>  condition(ex as TException);
                }

                this.configuration.Condition = nonGenericCondition;
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

            public void Interval(IntervalCalculation intervalCalculation) =>
                this.configuration.BackoffInterval = intervalCalculation;
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

    public interface IExceptionConditionExpression
    {
        IExceptionRetryExpression OnException(Func<Exception, bool> condition);
        IExceptionRetryExpression OnException<TException>(Func<TException, bool> condition = null)
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
        void Interval(IntervalCalculation intervalCalculation);
    }
}
