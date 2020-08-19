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

            public IExceptionRetryExpression OnException(Func<Exception, bool> condition = null) 
                => this.OnException<Exception>(condition);

            public IExceptionRetryExpression OnException<TException>(Func<TException, bool> condition = null) 
                where TException : Exception
            {
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

    public interface IExceptionConditionExpression
    {
        IExceptionRetryExpression OnException(Func<Exception, bool> condition);
        IExceptionRetryExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : Exception;
    }

    public interface IExceptionRetryExpression
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
