﻿namespace Micron.SqlClient
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
        private IExceptionRetryConfiguration retryConfiguration;

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
            // todo: make retry handling a function, not a configuration option. tell, don't ask!

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
            ISqlGatewayConfigurationExceptionConditionExpression,
            ISqlGatewayConfigurationRetryExpression
        {
            private readonly SqlGateway gateway;
            private IExceptionRetryExpression retryExpression;

            public CommandConfigurationBuilder(SqlGateway gateway)
                => this.gateway = gateway;

            public ISqlGatewayConfigurationExceptionConditionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
                where TConnection : DbConnection
            {
                this.gateway.connectionFactory = async () => await connectionFactory();
                return this;
            }

            public ISqlGatewayConfigurationRetryExpression OnException(Func<Exception, bool> condition = null)
            {
                this.retryExpression = ConfigureRetries.OnException(condition);
                return this;
            }

            public ISqlGatewayConfigurationRetryExpression OnException<TException>(Func<TException, bool> condition = null) where TException : Exception
            {
                this.retryExpression = ConfigureRetries.OnException(condition);
                return this;
            }

            public IExceptionRetryConfiguration Retry(RetryTimes times, BackoffInterval backoff) =>
                this.retryExpression.Retry(times, backoff);


            public IExceptionRetryConfiguration Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff) => 
                this.retryExpression.Retry(times, configureBackoff);
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
        ISqlGatewayConfigurationExceptionConditionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
            where TConnection : DbConnection;
    }

    public interface ISqlGatewayConfigurationExceptionConditionExpression
    {
        ISqlGatewayConfigurationRetryExpression OnException(Func<Exception, bool> condition = null);

        ISqlGatewayConfigurationRetryExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : Exception;
    }

    public interface ISqlGatewayConfigurationRetryExpression
    {
        IExceptionRetryConfiguration Retry(RetryTimes times, BackoffInterval backoff);

        IExceptionRetryConfiguration Retry(RetryTimes times,
            Action<IBackoffIntervalExpression> configureBackoff);

    }
}
