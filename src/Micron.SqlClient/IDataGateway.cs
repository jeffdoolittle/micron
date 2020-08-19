namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDataGateway
    {
        Task Execute(ICommand command, CancellationToken ct = default);

        Task Execute(ICommand[] commands, CancellationToken ct = default);

        Task<TValue> Scalar<TValue>(IScalarRequest<TValue> request, CancellationToken ct = default);

        IReadResult Read(IReadRequest request, CancellationToken ct = default);

        IReadMultipleResult ReadMultiple(IReadRequest request, CancellationToken ct = default);
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
}
