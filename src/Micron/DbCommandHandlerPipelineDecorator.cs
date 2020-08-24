namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DbCommandHandlerPipelineDecorator : IDbCommandHandler
    {
        private readonly IDbCommandHandler inner;
        private readonly IDbCommandConfigurer commandConfigurer;

        public DbCommandHandlerPipelineDecorator(IDbCommandHandler inner,
            IDbCommandConfigurer commandConfigurer)
        {
            this.inner = inner;
            this.commandConfigurer = commandConfigurer;
        }

        public int Execute(DbCommand command) =>
            this.inner.Execute(this.commandConfigurer
                .Configure(command));

        public Task<int> ExecuteAsync(DbCommand command,
            CancellationToken ct = default) =>
            this.inner.ExecuteAsync(this.commandConfigurer
                .Configure(command), ct);

        public void Read(DbCommand command,
            Action<IDataRecord> callback,
            CommandBehavior behavior = CommandBehavior.Default) =>
            this.inner.Read(this.commandConfigurer
                .Configure(command), callback, behavior);

        public Task ReadAsync(DbCommand command,
            Func<IDataRecord, Task> callback,
            CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken ct = default) =>
            this.inner.ReadAsync(this.commandConfigurer
                .Configure(command), callback, behavior, ct);

        public T Scalar<T>(DbCommand command) where T : struct =>
            this.inner.Scalar<T>(this.commandConfigurer.Configure(command));

        public Task<T> ScalarAsync<T>(DbCommand command,
            CancellationToken ct = default) where T : struct =>
            this.inner.ScalarAsync<T>(this.commandConfigurer.Configure(command), ct);

        public string String(DbCommand command) =>
            this.inner.String(this.commandConfigurer.Configure(command));

        public Task<string> StringAsync(DbCommand command,
            CancellationToken ct = default) =>
            this.inner.StringAsync(this.commandConfigurer.Configure(command), ct);

        public void Transaction(DbCommand[] commands,
            Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            commands = commands.Select( command =>
                    this.commandConfigurer.Configure(command))
                .ToArray();
            this.inner.Transaction(commands, resultIndexAndAffectedCallback);
        }

        public Task TransactionAsync(DbCommand[] commands,
            CancellationToken ct = default,
            Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            commands = commands.Select( command =>
                    this.commandConfigurer.Configure(command))
                .ToArray();

                return this.inner.TransactionAsync(commands, ct, resultIndexAndAffectedCallback);
        }
    }
}
