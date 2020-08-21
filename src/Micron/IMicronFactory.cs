namespace Micron
{
    using System.Data.Common;
    using Micron.Retry;

    public interface IMicronFactory
    {

    }

    public interface IMicronConfiguration
    {
        DbProviderFactory ProviderFactory { get; }
        DbConnectionStringBuilder ConnectionStringBuilder { get; }
        IRetryHandler RetryHandler { get; }
    }
}