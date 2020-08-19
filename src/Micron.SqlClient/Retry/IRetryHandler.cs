namespace Micron.SqlClient
{
    using System;
    using System.Threading.Tasks;

    public interface IRetryHandler
    {
        Task Execute(Func<Task> action);
    }
}
