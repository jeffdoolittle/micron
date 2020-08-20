namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Connect;

    internal partial class Session : ISession, ISessionLifecycle
    {
        private bool disposed;
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IsolationLevel? isolationLevel;
        private int retries;
        private DbConnection connection;
        private ITransaction transaction;

        internal Session(IDbConnectionFactory connectionFactory, IsolationLevel? isolationLevel = null)
        {
            this.connectionFactory = connectionFactory;
            this.isolationLevel = isolationLevel;
            this.connection = new NulloDbConnection();
            this.transaction = new NulloTransaction();
        }
        void ISessionLifecycle.Open()
        {
            this.Cleanup();

            this.connection = this.connectionFactory.CreateConnection();

            if (this.isolationLevel != null)
            {
                var dbTransaction = this.connection
                    .BeginTransaction(this.isolationLevel.Value);

                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        async Task ISessionLifecycle.OpenAsync(CancellationToken ct = default)
        {
            await this.CleanupAsync();

            this.connection = this.connectionFactory.CreateConnection();

            await this.connection.OpenAsync(ct).ConfigureAwait(false);

            if (this.isolationLevel != null)
            {
                var dbTransaction = await this.connection
                    .BeginTransactionAsync(this.isolationLevel.Value)
                        .ConfigureAwait(false);

                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        private void Cleanup()
        {
            if (this.retries == 0)
            {
                return;
            }

            this.connection.Close();
            this.connection.Dispose();
            this.retries++;
        }

        private async Task CleanupAsync()
        {
            if (this.retries == 0)
            {
                return;
            }

            await this.connection.CloseAsync().ConfigureAwait(false);
            await this.connection.DisposeAsync().ConfigureAwait(false);
            this.retries++;
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

    internal class NulloDbConnection : DbConnection
    {
        private readonly NulloDbTransaction transaction;

        public NulloDbConnection() =>
            this.transaction = new NulloDbTransaction(this);

        public override string ConnectionString { get; set; } = "";

        public override string Database => "";

        public override string DataSource => "";

        public override string ServerVersion => "";

        public override ConnectionState State => ConnectionState.Closed;

        public override void ChangeDatabase(string databaseName)
        {
            // no op
        }

        public override void Close()
        {
            // no op
        }

        public override void Open()
        {
            // no op
        }

        internal DbTransaction CurrentTransaction => this.transaction;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) 
            => this.transaction;

        protected override DbCommand CreateDbCommand()
            => throw new InvalidOperationException("Non-operational for a Nullo DbConnection.");
    }

    internal class NulloDbTransaction : DbTransaction
    {
        private readonly NulloDbConnection connection;

        public NulloDbTransaction(NulloDbConnection connection) =>
            this.connection = connection;

        public override IsolationLevel IsolationLevel =>
            IsolationLevel.Chaos;

        protected override DbConnection DbConnection => this.connection;

        public override void Commit()
        {
            // no op
        }

        public override void Rollback()
        {

            // no op
        }
    }
}