namespace Micron
{
    using Micron.Retry;
    using Microsoft.Extensions.Logging;

    public class MicronCommandHandlerFactory : IMicronCommandHandlerFactory
    {
        private readonly IRetryHandler retryHandler;
        private readonly ILogger<IMicronCommandHandler> logger;
        private readonly IDbConnectionFactory dbConnectionFactory;
        private readonly IDbCommandHandlerFactory dbCommandHandlerFactory;

        public MicronCommandHandlerFactory(
            IRetryHandler retryHandler,
            ILogger<IMicronCommandHandler> logger,
            IDbConnectionFactory dbConnectionFactory,
            IDbCommandHandlerFactory dbCommandHandlerFactory)
        {
            this.retryHandler = retryHandler;
            this.logger = logger;
            this.dbConnectionFactory = dbConnectionFactory;
            this.dbCommandHandlerFactory = dbCommandHandlerFactory;
        }

        public IMicronCommandHandler Build()
        {
            var dbConnection = this.dbConnectionFactory.CreateConnection();
            var dbCommandHandler = this.dbCommandHandlerFactory.Build();


            var handler = new MicronCommandHandler(dbConnection, dbCommandHandler);

            // var pipeline = new DbCommandHandlerPipelineDecorator(handler, this.commandConfigurer);

            // var retry = new DbCommandHandlerRetryDecorator(pipeline, this.retryHandler, this.logger);

            // var logging = new DbCommandHandlerLoggingDecorator(retry, this.logger);

            var exceptionHandling = new MicronCommandHandlerExceptionDecorator(handler, this.logger); // todo: introduce logger

            return exceptionHandling;
        }
    }
}
