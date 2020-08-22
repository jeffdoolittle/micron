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
                .SQLite(() => "connection-string") // or key from configuration
                .LogTo(NullLogger.Instance)
                .Build(builder =>
                    {
                        builder.DateTimeFormat = SQLiteDateFormats.ISO8601;
                        builder.DateTimeKind = DateTimeKind.Utc;
                        builder.DefaultTimeout = 5;
                    })
                .Retry(retry => retry
                    .OnException<SQLiteException>(condition => true)
                    .Retry(3))
                .Command(cmd => cmd.CommandTimeout = 5)
            );



            Console.WriteLine();
        }
    }

    public class SqliteMicronWireup : MicronWireup<SQLiteFactory>
    {

    }


    public static class Wireup
    {
        public static IMicronFactory Micron(Action<IMicronWireup> configure)
        {
            return null;
        }
    }

    public static class SQLiteMicronFactoryExtensions
    {
        public static ISQLiteMicronConfigurer SQLite(
            this IMicronWireup wireup, Func<string> connectionStringFunction)
        {
            wireup.Provider(new SQLiteFactory());
            wireup.ConnectionString(connectionStringFunction);

            return null;
        }

        public static ISQLiteMicronConfigurer SQLite(
            this IMicronWireup wireup, string connectionStringConfigurationKey = "Default")
        {
            wireup.Provider(new SQLiteFactory());
            wireup.ConnectionConfigurationKey(connectionStringConfigurationKey);

            return null;
        }
    }


    public interface ISQLiteMicronConfigurer :
        IMicronConfigurer<SQLiteConnectionStringBuilder, SQLiteCommand, SQLiteException>
    {

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
