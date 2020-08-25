namespace Micron
{
    using System.Collections.Generic;

    public class MicronCommandFactory
    {
        private static readonly MicronCommandCache Cache =
            new MicronCommandCache();

        public MicronCommand CreateCommand(string commandText,
            params object[] parameters)
        {
            var dict = new Dictionary<string, object>();
            for(var p = 0; p < parameters.Length; p++)
            {
                // todo: may need to check how different providers
                // handle parameter prefixes
                dict.Add($"@{p}", parameters[p]);
            }

            return this.CreateCommand(commandText, dict);
        }

        public MicronCommand CreateCommand(string commandText,
            IDictionary<string, object> parameters) =>
                Cache.CreateCommand(commandText, parameters);
    }
}
