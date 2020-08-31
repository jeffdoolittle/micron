namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDbCommandHandler
    {
        void Read(DbCommand command, Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default);

        T Scalar<T>(DbCommand command) where T : struct;

        string String(DbCommand command);

        int Execute(DbCommand command);

        void Batch(DbCommand[] commands,
            bool useTransaction,
            Action<int, int>? resultIndexAndAffectedCallback = null);

        Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default);

        Task<T> ScalarAsync<T>(DbCommand command, CancellationToken ct = default)
            where T : struct;

        Task<string> StringAsync(DbCommand command, CancellationToken ct = default);

        Task<int> ExecuteAsync(DbCommand command, CancellationToken ct = default);

        Task BatchAsync(DbCommand[] commands,
            bool useTransaction,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null,
            CancellationToken ct = default);
    }
}
