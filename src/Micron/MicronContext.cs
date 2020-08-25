namespace Micron
{
    using System.Collections.Generic;

    public class MicronContext
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IDbCommandHandlerFactory handlerFactory;

        public MicronContext(IDbConnectionFactory connectionFactory,
            IDbCommandHandlerFactory handlerFactory)
        {
            this.connectionFactory = connectionFactory;
            this.handlerFactory = handlerFactory;
        }

        public string String(string commandText, params MicronParameter[] parameters)
        {
            var connection = this.connectionFactory.CreateConnection();
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandText = commandText;
            for (var p = 0; p < parameters.Length; p++)
            {
                var current = parameters[p];
                var parameter = command.CreateParameter();
                parameter.ParameterName = current.Name;
                parameter.Value = current.Value;
                parameter.DbType = current.DataType.ToDbType();
            }
            var handler = this.handlerFactory.Build();
            return handler.String(command);
        }
    }

    public class MicronCommand
    {
        public string CommandText { get; set; } = "";

        public IList<MicronParameter> Parameters { get; } = new List<MicronParameter>();
    }

    public class MicronParameter
    {
        public string? Name { get; set; }
        public object? Value { get; set; }
        public MicronParameterDataType? DataType { get; set; }
    }
}
