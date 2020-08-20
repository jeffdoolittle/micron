namespace Micron.SqlClient
{
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    internal class SessionRetryDecorator : ISession
    {
        private readonly IRetryHandler retryHandler;
        private readonly ISession inner;

        public SessionRetryDecorator(IRetryHandler retryHandler, ISession inner)
        {
            this.retryHandler = retryHandler;
            this.inner = inner;
        }
        public void Commit() => this.inner.Commit();

        public ReadRequest Read(ReadRequest request) =>
            this.retryHandler.Execute(() => this.inner.Read(request));

        public IReadMultipleResult ReadMultiple(ReadRequest request) =>
            this.retryHandler.Execute(() => this.inner.ReadMultiple(request));

        public TValue Scalar<TValue>(ScalarRequest<TValue> request) =>
            this.retryHandler.Execute(() => this.inner.Scalar(request));

        public void Execute(CommandRequest command) =>
            this.retryHandler.Execute(() => this.inner.Execute(command));

        public void Dispose() => this.inner.Dispose();

        public async Task CommitAsync(CancellationToken ct = default) => 
            await this.inner.CommitAsync(ct).ConfigureAwait(false);

        public async Task<IReadResult> ReadAsync(ReadRequest request,
            CancellationToken ct = default) => 
                await this.retryHandler.ExecuteAsync(async () => 
                    await this.ReadAsync(request, ct).ConfigureAwait(false))
                        .ConfigureAwait(false);

        public async Task<IReadMultipleResult> ReadMultipleAsync(ReadRequest request,
            CancellationToken ct = default) => 
                await this.retryHandler.ExecuteAsync(async () => 
                    await this.inner.ReadMultipleAsync(request, ct)
                        .ConfigureAwait(false))
                            .ConfigureAwait(false);

        public async Task<TValue> ScalarAsync<TValue>(ScalarRequest<TValue> request,
            CancellationToken ct = default) => 
                await this.retryHandler.ExecuteAsync(async () => 
                    await this.inner.ScalarAsync(request, ct)
                        .ConfigureAwait(false))
                            .ConfigureAwait(false);

        public async Task ExecuteAsync(CommandRequest command,
            CancellationToken ct = default) => 
                await this.retryHandler.ExecuteAsync(async () => 
                    await this.inner.ExecuteAsync(command, ct)
                        .ConfigureAwait(false))
                            .ConfigureAwait(false);

        public ValueTask DisposeAsync() => this.inner.DisposeAsync();
    }
}
