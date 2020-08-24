namespace Micron.Retry
{
    using System;
    using System.Threading.Tasks;

    public interface IRetryHandler
    {
        void Execute(Action<int> action);

        Task ExecuteAsync(Func<int, Task> action);

        T Execute<T>(Func<int, T> function);

        Task<T> ExecuteAsync<T>(Func<int, Task<T>> function);
    }
}
