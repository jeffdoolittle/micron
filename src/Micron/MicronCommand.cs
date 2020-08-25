namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;

    public class MicronCommand
    {
        private readonly List<MicronParameter> parameters = new List<MicronParameter>();

        public string CommandText { get; set; } = "";

        public IList<MicronParameter> Parameters => this.parameters;

        public DbCommand ToDbCommand(Func<DbCommand> commandSupplier)
        {
            var command = commandSupplier();
            command.CommandText = this.CommandText;
            this.parameters.ForEach(parameter =>
            {
                var p = command.CreateParameter();
                p.ParameterName = parameter.Name ?? "";
                p.Value = parameter.Value;
                p.DbType = parameter.DataType.ToDbType();
                p.Direction = parameter.Direction;
                p.IsNullable = parameter.IsNullable;
                p.Precision = parameter.Precision;
                p.Scale = parameter.Scale;
                p.Size = parameter.Size;
                _ = command.Parameters.Add(p);
            });
            return command;
        }
    }
}
