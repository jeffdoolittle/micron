namespace Micron
{
    using System.Data.Common;

    public class DbConnectionFactory<TProviderFactory, TBuilder> :
        IDbConnectionFactory
        where TProviderFactory : DbProviderFactory
        where TBuilder : DbConnectionStringBuilder
    {
        private readonly TProviderFactory providerFactory;

        protected TBuilder ConnectionStringBuilder { get; }

        public DbConnectionFactory(
            TProviderFactory providerFactory,
            TBuilder connectionStringBuilder)
        {
            this.providerFactory = providerFactory;
            this.ConnectionStringBuilder = connectionStringBuilder;
        }

        public DbConnection CreateConnection()
        {
            var connectionString = this.ConnectionStringBuilder
                .ConnectionString;

            var connection = this.providerFactory
                .CreateConnection();

            connection.ConnectionString = connectionString;

            return connection;
        }
    }
}
