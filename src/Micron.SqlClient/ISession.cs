namespace Micron.SqlClient
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface ISessionLifecycle
    {
        void Open();

        Task OpenAsync(CancellationToken ct = default);
    }

    public interface IReadOnlySession : IDataGateway, IDisposable, IAsyncDisposable
    {
    }
    
    public interface ISession : IReadOnlySession
    {
        void Commit();

        Task CommitAsync(CancellationToken ct = default);
    }
}
