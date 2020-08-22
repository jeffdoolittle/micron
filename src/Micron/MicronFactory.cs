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


    public interface IMicronConfigurer
    {
        IMicronConfigurer Build<TBuilder>(Action<TBuilder> configure)
            where TBuilder : DbConnectionStringBuilder;

        IMicronConfigurer Retry(Func<IConditionExpression, IRetryHandler> configure);

        IMicronConfigurer LogTo(ILogger logger);

        IMicronConfigurer Command<TDbCommand>(Action<TDbCommand> pipeline)
            where TDbCommand : DbCommand;
    }

}
