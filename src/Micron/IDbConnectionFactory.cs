namespace Micron
{
    using System.Data.Common;

    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
