namespace Micron.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITransaction : IAsyncDisposable, IDisposable
    {
        void Commit();

        Task CommitAsync(CancellationToken ct = default);
    }

    internal class DbTransactionAdapter : ITransaction
    {
        private readonly DbTransaction transaction;

        public DbTransactionAdapter(DbTransaction transaction) =>
            this.transaction = transaction;

        public void Commit() => this.transaction.Commit();

        public async Task CommitAsync(CancellationToken ct = default) =>
            await this.transaction.CommitAsync(ct).ConfigureAwait(false);

        public void Dispose() => this.transaction.Dispose();

        public async ValueTask DisposeAsync() =>
            await this.transaction.DisposeAsync().ConfigureAwait(false);
    }

    internal class NulloTransaction : ITransaction
    {
        public void Commit() { return; }

        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;

        public void Dispose() { return; }

        public ValueTask DisposeAsync() => default;
    }
}
