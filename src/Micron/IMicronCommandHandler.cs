namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMicronCommandHandler
    {
        void Read(MicronCommand command, Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default);

        T Scalar<T>(MicronCommand command) where T : struct;

        string String(MicronCommand command);

        int Execute(MicronCommand command);

        void Transaction(MicronCommand[] commands,
            Action<int, int>? resultIndexAndAffectedCallback = null);

        int Batch(IEnumerable<MicronCommand> commands, int batchSize);

        Task ReadAsync(MicronCommand command,
            Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default);

        Task<T> ScalarAsync<T>(MicronCommand command,
            CancellationToken ct = default)
            where T : struct;

        Task<string> StringAsync(MicronCommand command,
            CancellationToken ct = default);

        Task<int> ExecuteAsync(MicronCommand command,
            CancellationToken ct = default);

        Task TransactionAsync(MicronCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null);

        Task<int> BatchAsync(IEnumerable<MicronCommand> commands, int batchSize,
            CancellationToken ct = default);
    }
}
