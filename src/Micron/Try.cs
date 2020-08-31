namespace Micron
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public static class Try
    {
        public static void To(Action action, ILogger logger) =>
            _ = To(() =>
                {
                    action();
                    return Unit.Default;
                }, logger);

        public static T To<T>(Func<T> function, ILogger logger)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred.");
                throw new MicronException("An unhandled exception occurred.", ex);
            }
        }

        public static async Task ToAsync(Func<Task> function, ILogger logger, CancellationToken ct = default) =>
            _ = await ToAsync(async () =>
                {
                    await function().ConfigureAwait(false);
                    return Unit.Default;
                }, logger, ct).ConfigureAwait(false);

        public static async Task<T> ToAsync<T>(Func<Task<T>> function, ILogger logger, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return await function().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred.");
                throw new MicronException("An unhandled exception occurred.", ex);
            }
        }
    }
}
