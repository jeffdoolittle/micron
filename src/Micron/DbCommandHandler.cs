namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DbCommandHandler : IDbCommandHandler
    {
        public void Read(DbCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default)
        {
            using var reader = command.ExecuteReader(behavior);

            while (reader.Read())
            {
                callback(reader);
            }

            reader.Close();
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            var value = command.ExecuteScalar();

            return !DBNull.Value.Equals(value) ? (T)value : default;
        }

        public string String(DbCommand command)
        {
            var value = command.ExecuteScalar();

            return !DBNull.Value.Equals(value) ? (string)value : "";
        }

        public int Execute(DbCommand command) =>
            command.ExecuteNonQuery();

        public async Task ReadAsync(DbCommand command,
            Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default)
        {
            await using var reader = await command.ExecuteReaderAsync(behavior, ct)
                       .ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                await callback(reader).ConfigureAwait(false);
            }

            await reader.CloseAsync().ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(DbCommand command, CancellationToken ct = default)
            where T : struct
        {
            var value = await command.ExecuteScalarAsync(ct)
                .ConfigureAwait(false);

            return !DBNull.Value.Equals(value) ? (T)value : default;
        }

        public async Task<string> StringAsync(DbCommand command, CancellationToken ct = default)
        {
            var value = await command.ExecuteScalarAsync(ct)
                .ConfigureAwait(false);

            return !DBNull.Value.Equals(value) ? (string)value : "";
        }

        public async Task<int> ExecuteAsync(DbCommand command, CancellationToken ct = default) =>
            await command.ExecuteNonQueryAsync(ct);

        public void Batch(DbCommand[] commands, bool useTransaction,
            Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            if (commands.Length == 0)
            {
                return;
            }

            if (commands.Select(c => c.Connection.ConnectionString).Distinct().Count() != 1)
            {
                throw new RootCauseException("All commands must share the same connection.");
            }

            var conn = commands[0].Connection;

            var results = new Dictionary<int, int>();

            DbTransaction? transaction = null;

            try
            {
                if (useTransaction)
                {
                    transaction = conn.BeginTransaction();
                }

                for (var i = 0; i < commands.Length; i++)
                {
                    var cmd = commands[i];
                    cmd.Transaction = transaction;
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

                if (useTransaction && transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch(Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }

        public async Task BatchAsync(DbCommand[] commands, bool useTransaction,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (commands.Length == 0)
            {
                return;
            }

            if (commands.Select(c => c.Connection.ConnectionString).Distinct().Count() != 1)
            {
                throw new RootCauseException("All commands must share the same connection.");
            }

            var conn = commands[0].Connection;

            var results = new Dictionary<int, int>();

            DbTransaction? transaction = null;

            try
            {
                if (useTransaction)
                {
                    transaction = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
                }

                for (var i = 0; i < commands.Length; i++)
                {
                    var cmd = commands[i];
                    cmd.Transaction = transaction;
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

                if (useTransaction && transaction != null)
                {
                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                }
            }
            catch(Exception)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(ct).ConfigureAwait(false);
                }
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}

