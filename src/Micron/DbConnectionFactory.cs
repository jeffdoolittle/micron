namespace Micron
{
    using System.Data.Common;

    public static class DbConnectionFactory
    {
        public static DbConnectionFactory<TProviderFactory, TBuilder> Create<TProviderFactory, TBuilder>(
            TProviderFactory providerFactory,
            TBuilder connectionStringBuilder)
            where TProviderFactory : DbProviderFactory
            where TBuilder : DbConnectionStringBuilder =>
                new DbConnectionFactory<TProviderFactory, TBuilder>(providerFactory, connectionStringBuilder);
    }

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
