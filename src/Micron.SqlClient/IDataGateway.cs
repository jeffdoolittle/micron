namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDataGateway
    {
        IReadResult Read(IReadRequest request);

        IReadMultipleResult ReadMultiple(IReadRequest request);

        TValue Scalar<TValue>(IScalarRequest<TValue> request);

        void Execute(ICommandRequest command);

        void Commit();

        Task<IReadResult> ReadAsync(IReadRequest request,
            CancellationToken ct = default);

        Task<IReadMultipleResult> ReadMultipleAsync(IReadRequest request,
            CancellationToken ct = default);

        Task<TValue> ScalarAsync<TValue>(IScalarRequest<TValue> request,
            CancellationToken ct = default);

        Task ExecuteAsync(ICommandRequest command,
            CancellationToken ct = default);

        Task CommitAsync(CancellationToken ct = default);
    }

    public interface IDataStatement
    {
        string CommandText { get; }
        object[] Parameters { get; }
        int TimeoutSeconds { get; }
    }

    public interface ICommand : IDataStatement
    {
        int ExpectedAffectedRows { get; }
    }

    public interface ICommandRequest
    {
        IEnumerable<ICommand> Commands { get; }
    }

    public interface IScalarRequest<T> : IDataStatement
    {
        Func<object, T> Converter { get; }
    }

    public interface IReadRequest : IDataStatement { }

    public interface IReadResult
    {
        IAsyncEnumerable<IDataResult> Results { get; }
    }

    public interface IReadMultipleResult
    {
        bool NextResult();
        IAsyncEnumerable<IDataResult> Results { get; }
    }

    public interface IDataResult : IReadOnlyDictionary<string, object>
    {
    }

    internal class ReadResult : IReadResult
    {
        public IAsyncEnumerable<IDataResult> Results { get; set; }
            = (new IDataResult[0]).AsAsyncEnumerable();
    }
}
