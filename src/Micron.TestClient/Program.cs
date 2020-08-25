namespace Micron.TestClient
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
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
                "name.basics.tsv.gz",
                "title.akas.tsv.gz",
                "title.basics.tsv.gz",
                "title.crew.tsv.gz",
                "title.episode.tsv.gz",
                "title.principals.tsv.gz",
                "title.ratings.tsv.gz"
            };

            var downloaders = fileNames.Select(x => new Downloader(client, x));
            var downloadTasks = downloaders.Select(x => x.DownloadAsync());

            await Task.WhenAll(downloadTasks);

            downloaders.ToList().ForEach(d => d.Dispose());

            Console.Write("Successfully downloaded IMDB data files.");

            return 0;
        }
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
            var fileInfo = new FileInfo(this.fileName);

            this.response = await this.client.GetAsync(this.fileName);

            if (this.response.IsSuccessStatusCode && fileInfo.Exists)
            {
                fileInfo.Delete();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Deleted existing file {this.fileName}");
                Console.ResetColor();
            }

            await using var ms = await this.response.Content.ReadAsStreamAsync();
            await using var fs = File.Create(fileInfo.FullName);
            _ = ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
        }

        public void Dispose() => this.response?.Dispose();
    }
}
