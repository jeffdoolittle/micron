namespace Micron.Tests
{
    using System;
    using System.Threading.Tasks;
    using Micron.SqlClient;
    using Xunit;

    public class RetryHandlerTests
    {
        [Fact]
        public async Task Can_create_a_retry_handler()
        {
            var handler = new RetryHandler(5, 1000, ex => true);
            await handler.Execute(() => Task.CompletedTask);
        }

        [Fact]
        public void Throw_exception_when_retry_count_exceeds_max() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(RetryTimes.MaxRetries + 1, 1000, ex => true));

        [Fact]
        public void Throw_exception_when_retry_count_deceeds_min() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(0, 1000, ex => true));

        [Fact]
        public void Throw_exception_when_backoff_interval_exceeds_max() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(1, BackoffInterval.MinBackoffMilliseconds - 1, ex => true));

        [Fact]
        public void Throw_exception_when_backoff_interval_deceeds_min() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(1, BackoffInterval.MaxBackoffMilliseconds + 1, ex => true));

        [Fact]
        public async Task Can_execute_a_retry_handler_once()
        {
            var count = 0;

            Task exec()
            {
                count++;
                return Task.CompletedTask;
            }

            var handler = new RetryHandler(RetryTimes.MaxRetries, 1000, ex => true);
            await handler.Execute(exec);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task Can_execute_a_retry_handler_max_times()
        {
            var tries = 0;
            var max = RetryTimes.MaxRetries;

            Task exec()
            {
                tries++;

                if (tries < max)
                {
                    throw new Exception();
                }

                return Task.CompletedTask;
            }

            var handler = new RetryHandler(RetryTimes.MaxRetries,
                BackoffInterval.MinBackoffMilliseconds, ex => true);
            await handler.Execute(exec);

            Assert.Equal(5, tries);
        }

        [Fact]
        public async Task Can_retry_for_specified_exception_types()
        {
            static bool canHandle(Exception ex) => ex is InvalidOperationException;

            var tries = 0;
            Task exec()
            {
                tries++;

                if (tries < RetryTimes.MaxRetries)
                {
                    throw new InvalidOperationException();
                }
                return Task.CompletedTask;
            }

            var handler = new RetryHandler(canHandle);
            await handler.Execute(exec);

            Assert.Equal(5, tries);
        }

        [Fact]
        public async Task Should_not_retry_for_unhandled_exception_types()
        {
            static bool canHandle(Exception ex) => ex is InvalidOperationException;

            var tries = 0;
            Task exec()
            {
                tries++;
                throw new Exception();
            }

            var handler = new RetryHandler(canHandle);
            _ = await Assert.ThrowsAsync<Exception>(() => handler.Execute(exec));

            Assert.Equal(1, tries);
        }
    }
}
