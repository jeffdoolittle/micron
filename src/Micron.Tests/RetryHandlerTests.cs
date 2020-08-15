/* 
 *  File: UnitTest1.cs
 *  
 *  Copyright © 2020 Jeff Doolittle.
 *  All rights reserved.
 *  
 *  Licensed under the BSD 3-Clause License. See LICENSE in project root folder for full license text.
 */

namespace Micron.Tests
{
    using System;
    using System.Threading.Tasks;
    using Micron.SqlClient;
    using Xunit;

    public class RetryHandlerTests
    {
        [Fact]
        public async Task Can_create_a_simple_retry_handler()
        {
            var handler = new RetryHandler(5, 1000, ex => true);
            await handler.Execute(() => Task.CompletedTask);
        }

        [Fact]
        public void Throw_exception_when_retry_count_exceeds_max() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(6, 1000, ex => true));

        [Fact]
        public void Throw_exception_when_retry_count_deceeds_min() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(0, 1000, ex => true));

        [Fact]
        public void Throw_exception_when_backoff_interval_exceeds_max() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(1, 249, ex => true));

        [Fact]
        public void Throw_exception_when_backoff_interval_deceeds_min() => _ =
             Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RetryHandler(1, 300001, ex => true));
    }
}
