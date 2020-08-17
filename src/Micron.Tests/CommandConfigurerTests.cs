namespace Micron.SqlClient
{
    using System;
    using Xunit;

    public class CommandConfigurerTests
    {
        [Fact]
        public void Can_do_stuff()
        {
            var factory = new CommandFactory(_ => _
                .Connection(() => null)
                .OnException<ArgumentException>(ex => ex
                    .Retry(5, backoff =>
                        backoff.Interval(attempt => attempt * attempt * 50))
                )
            );


        }
    }
}
