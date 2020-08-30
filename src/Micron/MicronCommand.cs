namespace Micron
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public class MicronCommand
    {
        private readonly List<MicronParameter> parameters = new List<MicronParameter>();

        public MicronCommand() : this("", CommandType.Text, 10)
        {
        }

        public MicronCommand(string commandText, CommandType commandType, int commandTimeoutSeconds)
        {
            this.CommandText = commandText;
            this.CommandType = commandType;
            this.CommandTimeoutSeconds = commandTimeoutSeconds;

        }
        public string CommandText { get; set; } = "";

        public CommandType CommandType { get; set; } = CommandType.Text;

        public int CommandTimeoutSeconds { get; set; } = 5;

        public IList<MicronParameter> Parameters => this.parameters;

        public void MapTo(DbCommand command)
        {
            command.CommandText = this.CommandText;
            command.CommandType = this.CommandType;
            command.CommandTimeout = this.CommandTimeoutSeconds;
            for (var p = 0; p < this.parameters.Count; p++)
            {
                var mp = this.parameters[p];
                var parameter = command.CreateParameter();
                parameter.ParameterName = mp.Name;
                parameter.Value = mp.Value;
                parameter.DbType = (DbType)mp.DataType;
                parameter.Direction = mp.Direction;
                parameter.IsNullable = mp.IsNullable;
                parameter.Precision = mp.Precision;
                parameter.Scale = mp.Scale;
                parameter.Size = mp.Size;

                _ = command.Parameters.Add(parameter);
            }
        }
    }
}
