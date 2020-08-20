namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    public class DataCommand : IDataCommand
    {
        private readonly IRetryHandler retryHandler;
        private readonly DbCommand command;

        public DataCommand(IRetryHandler retryHandler, DbCommand command)
        {
            this.retryHandler = retryHandler;
            this.command = command;
        }

        public void Read(Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default)
        {
            using var cmd = this.command;
            using var conn = cmd.Connection;

            try
            {
                this.retryHandler.Execute(() =>
                {
                    conn.Open();

                    using var reader = cmd.ExecuteReader(behavior);

                    while (reader.Read())
                    {
                        callback(reader);
                    }

                    reader.Close();
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public T Scalar<T>() where T : struct
        {
            using var cmd = this.command;
            using var conn = cmd.Connection;

            try
            {
                return this.retryHandler.Execute(() =>
                {
                    conn.Open();

                    var value = cmd.ExecuteScalar();

                    return !DBNull.Value.Equals(value) ? (T)value : default;
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public string String()
        {
            using var cmd = this.command;
            using var conn = cmd.Connection;

            try
            {
                return this.retryHandler.Execute(() =>
                {
                    conn.Open();

                    var value = cmd.ExecuteScalar();

                    return !DBNull.Value.Equals(value) ? (string)value : "";
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public int Execute()
        {
            using var cmd = this.command;
            using var conn = cmd.Connection;

            try
            {
                return this.retryHandler.Execute(() =>
                {
                    conn.Open();

                    var affected = cmd.ExecuteNonQuery();

                    return affected;
                });
            }
            finally
            {
                conn.Close();
            }
        }

        public async Task ReadAsync(Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default)
        {
            using var cmd = this.command;
            using var conn = this.command.Connection;

            try
            {
                await this.retryHandler.ExecuteAsync(async () =>
                {
                    await conn.OpenAsync(ct).ConfigureAwait(false);

                    using var reader = await cmd.ExecuteReaderAsync(behavior, ct)
                        .ConfigureAwait(false);

                    while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {
                        await callback(reader).ConfigureAwait(false);
                    }

                    await reader.CloseAsync().ConfigureAwait(false);

                }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<T> ScalarAsync<T>(CancellationToken ct = default)
            where T : struct
        {
            using var cmd = this.command;
            using var conn = this.command.Connection;

            try
            {
                return await this.retryHandler.ExecuteAsync(async () =>
                {
                    await conn.OpenAsync(ct).ConfigureAwait(false);

                    var value = await cmd.ExecuteScalarAsync(ct)
                        .ConfigureAwait(false);

                    return !DBNull.Value.Equals(value) ? (T)value : default;

                }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> StringAsync(CancellationToken ct = default)
        {
            using var cmd = this.command;
            using var conn = this.command.Connection;

            try
            {
                return await this.retryHandler.ExecuteAsync(async () =>
                {
                    await conn.OpenAsync(ct).ConfigureAwait(false);

                    var value = await cmd.ExecuteScalarAsync(ct)
                        .ConfigureAwait(false);

                    return !DBNull.Value.Equals(value) ? (string)value : "";

                }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> ExecuteAsync(CancellationToken ct = default)
        {
            using var cmd = this.command;
            using var conn = this.command.Connection;

            try
            {
                return await this.retryHandler.ExecuteAsync(async () =>
                {
                    await conn.OpenAsync(ct).ConfigureAwait(false);

                    var affected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                    return affected;

                }).ConfigureAwait(false);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
