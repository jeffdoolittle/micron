namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    internal class DbCommandHandlerLoggingDecorator : IDbCommandHandler
    {
        private readonly IDbCommandHandler inner;
        private readonly ILogger<IDbCommandHandler> logger;

        public DbCommandHandlerLoggingDecorator(IDbCommandHandler inner,
            ILogger<IDbCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public int Execute(DbCommand command)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

            var affected = this.inner.Execute(command);

            this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                commandText, affected);

            return affected;
        }

        public async Task<int> ExecuteAsync(DbCommand command, CancellationToken ct = default)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

            var affected = await this.inner.ExecuteAsync(command, ct);

            this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                commandText, affected);

            return affected;
        }

        public void Read(DbCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);

            this.inner.Read(command, callback, behavior);

            this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.",
                commandText, behavior);
        }

        public async Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);

            await this.inner.ReadAsync(command, callback, behavior, ct)
                .ConfigureAwait(false);

            this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.",
                commandText, behavior);
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            var commandText = command.CommandText;


            this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                                command.CommandText);

            var scalar = this.inner.Scalar<T>(command);

            this.logger.LogInformation("Retrieved scalar from command {CommandText}.",
                commandText);

            return scalar;
        }

        public async Task<T> ScalarAsync<T>(DbCommand command, CancellationToken ct = default) where T : struct
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                               command.CommandText);

            var scalar = await this.inner.ScalarAsync<T>(command, ct);

            this.logger.LogInformation("Retrieved scalar from command {CommandText}.",
                commandText);

            return scalar;
        }

        public string String(DbCommand command)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Retrieving string from command {CommandText}.",
                               command.CommandText);

            var scalar = this.inner.String(command);

            this.logger.LogInformation("Retrieved string from command {CommandText}.",
                commandText);

            return scalar;
        }

        public async Task<string> StringAsync(DbCommand command, CancellationToken ct = default)
        {
            var commandText = command.CommandText;

            this.logger.LogDebug("Retrieving string from command {CommandText}.",
                               command.CommandText);

            var scalar = await this.inner.StringAsync(command, ct);

            this.logger.LogInformation("Retrieved string from command {CommandText}.",
                commandText);

            return scalar;
        }

        public void Transaction(DbCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

            this.inner.Transaction(commands, resultIndexAndAffectedCallback);

            this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                commands.Length);
        }

        public async Task TransactionAsync(DbCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

            await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                .ConfigureAwait(false);

            this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                commands.Length);
        }
    }
}
