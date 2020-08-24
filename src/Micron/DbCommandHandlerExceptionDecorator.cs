namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    internal class DbCommandHandlerExceptionDecorator : IDbCommandHandler
    {
        private readonly IDbCommandHandler inner;
        private readonly ILogger<IDbCommandHandler> logger;

        public DbCommandHandlerExceptionDecorator(IDbCommandHandler inner,
            ILogger<IDbCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public void Read(DbCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default)
        {
            void exec() => this.inner.Read(command, callback, behavior);

            Try.To(exec, this.logger);
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            T exec()
            {
                var scalar = this.inner.Scalar<T>(command);

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public string String(DbCommand command)
        {
            string exec()
            {

                var scalar = this.inner.String(command);

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public int Execute(DbCommand command)
        {
            int exec()
            {
                var affected = this.inner.Execute(command);

                return affected;
            }

            return Try.To(exec, this.logger);
        }

        public void Transaction(DbCommand[] commands,
            Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            void exec() => this.inner.Transaction(commands, resultIndexAndAffectedCallback);

            Try.To(exec, this.logger);
        }

        public async Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default)
        {
            async Task exec() =>
                await this.inner.ReadAsync(command, callback, behavior, ct)
                    .ConfigureAwait(false);

            await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(DbCommand command,
            CancellationToken ct = default) where T : struct
        {
            async Task<T> exec()
            {
                var scalar = await this.inner.ScalarAsync<T>(command, ct);

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<string> StringAsync(DbCommand command,
            CancellationToken ct = default)
        {
            async Task<string> exec()
            {
                var scalar = await this.inner.StringAsync(command, ct);

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<int> ExecuteAsync(DbCommand command,
            CancellationToken ct = default)
        {
            async Task<int> exec()
            {
                var affected = await this.inner.ExecuteAsync(command, ct)
                    .ConfigureAwait(false);

                return affected;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task TransactionAsync(DbCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            async Task exec() =>
                await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                    .ConfigureAwait(false);

            await Try.To(exec, this.logger).ConfigureAwait(false);
        }
    }
}
