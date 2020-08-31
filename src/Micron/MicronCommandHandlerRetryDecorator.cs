namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.Retry;
    using Microsoft.Extensions.Logging;

    public class MicronCommandHandlerRetryDecorator : IMicronCommandHandler
    {
        private readonly IMicronCommandHandler inner;
        private readonly IRetryHandler retryHandler;
        private readonly ILogger logger;

        public MicronCommandHandlerRetryDecorator(IMicronCommandHandler inner,
            IRetryHandler retryHandler,
            ILogger logger)
        {
            this.inner = inner;
            this.retryHandler = retryHandler;
            this.logger = logger;
        }

        public int Batch(IEnumerable<MicronCommand> commands, int batchSize) =>
            this.retryHandler.Execute(attempts => {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var affected = this.inner.Batch(commands, batchSize);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return affected;
            });

        public async Task<int> BatchAsync(IAsyncEnumerable<MicronCommand> commands, int batchSize,
            CancellationToken ct = default) =>
            await this.retryHandler.ExecuteAsync(async attempts => {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var affected = await this.inner.BatchAsync(commands, batchSize)
                    .ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return affected;
            }).ConfigureAwait(false);

        public int Execute(MicronCommand command) =>
            this.retryHandler.Execute(attempts =>
            {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var affected = this.inner.Execute(command);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return affected;
            });

        public async Task<int> ExecuteAsync(MicronCommand command,
            CancellationToken ct = default) =>
            await this.retryHandler.ExecuteAsync(async attempts =>
            {
                ct.ThrowIfCancellationRequested();

                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var affected = await this.inner.ExecuteAsync(command, ct)
                    .ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return affected;
            }).ConfigureAwait(false);

        public void Read(MicronCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default) =>
            this.retryHandler.Execute(attempts =>
            {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                this.inner.Read(command, callback, behavior);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);
            });

        public async Task ReadAsync(MicronCommand command,
            Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default) =>
            await this.retryHandler.ExecuteAsync(async attempts =>
            {
                ct.ThrowIfCancellationRequested();

                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                await this.inner.ReadAsync(command, callback, behavior, ct)
                    .ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);
            }).ConfigureAwait(false);

        public T Scalar<T>(MicronCommand command) where T : struct =>
            this.retryHandler.Execute(attempts =>
            {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var value = this.inner.Scalar<T>(command);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return value;
            });

        public async Task<T> ScalarAsync<T>(MicronCommand command,
            CancellationToken ct = default) where T : struct =>
            await this.retryHandler.ExecuteAsync(async attempts =>
            {
                ct.ThrowIfCancellationRequested();

                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var value = await this.inner.ScalarAsync<T>(command)
                    .ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return value;
            }).ConfigureAwait(false);

        public string String(MicronCommand command) =>
            this.retryHandler.Execute(attempts =>
            {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var value = this.inner.String(command);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return value;
            });

        public async Task<string> StringAsync(MicronCommand command,
            CancellationToken ct = default) =>
            await this.retryHandler.ExecuteAsync(async attempts =>
            {
                ct.ThrowIfCancellationRequested();

                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                var value = await this.inner.StringAsync(command).ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);

                return value;
            }).ConfigureAwait(false);

        public void Transaction(MicronCommand[] commands,
            Action<int, int>? resultIndexAndAffectedCallback = null) =>
            this.retryHandler.Execute(attempts =>
            {
                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                this.inner.Transaction(commands, resultIndexAndAffectedCallback);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);
            });

        public async Task TransactionAsync(MicronCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null) =>
            await this.retryHandler.ExecuteAsync(async attempts =>
            {
                ct.ThrowIfCancellationRequested();

                this.logger.LogDebug("Attempt {AttemptNumber}...", attempts + 1);

                await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                    .ConfigureAwait(false);

                this.logger.LogDebug("Attempt {AttemptNumber} successful!", attempts + 1);
            }).ConfigureAwait(false);
    }
}
