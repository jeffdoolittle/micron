namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    internal partial class Session : ISession
    {
        private bool disposed;
        private readonly DbConnection connection;
        private readonly IsolationLevel? isolationLevel;
        private ITransaction transaction;

        internal Session(DbConnection connection, IsolationLevel? isolationLevel = null)
        {
            this.connection = connection;
            this.isolationLevel = isolationLevel;
            this.transaction = new NulloTransaction();
        }

        internal void Open()
        {
            this.connection.Open();

            if (this.isolationLevel != null)
            {
                var dbTransaction = this.connection.BeginTransaction(this.isolationLevel.Value);
                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        internal async Task OpenAsync(CancellationToken ct = default)
        {
            await this.connection.OpenAsync(ct).ConfigureAwait(false);

            if (this.isolationLevel != null)
            {
                var dbTransaction = await this.connection
                    .BeginTransactionAsync(this.isolationLevel.Value).ConfigureAwait(false);
                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        public void Commit() => this.transaction.Commit();

        public async Task CommitAsync(CancellationToken ct = default) =>
            await this.transaction.CommitAsync(ct).ConfigureAwait(false);

        public async ValueTask DisposeAsync()
        {
            await this.transaction.DisposeAsync().ConfigureAwait(false);
            await this.connection.CloseAsync().ConfigureAwait(false);
            await this.connection.DisposeAsync().ConfigureAwait(false);
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.transaction.Dispose();
                this.connection.Close();
                this.connection.Dispose();
            }

            this.disposed = true;
        }
    }
}