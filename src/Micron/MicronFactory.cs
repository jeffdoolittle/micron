namespace Micron
{
    using System;
    using System.Data.Common;
    using Micron.Retry;
    using Microsoft.Extensions.Logging;

    public interface IMicronFactory
    {

    }

    public interface IMicronConfiguration
    {
        DbProviderFactory ProviderFactory { get; }
        DbConnectionStringBuilder ConnectionStringBuilder { get; }
        IRetryHandler RetryHandler { get; }
    }
    public class MicronFactory : IMicronFactory
    {

    }

    public interface IMicronProvider<TProvider> 
        where TProvider : DbProviderFactory
    {

    }


    public abstract class MicronWireup<TProvider> : IMicronWireup<TProvider>
        where TProvider : DbProviderFactory
    {
        public ILogger Logger { get; set; }
    }


    public interface IMicronWireup<TProvider>
        where TProvider : DbProviderFactory
    {

    }


    public interface IMicronWireup
    {
        void Provider<TProvider>(TProvider provider) where TProvider : DbProviderFactory;

        void ConnectionString(Func<string> connectionStringFunction);

        void ConnectionConfigurationKey(string connectionConfigurationKey);
    }


    public interface IMicronConfigurer<TBuilder, TDbCommand, TDbException>
        where TBuilder : DbConnectionStringBuilder
        where TDbCommand : DbCommand
        where TDbException : DbException
    {
        IMicronConfigurer<TBuilder, TDbCommand, TDbException> Build(
            Action<TBuilder> configure);

        IMicronConfigurer<TBuilder, TDbCommand, TDbException> OnException(
            Func<IConditionExpression, IRetryHandler> configure);

        IMicronConfigurer<TBuilder, TDbCommand, TDbException> LogTo(
            ILogger logger);

        IMicronConfigurer<TBuilder, TDbCommand, TDbException> Command(
            Action<TDbCommand> pipeline);
    }

}
