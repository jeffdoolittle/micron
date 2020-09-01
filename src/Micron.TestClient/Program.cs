namespace Micron.TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.Retry;
    using Micron.TestClient.DataModel;
    using Micron.TestClient.DbModel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var scope = new ConsoleScope();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[ Process Id: {Process.GetCurrentProcess().Id} ]");
            Console.ResetColor();

            var serviceCollection = new ServiceCollection();
            _ = serviceCollection.AddLogging(cfg =>
                cfg.AddConsole(logger =>
                    logger.Format = ConsoleLoggerFormat.Systemd));

            var services = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            // if (args == null || args.Length == 0)
            // {
            //     Console.ForegroundColor = ConsoleColor.Red;
            //     Console.WriteLine("No arguments provided.");
            //     return 1;
            // }

            ////////////////////
            // get and unzip data if past cache date
            ////////////////////

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://datasets.imdbws.com/")
            };

            var fileNames = new[]
            {
                "name.basics",
                "title.akas",
                "title.basics",
                "title.crew",
                "title.episode",
                "title.principals",
                "title.ratings"
            };

            var downloaders = fileNames.Select(x => new Downloader(client, x + ".tsv.gz"));
            var downloadTasks = downloaders.Select(x => x.DownloadAsync(30));

            await Task.WhenAll(downloadTasks);

            downloaders.ToList().ForEach(d => d.Dispose());

            ////////////////////
            // load data to sqlite database
            ////////////////////

            var sw = new Stopwatch();
            sw.Start();

            var cst = new CancellationTokenSource();
            var ct = cst.Token;

            var logger = services.GetService<ILogger<IMicronCommandHandler>>();

            var connectionStringBuilder = new SQLiteConnectionStringBuilder("Data Source=imdb.sqlite")
            {
                JournalMode = SQLiteJournalModeEnum.Off
            };
            var connectionFactory = DbConnectionFactory.Create(new SQLiteFactory(), connectionStringBuilder);
            var retry = RetryHandler.Catch<SQLiteException>().Retry(3, _ => _.Interval(tries => tries * tries * BackoffInterval.MinBackoffMilliseconds));
            var commandHandlerFactory = new MicronCommandHandlerFactory(retry, logger, connectionFactory);

            var handler = commandHandlerFactory.Build();

            static string tableName<T>() => StringFns.ToSnakeCase(typeof(T).Name.Replace("DbRow", ""));

            var columns = TitleBasicsDbRow
                .TableColumns()
                .Select(col =>
                {
                    var dataType = col.RuntimeType.Name switch
                    {
                        nameof(Int32) => "INTEGER",
                        _ => "TEXT"
                    };

                    var sb = new StringBuilder()
                        .Append($"{col.Name} {dataType}")
                        .Append(col.IsPrimaryKey ? " PRIMARY KEY" : "")
                        .Append(col.IsNullable ? "" : " NOT NULL")
                        ;

                    return sb.ToString();
                    ;
                });

            var script = new SQLiteSchemaBuilder().CreateTable(tableName<TitleBasicsDbRow>(), columns);
            var commandFactory = new MicronCommandFactory();
            var micronCommand = commandFactory.CreateCommand(script);

            _ = await handler.ExecuteAsync(micronCommand, ct);

            Console.WriteLine($"Executed ddl in {sw.Elapsed}.");
            sw.Restart();

            var titlesFile = new FileInfo("title.basics.tsv");
            using var fs = titlesFile.OpenRead();
            using var rdr = new StreamReader(fs);

            var tsvRows = rdr.ReadLinesAsync().Where(line => line != null).Skip(1);
            var lineCount = 0;

            var commands = tsvRows.Select(line =>
            {
                if (line == null)
                {
                    throw new ArgumentException("line cannot be null");
                }

                var tsvRow = TitleTsvRow.FromLine(line);

                var dbRow = TitleBasicsDbRow.From(tsvRow);

                var insertSql = $"insert into {tableName<TitleBasicsDbRow>()} values (@0, @1, @2, @3, @4, @5, @6, @7, @8)";
                var insert = commandFactory.CreateCommand(insertSql,
                                                          dbRow.TitleId,
                                                          dbRow.TitleType,
                                                          dbRow.PrimaryTitle,
                                                          dbRow.OriginalTitle,
                                                          dbRow.IsAdult,
                                                          dbRow.StartYear,
                                                          dbRow.EndYear,
                                                          dbRow.RuntimeMinutes,
                                                          dbRow.GenresCsv);

                lineCount++;
                return insert;
            });

            Console.WriteLine($"Prepared command enumerable in {sw.Elapsed}.");
            sw.Restart();

            var insertHandler = commandHandlerFactory.Build();
            await insertHandler.BatchAsync(commands, 100000, ct);

            Console.WriteLine($"Lines: {lineCount} in {sw.Elapsed}");

            return 0;
        }
    }

    public class SQLiteSchemaBuilder
    {
        public string CreateTable(string tableName, IEnumerable<string> columns)
        {
            var sb = new StringBuilder();
            _ = sb
                .Append($"CREATE TABLE IF NOT EXISTS {tableName} (")
                .Append(string.Join(", ", columns))
                .Append(") WITHOUT ROWID;");

            return sb.ToString();
        }
    }
}
