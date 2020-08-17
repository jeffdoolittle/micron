namespace Micron.SqlClient.Sqlite
{
    using System;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Threading.Tasks;
    using Xunit;

    public class SqlGatewayTests
    {
        [Fact]
        public async Task Can_do_stuff()
        {
            static async Task<DbConnection> openConnection()
            {
                var conn = new SQLiteConnection("Data Source=file:memdb1?mode=memory&cache=shared");
                await conn.OpenAsync();
                return conn;
            }

            var gateway = new SqlGateway(_ => _
                .Connection(openConnection)
                .OnException<ArgumentException>(ex => ex
                    .Retry(5, backoff =>
                        backoff.Interval(attempt => attempt * attempt * 50))
                )
            );

            var value = await gateway.Scalar<int>("select 1, count(*) from sqlite_master");

            Assert.Equal(1, value);
        }
    }
}
