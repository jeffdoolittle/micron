namespace Micron.SqlClient
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IReadOnlySession : IDataGateway, IDisposable, IAsyncDisposable
    {
    }
    
    public interface ISession : IReadOnlySession
    {
        void Commit();

        Task CommitAsync(CancellationToken ct = default);
    }
}
