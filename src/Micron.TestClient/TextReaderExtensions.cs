namespace Micron.TestClient
{
    using System.Collections.Generic;
    using System.IO;

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
}
