namespace Micron.TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Micron.Retry;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;

    public static class TextReaderExtensions
    {
        public static async IAsyncEnumerable<string?> ReadLinesAsync(this StreamReader reader)
        {
            while(!reader.EndOfStream)
            {
                yield return await reader.ReadLineAsync();
            }
        }

        public static IEnumerable<string?> ReadLines(this StreamReader reader)
        {
            while(!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }
    }

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var scope = new ConsoleScope();

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
            var downloadTasks = downloaders.Select(x => x.DownloadAsync());

            await Task.WhenAll(downloadTasks);

            downloaders.ToList().ForEach(d => d.Dispose());

            var cst = new CancellationTokenSource();
            var ct = cst.Token;

            var logger = services.GetService<ILogger<IMicronCommandHandler>>();

            var connectionFactory = DbConnectionFactory.Create(new SQLiteFactory(), new SQLiteConnectionStringBuilder("Data Source=file:memdb1?mode=memory&cache=shared"));
            var commandFactory = new MicronCommandFactory();
            var retry = RetryHandler.Catch<SQLiteException>().Retry(3, _ => _.Interval(tries => tries * tries * BackoffInterval.MinBackoffMilliseconds));
            var commandHandlerFactory = new MicronCommandHandlerFactory(retry, logger, connectionFactory);
            var handler = commandHandlerFactory.Build();

            var script = new SQLiteSchemaBuilder().CreateTables();
            var micronCommand = commandFactory.CreateCommand(script);

            _ = await handler.ExecuteAsync(micronCommand, ct);

            Console.WriteLine("Executed ddl.");

            var titlesFile = new FileInfo("title.basics.tsv");
            using var fs = titlesFile.OpenRead();
            using var rdr = new StreamReader(fs);

            var tsvRows = rdr.ReadLines().Where(line => line != null).Skip(1);
            var lineCount = 0;

            var commands = tsvRows.Select(line =>
            {
                if (line == null)
                {
                    throw new ArgumentException("line cannot be null");
                }

                var tsvRow = TitleTsvRow.FromLine(line);

                try
                {
                    var dbRow = new TitleDbRow
                    {
                        TitleId = tsvRow.TitleId,
                        TitleType = tsvRow.TitleType,
                        PrimaryTitle = tsvRow.PrimaryTitle,
                        OriginalTitle = tsvRow.OriginalTitle,
                        IsAdult = tsvRow.IsAdult == "1" ? 1 : 0,
                        StartYear = ImdbNull.IsImdbNull(tsvRow.StartYear)
                            ? (int?)null
                            : Convert.ToInt32(tsvRow.StartYear),
                        EndYear = ImdbNull.IsImdbNull(tsvRow.EndYear)
                            ? (int?)null
                            : Convert.ToInt32(tsvRow.EndYear),
                        RuntimeMinutes = ImdbNull.IsImdbNull(tsvRow.RuntimeMinutes)
                            ? (int?)null
                            : Convert.ToInt32(tsvRow.RuntimeMinutes),
                        GenresCsv = tsvRow.GenresArray
                    };

                    var insertSql = $"insert into title_basics values (@0, @1, @2, @3, @4, @5, @6, @7, @8)";
                    var insert = commandFactory.CreateCommand(insertSql,
                                                              dbRow.TitleId,
                                                              dbRow.TitleType ?? "",
                                                              dbRow.PrimaryTitle,
                                                              dbRow.OriginalTitle,
                                                              dbRow.IsAdult,
                                                              dbRow.StartYear,
                                                              dbRow.EndYear,
                                                              dbRow.RuntimeMinutes,
                                                              dbRow.GenresCsv);

                    lineCount++;
                    return insert;
                }
                catch (Exception)
                {
                    using var scope = new ConsoleScope();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Bad input row!");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(tsvRow);
                    throw;
                }
            });

            var insertHandler = commandHandlerFactory.Build();
            insertHandler.Batch(commands, 10000);

            Console.WriteLine($"Lines: {lineCount}");

            return 0;
        }
    }

    public class ImdbNull
    {
        public static readonly ImdbNull Instance = new ImdbNull();

        public static readonly string NullString = @"\N";

        private ImdbNull() { }

        public override bool Equals(object? obj) =>
            ReferenceEquals(obj, this) || (obj is ImdbNull);

        public static bool IsImdbNull(string? value) =>
            value != null && value == @"\N";

        public override int GetHashCode() => 0;
        public override string? ToString() => NullString;

        public static implicit operator string(ImdbNull value) =>
            value?.ToString() ?? NullString;
    }

    public class ImdbConst
    {
        public ImdbConst(string value)
        {
            if (value.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new ArgumentException("Value must contain only alphanumeric characters", nameof(value));
            }
        }
    }

    public class TitleTsvRow
    {
        public string TitleId { get; set; } = "";
        public string? TitleType { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public string? IsAdult { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }
        public string? RuntimeMinutes { get; set; }
        public string? GenresArray { get; set; }

        public static TitleTsvRow FromLine(string tsvLine)
        {
            var row = new TitleTsvRow();
            var parts = tsvLine.Split("\t");
            var p = 0;
            row.TitleId = parts[p++];
            row.TitleType = parts[p++];
            row.PrimaryTitle = parts[p++];
            row.OriginalTitle = parts[p++];
            row.IsAdult = parts[p++];
            row.StartYear = parts[p++];
            row.EndYear = parts[p++];
            row.RuntimeMinutes = parts[p++];
            row.GenresArray = parts[p++];
            return row;
        }

        public override string ToString() =>
            string.Join("\t",
                this.TitleId, this.TitleType, this.PrimaryTitle,
                this.OriginalTitle, this.IsAdult, this.StartYear,
                this.EndYear, this.RuntimeMinutes, this.GenresArray);
    }

    public class TitleDbRow
    {
        public string TitleId { get; set; } = "";
        public string? TitleType { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public int IsAdult { get; set; } = 0;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public string? GenresCsv { get; set; }

        public override string ToString() =>
            string.Join("\t",
                this.TitleId, this.TitleType, this.PrimaryTitle,
                this.OriginalTitle, this.IsAdult, this.StartYear,
                this.EndYear, this.RuntimeMinutes, this.GenresCsv);
    }

    public class SQLiteSchemaBuilder
    {
        public string CreateTables()
        {
            var tables = new List<string>
            {
                this.CreateTitleBasicsTable()
            };
            return string.Join(Environment.NewLine, tables);
        }

        private string CreateTable(string tableName, IEnumerable<string> columns)
        {
            var sb = new StringBuilder();
            _ = sb
                .Append($"CREATE TABLE IF NOT EXISTS {tableName} (")
                .Append(string.Join(",", columns))
                .Append(") WITHOUT ROWID;");

            return sb.ToString();
        }

        private string CreateTitleBasicsTable()
        {
            var columns = new List<string>
            {
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.TitleId))} TEXT PRIMARY KEY",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.TitleType))} TEXT",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.PrimaryTitle))} TEXT",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.OriginalTitle))} TEXT",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.IsAdult))} INTEGER NOT NULL",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.StartYear))} INTEGER",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.EndYear))} INTEGER",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.RuntimeMinutes))} INTEGER",
                $"{StringFns.ToSnakeCase(nameof(TitleDbRow.GenresCsv))} TEXT",
            };

            return this.CreateTable("title_basics", columns);
        }
    }
}
