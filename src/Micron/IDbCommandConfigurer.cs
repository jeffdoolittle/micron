namespace Micron
{
    using System.Data.Common;

    public interface IDbCommandConfigurer
    {
        DbCommand Configure(DbCommand command);
    }
}
