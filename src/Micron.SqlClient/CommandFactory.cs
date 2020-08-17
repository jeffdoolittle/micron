namespace Micron.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    public class CommandFactory
    {
        private Func<Task<DbConnection>> connectionFactory;

        public CommandFactory(Action<ICommandConfigurerRootExpression> configure)
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
            ICommandConfigurerRootExpression,
            ICommandConfigurerExceptionExpression
        {
            private readonly CommandFactory factory;

            public CommandConfigurationBuilder(CommandFactory factory)
                => this.factory = factory;

            public ICommandConfigurerExceptionExpression Connection(Func<Task<DbConnection>> connectionFactory)
            {
                this.factory.connectionFactory = connectionFactory;
                return this;
            }

            public ICommandConfigurerExceptionExpression OnException(Action<IExceptionFilterExpression<Exception>> filter)
            {
                return this;
            }

            public ICommandConfigurerExceptionExpression OnException<TException>(Action<IExceptionFilterExpression<TException>> filter = null) where TException : Exception
            {
                return this;
            }
        }
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

    public interface ICommandConfigurerRootExpression
    {
        ICommandConfigurerExceptionExpression Connection(Func<Task<DbConnection>> connectionFactory);
    }

    public interface ICommandConfigurerExceptionExpression
    {
        ICommandConfigurerExceptionExpression OnException(Action<IExceptionFilterExpression<Exception>> filter);

        ICommandConfigurerExceptionExpression OnException<TException>(Action<IExceptionFilterExpression<TException>> filter = null)
            where TException : Exception;
    }

    public interface IExceptionFilterExpression<T> : IExceptionRetryExpression
        where T : Exception
    {
        IExceptionRetryExpression Matching(Func<Exception, bool> condition);

        IExceptionRetryExpression Matching<TException>(Func<TException, bool> condition)
            where TException : Exception;
    }

    public interface IExceptionRetryExpression
    {
        ICommandConfigurerExceptionExpression Retry(RetryTimes times, BackoffInterval backoff);

        ICommandConfigurerExceptionExpression Retry(RetryTimes times,
            Action<IBackoffIntervalConfigurationExpression> configureBackoff);
    }

    public interface IBackoffIntervalConfigurationExpression
    {
        ICommandConfigurerExceptionExpression Interval(IntervalCalculation intervalCalculation);
    }
}
