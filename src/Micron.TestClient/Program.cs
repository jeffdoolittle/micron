namespace Micron.TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var scope = new ConsoleScope();

            if (args == null || args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No arguments provided.");
                return 1;
            }

            var command = Commands
                .FirstOrDefault(_ => _.CanHandle(args)) ?? DefaultCommand;

            await command.HandleAsync(args);

            return 0;
        }

        private static readonly List<IConsoleCommand> Commands = new List<IConsoleCommand>
        {
            new LoadCommand()
        };

        private static readonly IConsoleCommand DefaultCommand = new HelpCommand();
    }

    public class HelpCommand : IConsoleCommand
    {
        public bool CanHandle(string[] args) => true;

        public Task HandleAsync(string[] args)
        {
            Console.WriteLine("Show help information!");
            return Task.CompletedTask;
        }
    }

    public class LoadCommand : IConsoleCommand
    {
        public bool CanHandle(string[] args) =>
            string.Equals("load", args[0], StringComparison.InvariantCultureIgnoreCase);

        public Task HandleAsync(string[] args)
        {
            var noun = args[1];



            return Task.CompletedTask;
        }
    }

    public interface IConsoleCommand
    {
        bool CanHandle(string[] args);
        Task HandleAsync(string[] args);
    }

    public class ConsoleScope : IDisposable
    {
        public void Dispose() =>
            Console.ResetColor();
    }
}
