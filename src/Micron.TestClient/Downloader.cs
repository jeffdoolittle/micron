namespace Micron.TestClient
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Threading.Tasks;

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

        public async Task DownloadAsync(int ifOlderThanDays)
        {
            var archiveFile = new FileInfo(this.fileName);
            var dataFile = new FileInfo(this.fileName.Replace(".gz", ""));

            var downloadArchive = true;
            if (archiveFile.Exists)
            {
                var expiry = TimeSpan.FromDays(ifOlderThanDays);
                var age = DateTimeOffset.UtcNow - archiveFile.LastWriteTimeUtc;
                if (age < expiry)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"File cache not expired. Skipping download of {this.fileName}.");
                    Console.ResetColor();
                    downloadArchive = false;
                }
            }

            if (downloadArchive)
            {
                await this.DoDownload(archiveFile);
            }

            var extract = true;
            if (dataFile.Exists)
            {
                var expiry = TimeSpan.FromDays(ifOlderThanDays);
                var age = DateTimeOffset.UtcNow - dataFile.LastWriteTimeUtc;
                if (age < expiry)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"File cache not expired. Skipping extraction of {this.fileName}.");
                    Console.ResetColor();
                    extract = false;
                }
            }

            if (extract)
            {
                dataFile.Delete();

                await this.UnGzip(archiveFile, dataFile);
            }
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
