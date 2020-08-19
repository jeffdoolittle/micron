namespace Micron.SqlClient.Retry
{
    using System;
    using System.Threading.Tasks;

    public interface IRetryHandler
    {
        Task Execute(Func<Task> action);
    }
}
