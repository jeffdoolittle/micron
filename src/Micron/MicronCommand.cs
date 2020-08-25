namespace Micron
{
    using System.Collections.Generic;
    using System.Data;

    public class MicronCommand
    {
        private readonly List<MicronParameter> parameters = new List<MicronParameter>();

        public string CommandText { get; set; } = "";

        public CommandType CommandType { get; set; } = CommandType.Text;

        public int CommandTimeoutSeconds { get; set; } = 5;

        public IList<MicronParameter> Parameters => this.parameters;
    }
}
