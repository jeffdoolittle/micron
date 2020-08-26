namespace Micron.TestClient
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var scope = new ConsoleScope();

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

            // var downloaders = fileNames.Select(x => new Downloader(client, x + ".tsv.gz"));
            // var downloadTasks = downloaders.Select(x => x.DownloadAsync());

            // await Task.WhenAll(downloadTasks);

            // downloaders.ToList().ForEach(d => d.Dispose());

            var titlesFile = new FileInfo("title.basics.tsv");
            using var fs = titlesFile.OpenRead();
            using var rdr = new StreamReader(fs);

            var cts = new CancellationTokenSource(5000);
            var firstLine = false;

            do
            {
                var line = await rdr.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                if (!firstLine)
                {
                    firstLine = true;
                    continue;
                }

                var tsvRow = TitleTsvRow.FromLine(line);
                Console.WriteLine(tsvRow);

                var row = new TitleDbRow
                {
                    TitleId = tsvRow.TitleId,
                    TitleType = tsvRow.TitleType,
                    PrimaryTitle = tsvRow.PrimaryTitle,
                    OriginalTitle = tsvRow.OriginalTitle,
                    IsAdult = tsvRow.IsAdult == "1",
                    StartYear = Convert.ToInt32(tsvRow.StartYear),
                    EndYear = ImdbNull.IsImdbNull(tsvRow.EndYear)
                        ? (int?)null
                        : Convert.ToInt32(tsvRow.EndYear),
                    RuntimeMinutes = ImdbNull.IsImdbNull(tsvRow.RuntimeMinutes)
                        ? (int?)null
                        : Convert.ToInt32(tsvRow.RuntimeMinutes),
                    GenresCsv = tsvRow.GenresArray
                };
                Console.WriteLine(row);

                if (cts.Token.IsCancellationRequested)
                {
                    break;
                }
            }
            while (true);

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
        public string? TitleId { get; set; }
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
        public string? TitleId { get; set; }
        public string? TitleType { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public bool IsAdult { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public string? GenresCsv { get; set; }

        public override string ToString() =>
            string.Join("\t",
                this.TitleId, this.TitleType, this.PrimaryTitle,
                this.OriginalTitle, this.IsAdult, this.StartYear,
                this.EndYear, this.RuntimeMinutes, this.GenresCsv);
    }

    public class Downloader : IDisposable
    {
        private readonly HttpClient client;
        private readonly string fileName;
        private HttpResponseMessage? response;

        public Downloader(HttpClient client, string fileName)
        {
            this.client = client;
            this.fileName = fileName;
        }

        public async Task DownloadAsync()
        {
            var archiveFile = new FileInfo(this.fileName);
            var dataFile = new FileInfo(this.fileName.Replace(".gz", ""));

            if (archiveFile.Exists)
            {
                var expiry = TimeSpan.FromDays(1);
                var age = DateTimeOffset.UtcNow - archiveFile.LastWriteTimeUtc;
                if (age < expiry)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"File cache not expired. Skipping download of {this.fileName}.");
                    Console.ResetColor();
                }
                else
                {
                    await this.DoDownload(archiveFile);
                }
            }

            if (dataFile.Exists)
            {
                dataFile.Delete();
            }

            await this.UnGzip(archiveFile, dataFile);

        }

        private async Task DoDownload(FileInfo fileInfo)
        {
            this.response = await this.client.GetAsync(this.fileName);

            if (this.response.IsSuccessStatusCode && fileInfo.Exists)
            {
                fileInfo.Delete();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Deleted existing file {this.fileName}");
                Console.ResetColor();
            }

            await using var ms = await this.response.Content.ReadAsStreamAsync();
            await using var fs = File.Create(fileInfo.FullName);
            _ = ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
        }

        private async Task UnGzip(FileInfo archiveFile, FileInfo dataFile)
        {
            using var archiveStream = archiveFile.OpenRead();
            using var gzip = new GZipStream(archiveStream, CompressionMode.Decompress);
            using var dataStream = dataFile.OpenWrite();

            const int chunk = 4096;
            var read = 0;
            var buffer = new byte[chunk];

            do
            {
                read = await gzip.ReadAsync(buffer, 0, chunk);
                await dataStream.WriteAsync(buffer, 0, read);
            } while (read > 0);
        }

        public void Dispose() => this.response?.Dispose();
    }
}
