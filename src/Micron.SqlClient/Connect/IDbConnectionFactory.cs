namespace Micron.SqlClient.Connect
{
    using System.Data.Common;

    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
