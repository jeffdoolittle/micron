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

            _ = this.Try(exec);
        }

        public T Scalar<T>() where T : struct
        {
            static T exec(DbCommand cmd)
            {
                var value = cmd.ExecuteScalar();

                return !DBNull.Value.Equals(value) ? (T)value : default;
            }

            return this.Try(exec);
        }

        public string String()
        {
            static string exec(DbCommand cmd)
            {
                var value = cmd.ExecuteScalar();

                return !DBNull.Value.Equals(value) ? (string)value : "";
            }

            return this.Try(exec);
        }

        public int Execute()
        {
            static int exec(DbCommand cmd) => cmd.ExecuteNonQuery();
            return this.Try(exec);
        }

        public async Task ReadAsync(Func<IDataRecord, Task> callback,
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

            _ = await this.Try(ct, exec).ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(CancellationToken ct = default)
            where T : struct
        {
            static async Task<T> exec(DbCommand cmd, CancellationToken ct)
            {
                var value = await cmd.ExecuteScalarAsync(ct)
                    .ConfigureAwait(false);

                return !DBNull.Value.Equals(value) ? (T)value : default;
            }

            return await this.Try(ct, exec).ConfigureAwait(false);
        }

        public async Task<string> StringAsync(CancellationToken ct = default)
        {
            static async Task<string> exec(DbCommand cmd, CancellationToken ct)
            {
                var value = await cmd.ExecuteScalarAsync(ct)
                    .ConfigureAwait(false);

                return !DBNull.Value.Equals(value) ? (string)value : "";
            }

            return await this.Try(ct, exec).ConfigureAwait(false);
        }

        public async Task<int> ExecuteAsync(CancellationToken ct = default)
        {
            static Task<int> exec(DbCommand cmd, CancellationToken ct) =>
                cmd.ExecuteNonQueryAsync(ct);

            return await this.Try(ct, exec).ConfigureAwait(false);
        }

        private T Try<T>(Func<DbCommand, T> exec)
        {
            using var cmd = this.command;
            using var conn = this.command.Connection;

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

        private async Task<T> Try<T>(CancellationToken ct,
            Func<DbCommand, CancellationToken, Task<T>> exec)
        {
            ct.ThrowIfCancellationRequested();

            using var cmd = this.command;
            using var conn = this.command.Connection;

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

// todo: handle multiple commands in a transaction
