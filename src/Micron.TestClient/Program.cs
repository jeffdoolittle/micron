﻿namespace Micron.TestClient
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

            var downloaders = fileNames.Select(x => new Downloader(client, x + ".tsv.gz"));
            var downloadTasks = downloaders.Select(x => x.DownloadAsync());

            await Task.WhenAll(downloadTasks);

            downloaders.ToList().ForEach(d => d.Dispose());






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
            } while(read == chunk);
        }

        public void Dispose() => this.response?.Dispose();
    }
}
