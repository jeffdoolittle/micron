namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class MicronCommandHandlerExceptionDecorator : IMicronCommandHandler
    {
        private readonly IMicronCommandHandler inner;
        private readonly ILogger<IMicronCommandHandler> logger;

        public MicronCommandHandlerExceptionDecorator(IMicronCommandHandler inner,
            ILogger<IMicronCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public int Batch(IEnumerable<MicronCommand> commands, int batchSize)
        {
            int exec()
            {
                var affected = this.inner.Batch(commands, batchSize);
                return affected;
            }

            return Try.To(exec, this.logger);
        }

        public async Task<int> BatchAsync(IAsyncEnumerable<MicronCommand> commands, int batchSize, CancellationToken ct = default)
        {
            async Task<int> exec()
            {
                var affected = await this.inner.BatchAsync(commands, batchSize).ConfigureAwait(false);
                return affected;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public int Execute(MicronCommand command)
        {
            int exec()
            {
                var affected = this.inner.Execute(command);

                return affected;
            }

            return Try.To(exec, this.logger);
        }

        public async Task<int> ExecuteAsync(MicronCommand command, CancellationToken ct = default)
        {
            async Task<int> exec()
            {
                var affected = await this.inner.ExecuteAsync(command, ct)
                    .ConfigureAwait(false);

                return affected;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public void Read(MicronCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default)
        {
            void exec() => this.inner.Read(command, callback, behavior);

            Try.To(exec, this.logger);
        }

        public async Task ReadAsync(MicronCommand command,
            Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default)
        {
            async Task exec() =>
                            await this.inner.ReadAsync(command, callback, behavior, ct)
                                .ConfigureAwait(false);

            await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public T Scalar<T>(MicronCommand command) where T : struct
        {
            T exec()
            {
                var scalar = this.inner.Scalar<T>(command);

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public async Task<T> ScalarAsync<T>(MicronCommand command, CancellationToken ct = default) where T : struct
        {
            async Task<T> exec()
            {
                var scalar = await this.inner.ScalarAsync<T>(command, ct);

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public string String(MicronCommand command)
        {
            string exec()
            {

                var scalar = this.inner.String(command);

                return scalar;
            }

            return Try.To(exec, this.logger);
        }

        public async Task<string> StringAsync(MicronCommand command, CancellationToken ct = default)
        {
            async Task<string> exec()
            {
                var scalar = await this.inner.StringAsync(command, ct);

                return scalar;
            }

            return await Try.ToAsync(exec, this.logger).ConfigureAwait(false);
        }

        public void Transaction(MicronCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            void exec() => this.inner.Transaction(commands, resultIndexAndAffectedCallback);

            Try.To(exec, this.logger);
        }

        public async Task TransactionAsync(MicronCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            async Task exec() =>
                await this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback)
                    .ConfigureAwait(false);

            await Try.To(exec, this.logger).ConfigureAwait(false);
        }
    }
}
