namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    internal class DbCommandHandlerDecorator : IDbCommandHandler
    {
        private readonly IDbCommandHandler inner;
        private readonly ILogger<IDbCommandHandler> logger;

        public DbCommandHandlerDecorator(IDbCommandHandler inner,
            ILogger<IDbCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger ?? NullLogger<IDbCommandHandler>.Instance;
        }

        public void Read(DbCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            void exec()
            {
                this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                    command.CommandText, behavior);

                this.inner.Read(command, callback, behavior);

                this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.");
            }

            Try.To(exec, this.logger);
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            T exec()
            {
                this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                    command.CommandText);

                var scalar = this.inner.Scalar<T>(command);

                this.logger.LogInformation("Retrieved scalar from command {CommandText}.");

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public string String(DbCommand command)
        {
            string exec()
            {
                this.logger.LogDebug("Retrieving string from command {CommandText}.",
                    command.CommandText);

                var scalar = this.inner.String(command);

                this.logger.LogInformation("Retrieved string from command {CommandText}.");

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public int Execute(DbCommand command)
        {
            int exec()
            {
                this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

                var affected = this.inner.Execute(command);

                this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                    command.CommandText, affected);

                return affected;
            }

            return Try.To(exec, this.logger);
        }

        public void Transaction(DbCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            void exec()
            {
                this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

                this.inner.Transaction(commands, resultIndexAndAffectedCallback);

                this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                    commands.Length);
            }

            Try.To(exec, this.logger);
        }

        public async Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            async Task exec()
            {
                this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                    command.CommandText, behavior);

                await this.inner.ReadAsync(command, callback, behavior, ct)
                    .ConfigureAwait(false);

                this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.");
            }

            await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(DbCommand command,
            CancellationToken ct = default) where T : struct
        {
            async Task<T> exec()
            {
                this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                    command.CommandText);

                var scalar = await this.inner.ScalarAsync<T>(command, ct);

                this.logger.LogInformation("Retrieved scalar from command {CommandText}.");

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<string> StringAsync(DbCommand command,
            CancellationToken ct = default)
        {
            async Task<string> exec()
            {
                this.logger.LogDebug("Retrieving string from command {CommandText}.",
                    command.CommandText);

                var scalar = await this.inner.StringAsync(command, ct);

                this.logger.LogInformation("Retrieved string from command {CommandText}.");

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task<int> ExecuteAsync(DbCommand command,
            CancellationToken ct = default)
        {
            async Task<int> exec()
            {
                this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

                var affected = await this.inner.ExecuteAsync(command, ct)
                    .ConfigureAwait(false);

                this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                    command.CommandText, affected);

                return affected;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public async Task TransactionAsync(DbCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            async Task exec()
            {
                this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

                await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                    .ConfigureAwait(false);

                this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                    commands.Length);
            }

            await Try.To(exec, this.logger).ConfigureAwait(false);
        }
    }
}
