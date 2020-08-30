namespace Micron
{
    public class MicronCommandHandlerFactory : IMicronCommandHandlerFactory
    {
        private readonly IDbConnectionFactory dbConnectionFactory;
        private readonly IDbCommandHandlerFactory dbCommandHandlerFactory;

        public MicronCommandHandlerFactory(
            IDbConnectionFactory dbConnectionFactory,
            IDbCommandHandlerFactory dbCommandHandlerFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
            this.dbCommandHandlerFactory = dbCommandHandlerFactory;
        }

        public IMicronCommandHandler Build()
        {
            var dbConnection = this.dbConnectionFactory.CreateConnection();
            var handler = this.dbCommandHandlerFactory.Build();
            return new MicronCommandHandler(dbConnection, handler);
        }
    }
}
