namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Connect;

    public interface IReadOnlySession : IDisposable, IAsyncDisposable
    {
        void Open();


        Task OpenAsync(CancellationToken ct = default);
    }

    public interface ISession : IReadOnlySession
    {
        void Commit();

        Task Commit(CancellationToken ct = default);
    }
    
    public interface ISessionFactory
    {
        IReadOnlySession OpenReadOnly();

        ISession OpenSession(IsolationLevel? isolationLevel = null);

        Task<IReadOnlySession> OpenReadOnlyAsync(CancellationToken ct = default);

        Task<ISession> OpenSessionAsync(CancellationToken ct = default,
            IsolationLevel? isolationLevel = null);
    }

    public class SessionFactory : ISessionFactory
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IsolationLevel defaultIsolationLevel;

        public SessionFactory(IDbConnectionFactory connectionFactory, 
            IsolationLevel defaultIsolationLevel = IsolationLevel.ReadCommitted)
        {
            this.connectionFactory = connectionFactory;
            this.defaultIsolationLevel = defaultIsolationLevel;
        }

        public IReadOnlySession OpenReadOnly()
        {
            var connection = this.connectionFactory.CreateConnection();
            var session = new Session(connection);
            session.Open();
            return session;
        }

        public async Task<IReadOnlySession> OpenReadOnlyAsync(CancellationToken ct = default)
        {
            var connection = this.connectionFactory.CreateConnection();
            var session = new Session(connection);
            await session.OpenAsync(ct);
            return session;
        }

        public ISession OpenSession(IsolationLevel? isolationLevel = null)
        {
            isolationLevel ??= this.defaultIsolationLevel;
            var connection = this.connectionFactory.CreateConnection();
            var session = new Session(connection, isolationLevel);
            session.Open();
            return session;
        }

        public async Task<ISession> OpenSessionAsync(CancellationToken ct = default,
            IsolationLevel? isolationLevel = null)
        {
            isolationLevel ??= this.defaultIsolationLevel;
            var connection = this.connectionFactory.CreateConnection();
            var session = new Session(connection, isolationLevel);
            await session.OpenAsync(ct);
            return session;
        }
    }

    internal class Session : ISession
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

        public void Open()
        {
            this.connection.Open();

            if (this.isolationLevel != null)
            {
                var dbTransaction = this.connection.BeginTransaction(this.isolationLevel.Value);
                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        public void Commit() => this.transaction.Commit();

        public async Task OpenAsync(CancellationToken ct = default)
        {
            await this.connection.OpenAsync(ct).ConfigureAwait(false);

            if (this.isolationLevel != null)
            {
                var dbTransaction = await this.connection
                    .BeginTransactionAsync(this.isolationLevel.Value).ConfigureAwait(false);
                this.transaction = new DbTransactionAdapter(dbTransaction);
            }
        }

        public async Task Commit(CancellationToken ct = default) =>
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