namespace Micron.SqlClient.Retry
{
    using System;
    using System.Threading.Tasks;

    public interface IRetryHandler
    {
        void Execute(Action action);

        Task ExecuteAsync(Func<Task> action);

        T Execute<T>(Func<T> function);

        Task<T> ExecuteAsync<T>(Func<Task<T>> function);
    }
}
