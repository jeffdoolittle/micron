namespace Micron.SqlClient
{
    using System.Threading;
    using System.Threading.Tasks;

    internal class SessionExceptionDecorator : ISession
    {
        private readonly ISession inner;

        public SessionExceptionDecorator(ISession inner) => this.inner = inner;

        public void Commit() => 
            Try.To(() => this.inner.Commit());

        public void Dispose() => 
            Try.To(() => this.inner.Dispose());

        public void Execute(ICommandRequest request) =>
            Try.To(() => this.inner.Execute(request));

        public IReadRequest Read(IReadRequest request) =>
            Try.To(() => this.inner.Read(request));

        public IReadMultipleResult ReadMultiple(IReadRequest request) => 
            Try.To(() => this.inner.ReadMultiple(request));

        public TValue Scalar<TValue>(IScalarRequest<TValue> request) => 
            Try.To(() => this.inner.Scalar(request));

        public async Task CommitAsync(CancellationToken ct = default) =>
            await Try.ToAsync(async () => await this.inner.CommitAsync(ct).ConfigureAwait(false))
                .ConfigureAwait(false);

        public async ValueTask DisposeAsync() =>
            await Try.ToAsync(async () => await this.inner.DisposeAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

        public async Task ExecuteAsync(ICommandRequest request, 
            CancellationToken ct = default) => 
                await Try.ToAsync(async () => await this.inner.ExecuteAsync(request, ct).ConfigureAwait(false))
                    .ConfigureAwait(false);

        public async Task<IReadResult> ReadAsync(IReadRequest request, 
            CancellationToken ct = default) => 
                await Try.ToAsync(async () => await this.inner.ReadAsync(request, ct).ConfigureAwait(false))
                    .ConfigureAwait(false);

        public async Task<IReadMultipleResult> ReadMultipleAsync(IReadRequest request, 
            CancellationToken ct = default) => 
                await Try.ToAsync(async () => await this.inner.ReadMultipleAsync(request, ct).ConfigureAwait(false))
                    .ConfigureAwait(false);

        public async Task<TValue> ScalarAsync<TValue>(IScalarRequest<TValue> request, 
            CancellationToken ct = default) => 
                await Try.ToAsync(async () => await this.inner.ScalarAsync(request, ct).ConfigureAwait(false))
                    .ConfigureAwait(false);

    }
}
