namespace Micron
{
    using System;
    using System.Data.Common;
    using Micron.Retry;
    using Microsoft.Extensions.Logging;

    public class DbCommandHandlerFactory : IDbCommandHandlerFactory
    {
        private readonly IRetryHandler retryHandler;
        private readonly ILogger<IDbCommandHandler> logger;
        private readonly Func<DbCommand, DbCommand> commandConfigurationPipeline;

        public DbCommandHandlerFactory(
            IRetryHandler retryHandler,
            ILogger<IDbCommandHandler> logger,
            Func<DbCommand, DbCommand> commandConfigurationPipeline
            )
        {
            this.retryHandler = retryHandler;
            this.logger = logger;
            this.commandConfigurationPipeline = commandConfigurationPipeline;
        }

        public IDbCommandHandler Build()
        {
            var handler = new DbCommandHandler(this.retryHandler);

            var decorated = new DbCommandHandlerDecorator(handler,
                this.commandConfigurationPipeline,
                this.logger);

            return decorated;
        }
    }
}
