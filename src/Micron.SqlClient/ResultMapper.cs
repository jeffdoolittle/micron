namespace Micron.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public class ResultMapper : IResultMapper
    {
        public Task Map(Func<DbDataReader, Exception, Task> mapper)
        {
            throw new NotImplementedException();
        }
    }
}
