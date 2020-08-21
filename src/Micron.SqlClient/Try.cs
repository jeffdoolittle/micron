namespace Micron.SqlClient
{
    using System;
    using System.Threading.Tasks;

    public static class Try
    {
        public static void To(Action action) =>
            _ = To(() =>
                {
                    action();
                    return Unit.Default;
                });

        public static T To<T>(Func<T> function)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                throw new MicronException("An unhandled exception occurred.", ex);
            }
        }

        public static async Task ToAsync(Func<Task> function) =>
            _ = await ToAsync(async () =>
                {
                    await function().ConfigureAwait(false);
                    return Unit.Default;
                }).ConfigureAwait(false);

        public static async Task<T> ToAsync<T>(Func<Task<T>> function)
        {
            try
            {
                return await function();
            }
            catch (Exception ex)
            {
                throw new MicronException("An unhandled exception occurred.", ex);
            }
        }
    }
}
