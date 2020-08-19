namespace Micron.SqlClient
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Connect;

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

    public interface ISessionFactoryConfigurationInitExpression
    {
        
    }
}
