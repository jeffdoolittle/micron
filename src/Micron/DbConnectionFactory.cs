namespace Micron
{
    using System;
    using System.Data.Common;

    public class DbConnectionFactory<TProviderFactory, TBuilder> :
        IDbConnectionFactory
        where TProviderFactory : DbProviderFactory
        where TBuilder : DbConnectionStringBuilder
    {
        private readonly TProviderFactory providerFactory;
        private readonly TBuilder builder;

        public DbConnectionFactory(
            TProviderFactory providerFactory,
            Action<TBuilder> build)
        {
            this.providerFactory = providerFactory;
            this.builder = (TBuilder)providerFactory.CreateConnectionStringBuilder();
            build(this.builder);
        }

        public DbConnection CreateConnection()
        {
            var connectionString = this.builder.ConnectionString;
            var connection = this.providerFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}