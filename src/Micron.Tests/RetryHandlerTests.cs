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
    using System.Threading.Tasks;
    using Micron.SqlClient;
    using Xunit;

    public class RetryHandlerTests
    {
        [Fact]
        public async Task Test1()
        {
            var handler = new RetryHandler(5, 1000, ex => true);
            await handler.Execute(() => Task.CompletedTask);
        }
    }
}
