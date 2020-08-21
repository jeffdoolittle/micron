namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    public class DbCommandHandler : IDbCommandHandler
    {
        private readonly IRetryHandler retryHandler;

        public DbCommandHandler(IRetryHandler retryHandler) =>
            this.retryHandler = retryHandler;

        public void Read(DbCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default)
        {
            Unit exec(DbCommand cmd)
            {
                using var reader = cmd.ExecuteReader(behavior);

                while (reader.Read())
                {
                    callback(reader);
                }

                reader.Close();

                return Unit.Default;
            }

            _ = this.Try(command, exec);
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            static T exec(DbCommand cmd)
            {
                var value = cmd.ExecuteScalar();

                return !DBNull.Value.Equals(value) ? (T)value : default;
            }

            return this.Try(command, exec);
        }

        public string String(DbCommand command)
        {
            static string exec(DbCommand cmd)
            {
                var value = cmd.ExecuteScalar();

                return !DBNull.Value.Equals(value) ? (string)value : "";
            }

            return this.Try(command, exec);
        }

        public int Execute(DbCommand command)
        {
            static int exec(DbCommand cmd) => cmd.ExecuteNonQuery();
            return this.Try(command, exec);
        }

        public void Transaction(DbCommand[] commands,
            Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            if (commands.Length == 0)
            {
                return;
            }

            if (commands.Select(c => c.Connection).Distinct().Count() != 1)
            {
                throw new RootCauseException("All commands must share the same connection.");
            }

            using var conn = commands[0].Connection;

            try
            {
                this.retryHandler.Execute(() =>
                {
                    var results = new Dictionary<int, int>();

                    conn.Open();
                    using var tran = conn.BeginTransaction();

                    for (var i = 0; i < commands.Length; i++)
                    {
                        var cmd = commands[i];
                        cmd.Transaction = tran;
                        var affected = cmd.ExecuteNonQuery();
                        results.Add(i, affected);
                    }

                    if (resultIndexAndAffectedCallback != null)
                    {
                        foreach (var result in results)
                        {
                            resultIndexAndAffectedCallback(result.Key, result.Value);
                        }
                    }

                    tran.Commit();
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public async Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default)
        {
            async Task<Unit> exec(DbCommand cmd, CancellationToken ct)
            {
                using var reader = await cmd.ExecuteReaderAsync(behavior, ct)
                    .ConfigureAwait(false);

                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    await callback(reader).ConfigureAwait(false);
                }

                await reader.CloseAsync().ConfigureAwait(false);

                return Unit.Default;
            }
            
            _ = await this.TryAsync(command, ct, exec).ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(DbCommand command, CancellationToken ct = default)
            where T : struct
        {
            static async Task<T> exec(DbCommand cmd, CancellationToken ct)
            {
                var value = await cmd.ExecuteScalarAsync(ct)
                    .ConfigureAwait(false);

                return !DBNull.Value.Equals(value) ? (T)value : default;
            }

            return await this.TryAsync(command, ct, exec).ConfigureAwait(false);
        }

        public async Task<string> StringAsync(DbCommand command, CancellationToken ct = default)
        {
            static async Task<string> exec(DbCommand cmd, CancellationToken ct)
            {
                var value = await cmd.ExecuteScalarAsync(ct)
                    .ConfigureAwait(false);

                return !DBNull.Value.Equals(value) ? (string)value : "";
            }

            return await this.TryAsync(command, ct, exec).ConfigureAwait(false);
        }

        public async Task<int> ExecuteAsync(DbCommand command, CancellationToken ct = default)
        {
            static Task<int> exec(DbCommand cmd, CancellationToken ct) =>
                cmd.ExecuteNonQueryAsync(ct);

            return await this.TryAsync(command, ct, exec).ConfigureAwait(false);
        }

        public async Task TransactionAsync(DbCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            if (commands.Length == 0)
            {
                return;
            }

            if (commands.Select(c => c.Connection).Distinct().Count() != 1)
            {
                throw new RootCauseException("All commands must share the same connection.");
            }

            using var conn = commands[0].Connection;

            try
            {
                await this.retryHandler.ExecuteAsync(async () =>
                {
                    var results = new Dictionary<int, int>();

                    conn.Open();
                    using var tran = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

                    for (var i = 0; i < commands.Length; i++)
                    {
                        var cmd = commands[i];
                        cmd.Transaction = tran;
                        var affected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        results.Add(i, affected);
                    }

                    if (resultIndexAndAffectedCallback != null)
                    {
                        foreach (var result in results)
                        {
                            await resultIndexAndAffectedCallback(result.Key, 
                                result.Value).ConfigureAwait(false);
                        }
                    }

                    await tran.CommitAsync(ct).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        private T Try<T>(DbCommand command, Func<DbCommand, T> exec)
        {
            using var cmd = command;
            using var conn = command.Connection;

            try
            {
                return this.retryHandler.Execute(() =>
               {
                   conn.Open();

                   return exec(cmd);
               });
            }
            finally
            {
                conn.Close();
            }
        }

        private async Task<T> TryAsync<T>(DbCommand command,
            CancellationToken ct,
            Func<DbCommand, CancellationToken, Task<T>> exec)
        {
            ct.ThrowIfCancellationRequested();

            using var cmd = command;
            using var conn = command.Connection;

            try
            {
                return await this.retryHandler.ExecuteAsync(async () =>
               {
                   ct.ThrowIfCancellationRequested();

                   await conn.OpenAsync(ct).ConfigureAwait(false);

                   return await exec(cmd, ct).ConfigureAwait(false);

               }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
