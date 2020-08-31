// namespace Micron.Tests
// {
//     using System;
//     using System.Data;
//     using System.Data.SQLite;
//     using System.Threading.Tasks;
//     using Micron.Retry;
//     using Microsoft.Extensions.Logging.Abstractions;
//     using Xunit;

//     public class IntegrationTests
//     {
//         [Fact]
//         public async Task Can_pull_it_all_together()
//         {
//             var logger = NullLogger<IDbCommandHandler>.Instance;

//             var connectionFactory = new SqliteConnectionFactory(
//                 "Data Source=:memory:", _ =>
//                  {
//                      _.DateTimeFormat = SQLiteDateFormats.ISO8601;
//                      _.DateTimeKind = DateTimeKind.Utc;
//                      _.DefaultDbType = DbType.String;
//                      _.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
//                      _.DefaultTimeout = 5;
//                      _.BusyTimeout = 5;
//                      _.Pooling = true;
//                  });

//             var retry = RetryHandler.Catch<SQLiteException>(ex =>
//             {
//                 var retry = ex.ResultCode switch
//                 {
//                     SQLiteErrorCode.Busy => true,
//                     SQLiteErrorCode.Locked => true,
//                     SQLiteErrorCode.IoErr => true,
//                     _ => false
//                 };

//                 return retry;

//             }).Retry(new RetryTimes(3), new BackoffInterval(attempt => attempt * attempt * 100));

//             // var commandFactory = new MicronCommandFactory();

//             // var command = commandFactory.CreateCommand("");


//             // using var conn = connectionFactory.CreateConnection();


//         }
//     }

//     public class SqliteConnectionFactory :
//         DbConnectionFactory<SQLiteFactory, SQLiteConnectionStringBuilder>
//     {
//         public SqliteConnectionFactory(string connectionString,
//             Action<SQLiteConnectionStringBuilder>? configureConnectionString = null) :
//                 base(new SQLiteFactory(), new SQLiteConnectionStringBuilder(connectionString)) =>
//                     configureConnectionString?.Invoke(this.ConnectionStringBuilder);
//     }
// }
