namespace Micron.SqlClient
{
    using System.Data;
    using System.Data.Common;
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
            var session = new Session(this.connectionFactory);
            ((ISessionLifecycle)session).Open();
            return session;
        }

        public async Task<IReadOnlySession> OpenReadOnlyAsync(CancellationToken ct = default)
        {
            var session = this.CreateSession();
            await ((ISessionLifecycle)session).OpenAsync(ct);
            return session;
        }

        public ISession OpenSession(IsolationLevel? isolationLevel = null)
        {
            var session = this.CreateSession(isolationLevel);
            ((ISessionLifecycle)session).Open();
            return session;
        }

        public async Task<ISession> OpenSessionAsync(CancellationToken ct = default,
            IsolationLevel? isolationLevel = null)
        {
            var session = this.CreateSession(isolationLevel);
            await ((ISessionLifecycle)session).OpenAsync(ct);
            return session;
        }

        private Session CreateSession(IsolationLevel? isolationLevel = null)
        {
            isolationLevel ??= this.defaultIsolationLevel;
            var session = new Session(this.connectionFactory, isolationLevel);
            var retryDecorator = new SessionRetryDecorator(null, session);
            var exceptionDecorator = new SessionExceptionDecorator(retryDecorator);
            return exceptionDecorator;
        }
    }

    public interface ISessionFactoryConfigurationInitExpression
    {

    }
}
