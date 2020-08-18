namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    public class SqlGateway : ISqlGateway
    {
        private Func<Task<DbConnection>> connectionFactory;
        private readonly List<IExceptionRetryConfiguration> retryConfigurations = 
            new List<IExceptionRetryConfiguration>();

        public SqlGateway(Action<ISqlGatewayConfigurationRootExpression> configure)
        {
            var builder = new CommandConfigurationBuilder(this);
            configure(builder);
        }

        public async Task Execute(params ICommand[] commands)
        {
            using var conn = await this.connectionFactory();
            using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            // TODO: add retry logic in here.
            // TODO: allow cancellation tokens

            foreach (var command in commands)
            {
                using var cmd = BuildCommand(command.CommandText, command.Parameters, conn, tran);
                var affected = await cmd.ExecuteNonQueryAsync();
                if (command.ExpectedAffectedRows != affected)
                {
                    throw new MicronException($"Expected ${command.ExpectedAffectedRows} affected rows but returned ${affected}.");
                }
            }
            await tran.CommitAsync();
        }

        public async ValueTask<T> Scalar<T>(string commandText, params object[] parameters)
        {
            using var conn = await this.connectionFactory();
            using var cmd = BuildCommand(commandText, parameters, conn);
            var result = await cmd.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public async IAsyncEnumerable<T> Query<T>(IQuery<T> query)
        {
            using var conn = await this.connectionFactory();
            using var cmd = BuildCommand(query.CommandText, query.Parameters, conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                yield return await query.Map(rdr);
            }
        }

        private static DbCommand BuildCommand(string commandText, object[] parameters, DbConnection conn, DbTransaction tran = null)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = commandText;

            if (tran != null)
            {
                cmd.Transaction = tran;
            }

            foreach (var parameter in parameters)
            {
                var p = cmd.CreateParameter();
                p.Value = parameter;
                _ = cmd.Parameters.Add(p);
            }
            return cmd;
        }

        private class CommandConfigurationBuilder :
            ISqlGatewayConfigurationRootExpression,
            ISqlGatewayConfigurationExceptionExpression
        {
            private readonly SqlGateway gateway;

            public CommandConfigurationBuilder(SqlGateway gateway)
                => this.gateway = gateway;

            public ISqlGatewayConfigurationExceptionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
                where TConnection : DbConnection
            {
                this.gateway.connectionFactory = async () => await connectionFactory();
                return this;
            }

            public ISqlGatewayConfigurationExceptionExpression OnException(Func<IExceptionFilterExpression, IExceptionRetryConfiguration> filterExpression)
            {
                var exceptionRetryConfiguration = filterExpression(ConfigureRetries.OnException());
                this.gateway.retryConfigurations.Add(exceptionRetryConfiguration);
                return this;
            }

            public ISqlGatewayConfigurationExceptionExpression OnException<TException>(Func<IExceptionFilterExpression, IExceptionRetryConfiguration> filterExpression = null) where TException : Exception
            {
                var exceptionRetryConfiguration = filterExpression(ConfigureRetries.OnException<TException>());
                this.gateway.retryConfigurations.Add(exceptionRetryConfiguration);
                return this;
            }
        }
    }

    public interface ISqlGateway
    {
         Task Execute(params ICommand[] commands);
         ValueTask<T> Scalar<T>(string commandText, params object[] parameters);
         IAsyncEnumerable<T> Query<T>(IQuery<T> query);
    }

    public interface ICommand
    {
        string CommandText { get; }
        object[] Parameters { get; }
        int ExpectedAffectedRows { get; }
    }

    public interface IQuery<T>
    {
        string CommandText { get; }
        object[] Parameters { get; }
        Func<IDataReader, Task<T>> Map { get; }
    }

    public interface ISqlGatewayConfigurationRootExpression
    {
        ISqlGatewayConfigurationExceptionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
            where TConnection : DbConnection;
    }

    public interface ISqlGatewayConfigurationExceptionExpression
    {
        ISqlGatewayConfigurationExceptionExpression OnException(Func<IExceptionFilterExpression, IExceptionRetryConfiguration> filterExpression);

        ISqlGatewayConfigurationExceptionExpression OnException<TException>(Func<IExceptionFilterExpression, IExceptionRetryConfiguration> filterExpression = null)
            where TException : Exception;
    }
}
