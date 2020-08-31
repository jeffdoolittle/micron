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

            var retry = new MicronCommandHandlerRetryDecorator(handler, this.retryHandler, this.logger);

            var logging = new MicronCommandHandlerLoggingDecorator(retry, this.logger);

            var exceptionHandling = new MicronCommandHandlerExceptionDecorator(logging, this.logger);

            return exceptionHandling;
        }
    }
}
