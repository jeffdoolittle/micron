namespace Micron.Tests
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using Micron.Retry;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class MicronFactoryTests
    {
        [Fact]
        public void Can_configure_micron_factory()
        {
            var factory = Wireup.Micron(_ => _
                // .SQLite("connection string configuration key")
                .SQLite(() => "connection-string")
                .Connection<SQLiteConnectionStringBuilder>(builder => 
                    builder.DateTimeFormat = SQLiteDateFormats.ISO8601)
                .Retry(retry => retry.OnException<SQLiteException>().Retry(3))
                .Logger(() => NullLogger.Instance)                
            );



            Console.WriteLine();

            // var factory = new MicronFactory();


            // static void exec(Action<IMicronFactoryConfigurationRootExpression> micron)
            // {
            // }

            // exec(micron => micron
            //     .Provider(() => SQLiteFactory.Instance)
            //     .Connect(() => "")
            //     .Builder(() => new SQLiteConnectionStringBuilder())
            // );

            // exec(micron => micron
            //     .SQLite()
            // .Provider(() => SQLiteFactory.Instance)
            // .Connect(() => "")
            // .Builder(() => new SQLiteConnectionStringBuilder())
            // );


            // specify provider and connections string builder types
            // configure retry logic
            // configure logging
            // configure other default values?
        }
    }

    public static class Wireup
    {
        public static IMicronFactory Micron(Action<IMicronWireup> configure)
        {
            return null;
        }
    }

    public interface IMicronWireup
    {

    }

    public interface IMicronConfigurer
    {
        IMicronConfigurer Connection<TBuilder>(Action<TBuilder> configure)
            where TBuilder : DbConnectionStringBuilder;

        IMicronConfigurer Retry(Func<IConditionExpression, IRetryHandler> configure);

        IMicronConfigurer Logger(Func<ILogger> logger);

    }

    public static class SQLiteMicronFactoryExtensions
    {
        public static IMicronConfigurer SQLite(this IMicronWireup wireup, Func<string> connectionStringFunction)
        {
            return null;
        }

        public static IMicronConfigurer SQLite(this IMicronWireup wireup, string connectionStringConfigurationKey = "Default")
        {
            return null;
        }
    }

    // public abstract class DbConnectionConfigurer
    // {

    // }

    // public interface IMicronFactoryConfigurationRootExpression
    // {
    //     IMicronFactoryConfigurationConnectionSourceExpression Provider<TProviderFactory>(
    //         Func<TProviderFactory> factory
    //     )
    //     where TProviderFactory : DbProviderFactory;
    // }

    // public interface IMicronFactoryConfigurationConnectionSourceExpression
    // {
    //     /// <summary>
    //     /// Supply a function that returns a connection string.
    //     /// </summary>
    //     /// <param name="connectionStringFunction"></param>
    //     /// <returns></returns>
    //     IMicronFactoryConfigurationProviderExpression Connect(Func<string> connectionStringFunction);

    //     /// <summary>
    //     /// Supply a configuration key from which to resolve a connection string.
    //     /// <see cref="Microsoft.Extensions.Configuration.IConfiguration" />.
    //     /// </summary>
    //     /// <param name="connectionStringConfigurationKey"></param>
    //     /// <returns></returns>
    //     IMicronFactoryConfigurationProviderExpression Connect(string connectionStringConfigurationKey);
    // }

    // public interface IMicronFactoryConfigurationProviderExpression<TProviderFactory>
    //     where TProviderFactory : DbProviderFactory
    // {
    //     IMicronFactoryConfigurationBuilderExpression<TProviderFactory, TBuilder> Builder<TBuilder>(
    //         Func<TBuilder> factory            
    //     )
    //     where TBuilder : DbConnectionStringBuilder;
    // }

    // public interface IMicronFactoryConfigurationBuilderExpression<TProviderFactory, TBuilder>
    //     where TProviderFactory : DbProviderFactory
    //     where TBuilder : DbConnectionStringBuilder
    // {

    // }

    // public interface IMicronFactoryConfigurationExpression<TProviderFactory>
    //     where TProviderFactory : DbProviderFactory
    // {

    // }
}
