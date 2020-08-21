namespace Micron.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public class DbCommandHandlerDecorator : IDbCommandHandler
    {
        private readonly IDbCommandHandler inner;
        private readonly ILogger<IDbCommandHandler> logger;

        public DbCommandHandlerDecorator(IDbCommandHandler inner,
            ILogger<IDbCommandHandler> logger)
        {
            this.inner = inner;
            this.logger = logger ?? NullLogger<IDbCommandHandler>.Instance;
        }

        public int Execute(DbCommand command)
        {
            int exec()
            {
                this.logger.LogDebug("Executing command {CommandText}.", command.CommandText);

                var affected = this.inner.Execute(command);

                this.logger.LogInformation("Executed command {CommandText} affecting {Affected} rows.",
                    command.CommandText, affected);

                return affected;
            }

            return Try.To(exec);
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
