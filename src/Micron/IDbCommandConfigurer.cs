namespace Micron
{
    using System;
    using System.Data.Common;

    public interface IDbCommandConfigurer
    {
        DbCommand Configure(DbCommand command);
    }

    public class DbCommandConfigurerPipeline : IDbCommandConfigurer
    {
        private readonly Func<DbCommand, DbCommand> pipeline;

        public DbCommandConfigurerPipeline(Func<DbCommand, DbCommand> pipeline) =>
            this.pipeline = pipeline;

        public DbCommand Configure(DbCommand command) =>
            this.pipeline(command);
    }
}
