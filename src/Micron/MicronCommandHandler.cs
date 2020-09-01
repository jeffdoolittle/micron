namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class MicronCommandHandler : IMicronCommandHandler
    {
        private readonly IDbConnectionFactory dbConnectionFactory;
        private readonly IDbCommandHandler dbCommandHandler;
        public MicronCommandHandler(IDbConnectionFactory dbConnectionFactory, IDbCommandHandler dbCommandHandler)
        {
            this.dbConnectionFactory = dbConnectionFactory;
            this.dbCommandHandler = dbCommandHandler;
        }

        public int Execute(MicronCommand command)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var affected = this.dbCommandHandler.Execute(dbCommand);
            conn.Close();
            return affected;
        }

        public async Task<int> ExecuteAsync(MicronCommand command, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var affected = await this.dbCommandHandler.ExecuteAsync(dbCommand, ct);
            await conn.CloseAsync();
            return affected;
        }

        public void Read(MicronCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            this.dbCommandHandler.Read(dbCommand, callback, behavior);
            conn.Close();
        }

        public async Task ReadAsync(MicronCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            await this.dbCommandHandler.ReadAsync(dbCommand, callback, behavior, ct);
            await conn.CloseAsync();
        }

        public T Scalar<T>(MicronCommand command) where T : struct
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var value = this.dbCommandHandler.Scalar<T>(dbCommand);
            conn.Close();
            return value;
        }

        public async Task<T> ScalarAsync<T>(MicronCommand command, CancellationToken ct = default)
            where T : struct
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var value = await this.dbCommandHandler.ScalarAsync<T>(dbCommand, ct);
            await conn.CloseAsync();
            return value;
        }

        public string String(MicronCommand command)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var value = this.dbCommandHandler.String(dbCommand);
            conn.Close();
            return value;
        }

        public async Task<string> StringAsync(MicronCommand command, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            var value = await this.dbCommandHandler.StringAsync(dbCommand, ct);
            await conn.CloseAsync();
            return value;
        }

        public void Transaction(MicronCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();

            var dbCommands = commands.Select(x =>
            {
                var dbCommand = conn.CreateCommand();
                x.MapTo(dbCommand);
                return dbCommand;
            });

            this.dbCommandHandler.Transaction(dbCommands.ToArray(), resultIndexAndAffectedCallback);

            foreach (var cmd in dbCommands)
            {
                cmd.Dispose();
            }

            conn.Close();
        }

        public async Task TransactionAsync(MicronCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);

            var dbCommands = commands.Select(x =>
            {
                var dbCommand = conn.CreateCommand();
                x.MapTo(dbCommand);
                return dbCommand;
            });

            await this.dbCommandHandler.TransactionAsync(dbCommands.ToArray(), resultIndexAndAffectedCallback, ct);

            foreach (var cmd in dbCommands)
            {
                await cmd.DisposeAsync();
            }

            await conn.CloseAsync();
        }

        public void Batch(IEnumerable<MicronCommand> commands, int batchSize,
            Action<int, int>? batchIndexAndAffectedCallback = null)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();

            var batchIndex = 0;

            var batch = new List<DbCommand>();
            var batchAffected = 0;
            foreach (var cmd in commands)
            {
                var dbCommand = conn.CreateCommand();
                cmd.MapTo(dbCommand);
                batch.Add(dbCommand);

                if (batch.Count == batchSize)
                {
                    this.dbCommandHandler.Transaction(batch.ToArray(), (i, x) => batchAffected += x);
                    batchIndexAndAffectedCallback?.Invoke(batchIndex, batchAffected);
                    batch.ForEach(cmd => cmd.Dispose());
                    batch.Clear();
                    batchAffected = 0;
                    batchIndex++;
                }
            }

            if (batch.Count > 0)
            {
                this.dbCommandHandler.Transaction(batch.ToArray(), (i, x) => batchAffected += x);
                batchIndexAndAffectedCallback?.Invoke(batchIndex, batchAffected);
                batch.ForEach(cmd => cmd.Dispose());
                batch.Clear();
            }

            conn.Close();
        }

        public async Task BatchAsync(IAsyncEnumerable<MicronCommand> commands, int batchSize,
            CancellationToken ct = default,
            Func<int, int, Task>? batchIndexAndAffectedCallback = null)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);

            var batchIndex = 0;
            await foreach (var set in commands.InSetsOf(batchSize))
            {
                var batchAffected = 0;
                var batch = await set
                    .SelectAwait(command=>
                    {
                        var dbCommand = conn.CreateCommand();
                        command.MapTo(dbCommand);
                        return new ValueTask<DbCommand>(dbCommand);
                    })
                    .ToArrayAsync();

                await this.dbCommandHandler.TransactionAsync(batch, (i, x) =>
                {
                    batchAffected += x;
                    return Task.CompletedTask;
                });
                await (batchIndexAndAffectedCallback?.Invoke(batchIndex++, batchAffected) ?? Task.CompletedTask);
                await Task.WhenAll(batch.Select(cmd => cmd.DisposeAsync().AsTask()));
            }

            await conn.CloseAsync();
        }
    }

    public static class AsyncEnumerableExtensions
    {
        private class Indexed<T>
        {
            public Indexed(T item, int index)
            {
                this.Index = index;
                this.Item = item;
            }

            public T Item { get; }
            public int Index { get; }

            public static ValueTask<Indexed<T>> AsValueTask(T item, int index) =>
                new ValueTask<Indexed<T>>(new Indexed<T>(item, index));
        }

        public static async IAsyncEnumerable<IAsyncEnumerable<T>> InSetsOf<T>(this IAsyncEnumerable<T> source,
            int setSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var sets = source.SelectAwait((x, i) => Indexed<T>.AsValueTask(x, i))
                .GroupByAwaitWithCancellation((x, t) => new ValueTask<int>(x.Index / setSize))
                .SelectAwait(g => new ValueTask<IAsyncEnumerable<T>>(g.SelectAwait(x => new ValueTask<T>(x.Item))));

            await foreach (var set in sets)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                yield return set;
            }
        }
    }
}
