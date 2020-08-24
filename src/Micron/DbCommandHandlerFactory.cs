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
        private readonly IDbCommandConfigurer commandConfigurer;

        public DbCommandHandlerFactory(
            IRetryHandler retryHandler,
            ILogger<IDbCommandHandler> logger,
            IDbCommandConfigurer commandConfigurer
            )
        {
            this.retryHandler = retryHandler;
            this.logger = logger;
            this.commandConfigurer = commandConfigurer;
        }

        public IDbCommandHandler Build()
        {
            var handler = new DbCommandHandler();

            var pipeline = new DbCommandHandlerPipelineDecorator(handler, this.commandConfigurer);

            var retry = new DbCommandHandlerRetryDecorator(pipeline, this.retryHandler, this.logger);

            var logging = new DbCommandHandlerLoggingDecorator(retry, this.logger);

            var exceptionHandling = new DbCommandHandlerExceptionDecorator(logging, this.logger);

            return exceptionHandling;
        }
    }
}
