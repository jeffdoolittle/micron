namespace Micron
{
    using Micron.Retry;
    using Microsoft.Extensions.Logging;

    public class MicronCommandHandlerFactory : IMicronCommandHandlerFactory
    {
        private readonly IRetryHandler retryHandler;
        private readonly ILogger<IMicronCommandHandler> logger;
        private readonly IDbConnectionFactory dbConnectionFactory;

        public MicronCommandHandlerFactory(
            IRetryHandler retryHandler,
            ILogger<IMicronCommandHandler> logger,
            IDbConnectionFactory dbConnectionFactory)
        {
            this.retryHandler = retryHandler;
            this.logger = logger;
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public IMicronCommandHandler Build()
        {
            var dbCommandHandler = new DbCommandHandler();

            var handler = new MicronCommandHandler(this.dbConnectionFactory, dbCommandHandler);

            var retry = new MicronCommandHandlerRetryDecorator(handler, this.retryHandler, this.logger);

            var logging = new MicronCommandHandlerLoggingDecorator(retry, this.logger);

            var exceptionHandling = new MicronCommandHandlerExceptionDecorator(logging, this.logger);

            return exceptionHandling;
        }
    }
}
