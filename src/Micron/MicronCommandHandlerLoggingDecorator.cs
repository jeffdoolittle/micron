namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class MicronCommandHandlerLoggingDecorator : IMicronCommandHandler
    {
        private readonly IMicronCommandHandler inner;
        private readonly ILogger<IMicronCommandHandler> logger;

        public MicronCommandHandlerLoggingDecorator(IMicronCommandHandler inner,
            ILogger<IMicronCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public void Batch(IEnumerable<MicronCommand> commands, int batchSize,
            Action<int, int>? batchIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Executing batch.");


            void callback(int batchIndex, int affectedCount)
            {
                this.logger.LogInformation("Executed batch {BatchIndex} affecting {Affected} rows.",
                    batchIndex, affectedCount);

                batchIndexAndAffectedCallback?.Invoke(batchIndex, affectedCount);
            }

            this.inner.Batch(commands, batchSize, callback);
        }

        public async Task BatchAsync(IAsyncEnumerable<MicronCommand> commands, int batchSize,
            CancellationToken ct = default,
            Func<int, int, Task>? batchIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Executing batch.");

            Task callback(int batchIndex, int affectedCount)
            {
                this.logger.LogInformation("Executed batch {BatchIndex} affecting {Affected} rows.",
                    batchIndex, affectedCount);

                return batchIndexAndAffectedCallback?.Invoke(batchIndex, affectedCount) ?? Task.CompletedTask;
            }

            await this.inner.BatchAsync(commands, batchSize, ct, callback)
                .ConfigureAwait(false);
        }

        public int Execute(MicronCommand command)
        {
            this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

            var affected = this.inner.Execute(command);

            this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                command.CommandText, affected);

            return affected;
        }

        public async Task<int> ExecuteAsync(MicronCommand command, CancellationToken ct = default)
        {
            this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

            var affected = await this.inner.ExecuteAsync(command, ct)
                .ConfigureAwait(false);

            this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                command.CommandText, affected);

            return affected;
        }

        public void Read(MicronCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);

            this.inner.Read(command, callback, behavior);

            this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);
        }

        public async Task ReadAsync(MicronCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {

            this.logger.LogDebug("Reading from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);

            await this.inner.ReadAsync(command, callback, behavior, ct)
                .ConfigureAwait(false);

            this.logger.LogInformation("Read from command {CommandText} with {CommandBehavior}.",
                command.CommandText, behavior);
        }

        public T Scalar<T>(MicronCommand command) where T : struct
        {
            this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                                command.CommandText);

            var scalar = this.inner.Scalar<T>(command);

            this.logger.LogInformation("Retrieved scalar from command {CommandText}.",
                command.CommandText);

            return scalar;
        }

        public async Task<T> ScalarAsync<T>(MicronCommand command, CancellationToken ct = default) where T : struct
        {
            this.logger.LogDebug("Retrieving scalar from command {CommandText}.",
                               command.CommandText);

            var scalar = await this.inner.ScalarAsync<T>(command, ct)
                .ConfigureAwait(false);

            this.logger.LogInformation("Retrieved scalar from command {CommandText}.",
                command.CommandText);

            return scalar;
        }

        public string String(MicronCommand command)
        {
            this.logger.LogDebug("Retrieving string from command {CommandText}.",
                               command.CommandText);

            var scalar = this.inner.String(command);

            this.logger.LogInformation("Retrieved string from command {CommandText}.",
                command.CommandText);

            return scalar;
        }

        public async Task<string> StringAsync(MicronCommand command, CancellationToken ct = default)
        {
            this.logger.LogDebug("Retrieving string from command {CommandText}.",
                               command.CommandText);

            var scalar = await this.inner.StringAsync(command, ct)
                .ConfigureAwait(false);

            this.logger.LogInformation("Retrieved string from command {CommandText}.",
                command.CommandText);

            return scalar;
        }

        public void Transaction(MicronCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

            this.inner.Transaction(commands, resultIndexAndAffectedCallback);

            this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                commands.Length);
        }

        public async Task TransactionAsync(MicronCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            this.logger.LogDebug("Performing transaction for {CommandCount} commands.", commands.Length);

            await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                .ConfigureAwait(false);

            this.logger.LogInformation("Performed transaction for {CommandCount} commands.",
                commands.Length);
        }
    }
}
