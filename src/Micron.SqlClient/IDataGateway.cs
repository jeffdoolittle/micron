namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDataGateway
    {
        IEnumerable<IDataRecord> Read(ReadRequest request);

        IReadMultipleResult ReadMultiple(ReadRequest request);

        TValue Scalar<TValue>(ScalarRequest<TValue> request)
            where TValue : struct;

        string Scalar(StringRequest request);

        void Execute(CommandRequest command);

        Task<IAsyncEnumerable<IDataRecord>> ReadAsync(ReadRequest request,
            CancellationToken ct = default);

        Task<IAsyncReadMultipleResult> ReadMultipleAsync(ReadRequest request,
            CancellationToken ct = default);

        Task<TValue> ScalarAsync<TValue>(ScalarRequest<TValue> request,
            CancellationToken ct = default)
                where TValue : struct;

        Task<string> ScalarAsync(StringRequest request,
            CancellationToken ct = default);

        Task ExecuteAsync(CommandRequest command,
            CancellationToken ct = default);
    }

    public class DataStatement
    {
        public string CommandText { get; set; } = "";
        public object[] Parameters { get; set; } = new object[0];
        public int TimeoutSeconds { get; set; }
    }

    public class CommandRequest
    {
        public IList<DbCommandHandler> Commands { get; } = new List<DbCommandHandler>();
    }

    public class ScalarRequest<T> : DataStatement where T : struct
    {
        public Func<object, T> Converter { get; set; } = _ => default;
    }

    public class StringRequest : DataStatement
    {
        public Func<object, string> Converter { get; set; } = _ => "";
    }

    public class ReadRequest : DataStatement { }

    public interface IReadMultipleResult : IDisposable
    {
        bool NextResult();
        IEnumerable<IDataRecord> Results { get; }
    }

    public interface IAsyncReadMultipleResult : IDisposable, IAsyncDisposable
    {
        bool NextResult();
        IAsyncEnumerable<IDataRecord> Results { get; }
    }
}
