namespace Micron.Tests
{
    using System;
    using Micron.Retry;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class DbCommandHandlerFactoryTests
    {
        [Fact]
        public void Can_create_a_db_command_handler_factory()
        {
            var retry = RetryHandler
                .Catch<Exception>(ex => true)
                .Retry(3, 50);
            var logger = NullLogger<IDbCommandHandler>.Instance;
            var configurer = new DbCommandConfigurerPipeline(cmd => cmd);

            var factory = new DbCommandHandlerFactory(retry, logger, configurer);

            var handler = factory.Build();
        }
    }
}
