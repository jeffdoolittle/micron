namespace Micron
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    public class MicronCommand : IEquatable<MicronCommand>
    {
        internal MicronCommand() : this("", CommandType.Text, 10, new MicronParameter[0])
        {
        }

        internal MicronCommand(string commandText,
            CommandType commandType,
            int commandTimeoutSeconds,
            MicronParameter[] parameters)
        {
            this.CommandText = commandText;
            this.CommandType = commandType;
            this.CommandTimeoutSeconds = commandTimeoutSeconds;
            this.Parameters = parameters.Select(c => c.Clone()).ToArray();
        }

        public string CommandText { get; }

        public CommandType CommandType { get; }

        public int CommandTimeoutSeconds { get; }

        public MicronParameter[] Parameters { get; }

        public void MapTo(DbCommand command, object[] values)
        {
            command.CommandText = this.CommandText;
            command.CommandType = this.CommandType;
            command.CommandTimeout = this.CommandTimeoutSeconds;
            if (values.Length != this.Parameters.Length)
            {
                throw new RootCauseException($"Expected {this.Parameters.Length} parameters but {values.Length} values were supplied.");
            }
            for (var p = 0; p < this.Parameters.Length; p++)
            {
                var mp = this.Parameters[p];
                var parameter = command.CreateParameter();
                parameter.ParameterName = mp.Name;
                parameter.Value = values[p];
                parameter.DbType = (DbType)mp.DataType;
                parameter.Direction = mp.Direction;
                parameter.IsNullable = mp.IsNullable;
                parameter.Precision = mp.Precision;
                parameter.Scale = mp.Scale;
                parameter.Size = mp.Size;

                _ = command.Parameters.Add(parameter);
            }
        }

        public bool Equals(MicronCommand other) => this.GetCommandHash() == other.GetCommandHash();

        public override bool Equals(object obj) => (obj is MicronCommand cmd) && this.Equals(cmd);

        public override int GetHashCode() => this.GetCommandHash().GetHashCode();

        internal static string GetHash(string commandText, CommandType commandType, MicronParameter[] parameters)
        {
            var value = string.Join(",",
                new[] { nameof(MicronCommand), commandText, commandType.ToString() }
                .Union(parameters.SelectMany(x => x.GetParameterValues()))
                );
            return Hash.CreateHexadecimalHashString(value);
        }

        private string GetCommandHash() =>
            GetHash(this.CommandText, this.CommandType, this.Parameters);
    }
}
