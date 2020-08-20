namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDataCommand
    {
        void Multiple___();

        void Read(Action<IDataRecord> callback, 
            CommandBehavior behavior = CommandBehavior.Default);

        T Scalar<T>() where T : struct;

        string String();

        int Execute();

        Task ReadAsync(Func<IDataRecord, Task> callback, 
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default);

        Task<T> ScalarAsync<T>(CancellationToken ct = default) 
            where T : struct;

        Task<string> StringAsync(CancellationToken ct = default);

        Task<int> ExecuteAsync(CancellationToken ct = default);
    }
}
