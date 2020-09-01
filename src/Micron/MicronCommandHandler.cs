namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
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
            using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);

            var batchIndex = 0;

            var dbCommands = Enumerable.Range(0, batchSize)
                .Select(_ => conn.CreateCommand());

            var dbCommand = conn.CreateCommand();

            var batch = new List<DbCommand>();
            var batchAffected = 0;
            await foreach (var cmd in commands)
            {
                cmd.MapTo(dbCommand);
                batch.Add(dbCommand);

                if (batch.Count == batchSize)
                {
                    await this.dbCommandHandler.TransactionAsync(batch.ToArray(), (i, x) => { batchAffected += x; return Task.CompletedTask; }, ct);
                    batchAffected = batch.Count;
                    await (batchIndexAndAffectedCallback?.Invoke(batchIndex, batchAffected) ?? Task.CompletedTask);
                    batch.Clear();
                    batchAffected = 0;
                    batchIndex++;
                }
            }

            if (batch.Count > 0)
            {
                await this.dbCommandHandler.TransactionAsync(batch.ToArray(), (i, x) => { batchAffected += x; return Task.CompletedTask; }, ct);
                await (batchIndexAndAffectedCallback?.Invoke(batchIndex, batchAffected) ?? Task.CompletedTask);
                batch.Clear();
            }

            conn.Close();
        }
    }
}
