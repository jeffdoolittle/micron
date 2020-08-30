namespace Micron
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MicronCommandHandlerFactory : IMicronCommandHandlerFactory
    {
        public MicronCommandHandlerFactory()
        {
        }

        public IMicronCommandHandler Build()
        {
            throw new NotImplementedException();
        }
    }

    public class MicronCommandHandler : IMicronCommandHandler
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IDbCommandHandler dbCommandHandler;
        public MicronCommandHandler(IDbConnectionFactory connectionFactory, IDbCommandHandler dbCommandHandler)
        {
            this.connectionFactory = connectionFactory;
            this.dbCommandHandler = dbCommandHandler;
        }

        public int Execute(MicronCommand command)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.Execute(dbCommand);
        }

        public Task<int> ExecuteAsync(MicronCommand command, CancellationToken ct = default)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.ExecuteAsync(dbCommand, ct);
        }

        public void Read(MicronCommand command, Action<IDataRecord> callback, CommandBehavior behavior = CommandBehavior.Default)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            this.dbCommandHandler.Read(dbCommand, callback, behavior);
        }

        public Task ReadAsync(MicronCommand command, Func<IDataRecord, Task> callback, CommandBehavior behavior = CommandBehavior.Default, CancellationToken ct = default)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.ReadAsync(dbCommand, callback, behavior, ct);
        }

        public T Scalar<T>(MicronCommand command) where T : struct
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.Scalar<T>(dbCommand);
        }

        public Task<T> ScalarAsync<T>(MicronCommand command, CancellationToken ct = default)
            where T : struct
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.ScalarAsync<T>(dbCommand, ct);
        }

        public string String(MicronCommand command)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.String(dbCommand);
        }

        public Task<string> StringAsync(MicronCommand command, CancellationToken ct = default)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommand = connection.CreateCommand();
            command.MapTo(dbCommand);
            return this.dbCommandHandler.StringAsync(dbCommand, ct);
        }

        public void Transaction(MicronCommand[] commands, Action<int, int>? resultIndexAndAffectedCallback = null)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommands = commands.Select(x =>
            {
                var dbCommand = connection.CreateCommand();
                x.MapTo(dbCommand);
                return dbCommand;
            });

            this.dbCommandHandler.Transaction(dbCommands.ToArray(), resultIndexAndAffectedCallback);
        }

        public Task TransactionAsync(MicronCommand[] commands, CancellationToken ct = default, Func<int, int, Task>? resultIndexAndAffectedCallback = null)
        {
            var connection = this.connectionFactory.CreateConnection();
            var dbCommands = commands.Select(x =>
            {
                var dbCommand = connection.CreateCommand();
                x.MapTo(dbCommand);
                return dbCommand;
            });

            return this.dbCommandHandler.TransactionAsync(dbCommands.ToArray(), ct, resultIndexAndAffectedCallback);
        }
    }
}
