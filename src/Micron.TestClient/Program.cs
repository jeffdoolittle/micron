namespace Micron.TestClient
{
    using System;

    public class Program
    {
        public static int Main(string[] args)
        {
            if (args?.Length < 2)
            {
                return 1;
            }

            var verb = args[0];
            var noun = args[1];



        }
    }
}
