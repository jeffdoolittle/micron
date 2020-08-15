/* 
 *  File: Class1.cs
 *  
 *  Copyright © 2020 Jeff Doolittle.
 *  All rights reserved.
 *  
 *  Licensed under the BSD 3-Clause License. See LICENSE in project root folder for full license text.
 */

namespace Micron.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    public class ResultMapper : IResultMapper
    {
        public Task Map(Func<DbDataReader, Exception, Task> mapper)
        {
            throw new NotImplementedException();
        }
    }

    public class CommandAdapter
    {
        private readonly DbConnection connection;
        private readonly Func<DbCommand, DbCommand> commandConfigurationPipeline;

        public CommandAdapter(DbConnection connection, Func<DbCommand, DbCommand> commandConfigurationPipeline)
        {
            this.connection = connection;
            this.commandConfigurationPipeline = commandConfigurationPipeline;
        }

        private class CommandConfigurer : ICommandConfigurerConnectExpression
        {
            public ICommandConfigurerPipelineExpression Connection(Func<DbConnection> conn)
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface ICommandConfigurerConnectExpression
    {
        ICommandConfigurerPipelineExpression Connection(Func<DbConnection> conn);
    }

    public interface ICommandConfigurerPipelineExpression
    {
        ICommandConfigurerExceptionExpression Pipeline(Func<DbCommand, DbCommand> commandConfigurationPipeline);

    }

    public interface ICommandConfigurerExceptionExpression
    {
        ICommandConfigurerRetryExpression OnException(Func<Exception, bool> condition);
    }

    public interface ICommandConfigurerRetryExpression
    {
        /// <summary>
        /// Retry configuration
        /// </summary>
        /// <param name="times">The number of times to retry.</param>
        /// <param name="backoffMilliseconds">The number of milliseconds to wait between retries.</param>
        /// <returns></returns>
        ICommandConfigurerExceptionExpression Retry(RetryTimes times, BackoffInterval backoff);
    }
}
