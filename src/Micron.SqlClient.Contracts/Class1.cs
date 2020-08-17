namespace Micron.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public interface IResultMapper
    {
        Task Map(Func<DbDataReader, Exception, Task> mapper);
    }
}
