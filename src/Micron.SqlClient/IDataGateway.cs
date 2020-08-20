namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    public interface ICommand
    {
        void Read(Action<IDataRecord> callback);

        T Scalar<T>() where T : struct;

        string String();

        int Execute();

        Task ReadAsync(Func<IDataRecord, Task> callback, CancellationToken ct = default);

        Task<T> ScalarAsync<T>() where T : struct;

        Task<string> StringAsync(CancellationToken ct = default);

        Task<int> ExecuteAsync(CancellationToken ct = default);
    }

    public class Command : ICommand
    {
        private readonly IRetryHandler retryHandler;
        private readonly DbCommand command;

        public Command(IRetryHandler retryHandler, DbCommand command)
        {
            this.retryHandler = retryHandler;
            this.command = command;
        }

        public void Read(Action<IDataRecord> callback) =>
            this.retryHandler.Execute(() =>
            {
                using var cmd = this.command;
                using var conn = cmd.Connection;
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    callback(reader);
                }
                reader.Close();
                conn.Close();
            });

        public T Scalar<T>() where T : struct =>
            this.retryHandler.Execute(() =>
            {
                using var cmd = this.command;
                using var conn = cmd.Connection;
                conn.Open();
                var value = cmd.ExecuteScalar();
                conn.Close();
                if (DBNull.Value.Equals(value))
                {
                    return default;
                }
                return (T)value;
            });

        public string String() =>
            this.retryHandler.Execute(() =>
            {
                using var cmd = this.command;
                using var conn = cmd.Connection;
                conn.Open();
                var value = cmd.ExecuteScalar();
                conn.Close();
                if (DBNull.Value.Equals(value))
                {
                    return "";
                }
                return (string)value;
            });

        public int Execute() => 
            this.retryHandler.Execute(() =>
            {
                using var cmd = this.command;
                using var conn = cmd.Connection;
                conn.Open();
                var affected = cmd.ExecuteNonQuery();
                conn.Close();
                return affected;
            });

        public Task<int> ExecuteAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task ReadAsync(Func<IDataRecord, Task> callback,
            CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> ScalarAsync<T>() where T : struct
        {
            throw new NotImplementedException();
        }

        public Task<string> StringAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }

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

    public class DataCommand : DataStatement
    {
        public int ExpectedAffectedRows { get; set; } = -1;
    }

    public class CommandRequest
    {
        public IList<DataCommand> Commands { get; } = new List<DataCommand>();
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
