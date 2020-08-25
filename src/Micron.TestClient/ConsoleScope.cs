namespace Micron.TestClient
{
    using System;

    public class ConsoleScope : IDisposable
    {
        public void Dispose() =>
            Console.ResetColor();
    }
}
