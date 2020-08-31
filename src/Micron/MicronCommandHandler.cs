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
            return this.dbCommandHandler.Execute(dbCommand);
        }

        public async Task<int> ExecuteAsync(MicronCommand command, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            return await this.dbCommandHandler.ExecuteAsync(dbCommand, ct);
        }

        public void Read(MicronCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            this.dbCommandHandler.Read(dbCommand, callback, behavior);
        }

        public async Task ReadAsync(MicronCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            await this.dbCommandHandler.ReadAsync(dbCommand, callback, behavior, ct);
        }

        public T Scalar<T>(MicronCommand command) where T : struct
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.Scalar<T>(dbCommand);
        }

        public async Task<T> ScalarAsync<T>(MicronCommand command, CancellationToken ct = default)
            where T : struct
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            return await this.dbCommandHandler.ScalarAsync<T>(dbCommand, ct);
        }

        public string String(MicronCommand command)
        {
            using var conn = this.dbConnectionFactory.CreateConnection();
            conn.Open();
            using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.String(dbCommand);
        }

        public async Task<string> StringAsync(MicronCommand command, CancellationToken ct = default)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var dbCommand = conn.CreateCommand();
            command.MapTo(dbCommand);
            return await this.dbCommandHandler.StringAsync(dbCommand, ct);
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
        }

        public async Task TransactionAsync(MicronCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            await using var conn = this.dbConnectionFactory.CreateConnection();
            await conn.OpenAsync(ct);

            var dbCommands = commands.Select(x =>
            {
                var dbCommand = conn.CreateCommand();
                x.MapTo(dbCommand);
                return dbCommand;
            });

            await this.dbCommandHandler.TransactionAsync(dbCommands.ToArray(), ct, resultIndexAndAffectedCallback);

            foreach (var cmd in dbCommands)
            {
                await cmd.DisposeAsync();
            }
        }
        public int Batch(IEnumerable<MicronCommand> commands, int batchSize)
        {
            var processedCount = 0;
            var affected = 0;

            do
            {
                var batchAffected = 0;

                var batch = commands.Skip(processedCount).Take(batchSize).ToArray();

                this.Transaction(batch, (i, x) => batchAffected += x);

                if (batch.Length < batchSize)
                {
                    break;
                }

                processedCount += batch.Length;
                affected += batchAffected;

            } while (true);

            return affected;
        }

        public async Task<int> BatchAsync(IEnumerable<MicronCommand> commands, int batchSize,
            CancellationToken ct = default)
        {
            var processedCount = 0;
            var affected = 0;

            do
            {
                var batchAffected = 0;

                var batch = commands.Skip(processedCount).Take(batchSize).ToArray();

                await this.TransactionAsync(batch, ct, (i, x) =>
                    {
                        batchAffected += x;
                        return Task.CompletedTask;
                    });

                if (batch.Length < batchSize)
                {
                    break;
                }

                processedCount += batch.Length;
                affected += batchAffected;

            } while (true);

            return affected;
        }
    }
}
