// // namespace Micron
// // {
// //     using System;
// //     using System.Data.Common;
// //     using Micron.Retry;
// //     using Microsoft.Extensions.Logging;

// //     public interface IMicronFactory
// //     {

// //     }

// //     public class MicronConfiguration : IMicronConfiguration
// //     {
//     public static class Wireup
//     {
//         public static IMicronFactory Micron(Action<IMicronWireup> configure)
//         {
//             return null;
//         }
//     }


//     public interface IMicronFactory
//     {

//     }

//     public class MicronFactory : IMicronFactory
//     {

// //     }

// //     public interface IMicronConfiguration
// //     {
// //         DbProviderFactory ProviderFactory { get; }
// //         DbConnectionStringBuilder ConnectionStringBuilder { get; }
// //         IRetryHandler RetryHandler { get; }
// //         ILogger Logger { get; }
// //     }
//     public interface IMicronProvider<TProvider>
//         where TProvider : DbProviderFactory
//     {

// //     public class MicronFactory : IMicronFactory
// //     {

// //     }

// //     public interface IMicronProvider<TProvider>
// //         where TProvider : DbProviderFactory
// //     {

// //     }


// //     public abstract class MicronWireup<TProvider> : IMicronWireup<TProvider>
// //         where TProvider : DbProviderFactory
// //     {
// //         public ILogger Logger { get; set; }
// //     }


// //     public interface IMicronWireup<TProvider>
// //         where TProvider : DbProviderFactory
// //     {

// //     }


// //     public interface IMicronWireup
// //     {
// //         void Provider<TProvider>(TProvider provider) where TProvider : DbProviderFactory;
//     public interface IMicronConfiguration
//     {
//         DbProviderFactory ProviderFactory { get; set; }
//         DbConnectionStringBuilder ConnectionStringBuilder { get; set; }
//         Func<DbCommand, DbCommand> CommandConfigurer { get; set; }
//         Func<DbParameter, DbParameter> ParameterConfigurer { get; set; }
//         ILogger Logger { get; set; }
//         IRetryHandler RetryHandler { get; set; }
//     }

//     public interface IMicronProviderFactoryConfiguration<TDbProviderFactory>
//         where TDbProviderFactory : DbProviderFactory
//     {

//     }

//     public interface IMicronConfiguration<TDbProviderFactory, TDbConnectionStringBuilder, TDbCommand, TDbParameter, TDbException>
//         : IMicronConfiguration
//         where TDbProviderFactory : DbProviderFactory
//         where TDbConnectionStringBuilder : DbConnectionStringBuilder
//         where TDbCommand : DbCommand
//         where TDbParameter : DbParameter
//         where TDbException : DbException
//     {
//         TDbProviderFactory TypedProviderFactory { get; set; }
//         TDbConnectionStringBuilder TypedConnectionStringBuilder { get; set; }

//         Func<TDbCommand, TDbCommand> TypedCommandConfigurer { get; set; }

//         Func<TDbParameter, TDbParameter> TypedParameterConfigurer { get; set; }


//     }

// //         void ConnectionString(Func<string> connectionStringFunction);

// //         void ConnectionConfigurationKey(string connectionConfigurationKey);
// //     }


// //     public interface IMicronConfigurer<TBuilder, TDbCommand, TDbException>
// //         where TBuilder : DbConnectionStringBuilder
// //         where TDbCommand : DbCommand
// //         where TDbException : DbException
// //     {
// //         IMicronConfigurer<TBuilder, TDbCommand, TDbException> Build(
// //             Action<TBuilder> configure);

// //         IMicronConfigurer<TBuilder, TDbCommand, TDbException> OnException(
// //             Func<IConditionExpression, IRetryHandler> configure);

// //         IMicronConfigurer<TBuilder, TDbCommand, TDbException> LogTo(
// //             ILogger logger);

// //         IMicronConfigurer<TBuilder, TDbCommand, TDbException> Command(
// //             Action<TDbCommand> pipeline);
// //     }

// // }
