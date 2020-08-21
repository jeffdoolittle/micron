namespace Micron.Core
{
    using System.Data.Common;

    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
