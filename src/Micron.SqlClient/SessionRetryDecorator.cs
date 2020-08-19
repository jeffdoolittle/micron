namespace Micron.SqlClient
{
    using System;
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

        public Task CommitAsync(CancellationToken ct = default) => this.inner.CommitAsync(ct);

        public void Dispose() => this.inner.Dispose();

        public ValueTask DisposeAsync() => this.inner.DisposeAsync();

        public void Execute(ICommandRequest command) => 
            this.retryHandler.Execute(() => this.inner.Execute(command));

        public async Task ExecuteAsync(ICommandRequest command, CancellationToken ct = default) => 
            await this.retryHandler.ExecuteAsync(() => this.inner.ExecuteAsync(command, ct))
                .ConfigureAwait(false);

        public IReadRequest Read(IReadRequest request) => 
            this.retryHandler.Execute(() => this.inner.Read(request));

        public Task<IReadResult> ReadAsync(IReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IReadMultipleResult ReadMultiple(IReadRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IReadMultipleResult> ReadMultipleAsync(IReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public TValue Scalar<TValue>(IScalarRequest<TValue> request)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> ScalarAsync<TValue>(IScalarRequest<TValue> request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
