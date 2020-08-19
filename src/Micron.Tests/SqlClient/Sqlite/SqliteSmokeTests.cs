namespace Micron.SqlClient.Sqlite
{
    using System.Data.SQLite;
    using System.Threading.Tasks;
    using Xunit;

    public class SqliteSmokeTests
    {
        [Fact]
        public async Task Can_open_an_sqlite_connection()
        {
            using var conn = new SQLiteConnection("Data Source=file:memdb1?mode=memory&cache=shared");
            await conn.OpenAsync();      
            await conn.CloseAsync();
        }
    }
}
