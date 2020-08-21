 namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    public class DbCommandHandlerDecorator : IDbCommandHandler
    {
        public DbCommandHandlerDecorator(IDbCommandHandler inner)
        {

        }

        public int Execute(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteAsync(DbCommand command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Read(DbCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            throw new NotImplementedException();
        }

        public Task ReadAsync(DbCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public T Scalar<T>(DbCommand command) where T : struct
        {
            throw new NotImplementedException();
        }

        public Task<T> ScalarAsync<T>(DbCommand command, CancellationToken ct = default) where T : struct
        {
            throw new NotImplementedException();
        }

        public string String(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public Task<string> StringAsync(DbCommand command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Transaction(DbCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            throw new NotImplementedException();
        }

        public Task TransactionAsync(DbCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            throw new NotImplementedException();
        }
    }
}
