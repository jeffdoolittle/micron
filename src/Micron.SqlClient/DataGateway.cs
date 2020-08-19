namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.SqlClient.Retry;

    public static class DataGatewayExtensions
    {
        public static ValueTask<T> Scalar<T>(this IDataGateway gateway, string commandText, CancellationToken ct = default)
        {
            return gateway.Scalar<T>(commandText, ct);
        }
    }

    public class DataGateway : IDataGateway
    {
        private Func<Task<DbConnection>> connectionFactory;
        private IRetryHandler retryHandler;

        public DataGateway(Action<ISqlGatewayConfigurationRootExpression> configure)
        {
            var builder = new CommandConfigurationBuilder(this);
            configure(builder);
        }

        public async Task Execute(ICommandRequest request)
        {
            async Task exec(IEnumerable<ICommand> commands)
            {
                using var conn = await this.connectionFactory();
                using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                foreach (var command in commands)
                {
                    using var cmd = BuildCommand(command, conn, tran);
                    var affected = await cmd.ExecuteNonQueryAsync(request.CancellationToken).ConfigureAwait(false);
                    if (command.ExpectedAffectedRows != affected)
                    {
                        throw new MicronException($"Expected ${command.ExpectedAffectedRows} affected rows but returned ${affected}.");
                    }
                }
                await tran.CommitAsync(request.CancellationToken).ConfigureAwait(false);
                await conn.CloseAsync();
            }

            await this.retryHandler.Execute(() => exec(request.Commands));
        }

        public async ValueTask<T> Scalar<T>(IScalarRequest<T> request)
        {
            T value = default;

            static TValue convert<TValue>(object value)
            {
                try
                {
                    return (TValue)Convert.ChangeType(value, typeof(TValue));
                }
                catch (Exception ex)
                {
                    throw new MicronException($"Unable to convert ${value} to {typeof(T)}", ex);
                }
            }

            async Task exec(string commandText, params object[] parameters)
            {
                using var conn = await this.connectionFactory();
                using var cmd = BuildCommand(request, conn);
                var result = await cmd.ExecuteScalarAsync();
                value = convert<T>(result);
                await conn.CloseAsync();
            }

            await this.retryHandler.Execute(() => exec(request.CommandText, request.Parameters));

            return value;
        }

        public async IAsyncEnumerable<T> Query<T>(IQuery<T> query)
        {
            IAsyncEnumerable<T> enumerable = null;

            async IAsyncEnumerable<TValue> exec<TValue>(IQuery<TValue> query)
            {
                using var conn = await this.connectionFactory();
                using var cmd = BuildCommand(query, conn);
                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    yield return await query.Map(rdr);
                }
                await conn.CloseAsync();
            }

            await this.retryHandler.Execute(() =>
            {
                enumerable = exec(query);
                return Task.CompletedTask;
            });

            await using var enumerator = enumerable.GetAsyncEnumerator();
            for (var more = true; more;)
            {
                more = await enumerator.MoveNextAsync();
                yield return enumerator.Current;
            }
        }

        private static DbCommand BuildCommand(IDataStatement statement,
            DbConnection conn, DbTransaction tran = null)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = statement.CommandText;

            if (tran != null)
            {
                cmd.Transaction = tran;
            }

            foreach (var parameter in statement.Parameters)
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
            private IRetryTimesExpression retryExpression;

            public CommandConfigurationBuilder(SqlGateway gateway)
                => this.gateway = gateway;

            public ISqlGatewayConfigurationExceptionConditionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
                where TConnection : DbConnection
            {
                this.gateway.connectionFactory = async () => await connectionFactory();
                return this;
            }

            public ISqlGatewayConfigurationRetryExpression OnException<TException>(Func<TException, bool> condition = null)
                where TException : DbException
            {
                this.retryExpression = RetryHandler.OnException(condition);
                return this;
            }

            public void Retry(RetryTimes times, BackoffInterval backoff) =>
                this.gateway.retryHandler = this.retryExpression.Retry(times, backoff);


            public void Retry(RetryTimes times, Action<IBackoffIntervalExpression> configureBackoff) =>
                this.gateway.retryHandler = this.retryExpression.Retry(times, configureBackoff);
        }
    }

    public interface ISqlGatewayConfigurationRootExpression
    {
        ISqlGatewayConfigurationExceptionConditionExpression Connection<TConnection>(Func<Task<TConnection>> connectionFactory)
            where TConnection : DbConnection;
    }

    public interface ISqlGatewayConfigurationExceptionConditionExpression
    {
        ISqlGatewayConfigurationRetryExpression OnException<TException>(Func<TException, bool> condition = null)
            where TException : DbException;
    }

    public interface ISqlGatewayConfigurationRetryExpression
    {
        void Retry(RetryTimes times, BackoffInterval backoff);

        void Retry(RetryTimes times,
            Action<IBackoffIntervalExpression> configureBackoff);
    }
}
