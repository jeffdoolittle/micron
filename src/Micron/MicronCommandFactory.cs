namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class MicronCommandHandlerFactory : IMicronCommandHandlerFactory
    {
        public MicronCommandHandlerFactory()
        {
        }

        public IMicronCommandHandler Build()
        {
            throw new NotImplementedException();
        }
    }

    public class MicronCommandFactory : IMicronCommandFactory
    {
        // private static readonly MicronCommandCache Cache =
        //     new MicronCommandCache();

        private readonly string defaultParameterPrefix;

        public MicronCommandFactory() : this("@")
        {
        }

        public MicronCommandFactory(string defaultParameterPrefix = "@") =>
            this.defaultParameterPrefix = defaultParameterPrefix;

        public MicronCommand CreateCommand(string commandText,
            params object[] parameters) =>
                this.CreateCommand(commandText, _ =>
                    {
                        for (var p = 0; p < parameters.Length; p++)
                        {
                            _ = _.InParameter($"{this.defaultParameterPrefix}{p}",
                                parameters[p]);
                        }
                    });

        public MicronCommand CreateCommand(string commandText,
            IDictionary<string, object> parameters) =>
                this.CreateCommand(commandText, _ =>
                {
                    foreach (var p in parameters)
                    {
                        _ = _.InParameter(p.Key,
                            p.Value);
                    }
                });

        public MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameterExpression) =>
                this.CreateCommand(commandText, parameterExpression, null);

        public MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameterExpression,
            Action<IMicronCommandFactoryExpression>? configure)
        {
            var command = new MicronCommand
            {
                CommandText = commandText
            };

            if (configure != null)
            {
                var configurer = new CommandConfigurer(command);
                configure(configurer);
            }

            var builder = new ParameterBuilder();
            parameterExpression(builder);
            builder.Parameters.ForEach(p => command.Parameters.Add(p));

            return command;
        }

        private class CommandConfigurer : IMicronCommandFactoryExpression
        {
            private readonly MicronCommand command;
            public CommandConfigurer(MicronCommand command) =>
                this.command = command;

            public void Configure(int commandTimeoutSeconds = 5, CommandType commandType = CommandType.Text)
            {
                this.command.CommandTimeoutSeconds = commandTimeoutSeconds;
                this.command.CommandType = commandType;
            }
        }

        private class ParameterBuilder : IMicronCommandFactoryParameterExpression
        {
            public List<MicronParameter> Parameters { get; } =
                new List<MicronParameter>();

            private IMicronCommandFactoryParameterExpression Parameter(
                string name,
                object? value,
                Type valueType,
                ParameterDirection direction,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null)
            {
                var dbType = MicronTypeMap.Instance.MapTypeToDbType(valueType);
                var parameterDataType = (MicronParameterDataType)dbType;

                var parameter = new MicronParameter
                {
                    Direction = direction,
                    Name = name,
                    Value = value,
                    DataType = parameterDataType,
                    IsNullable = MicronTypeMap.Instance.IsNullable(valueType)
                };

                if (configure != null)
                {
                    var configurer = new ParameterConfigurer(parameter);
                    configure(configurer);
                }

                this.Parameters.Add(parameter);

                return this;
            }

            public IMicronCommandFactoryParameterExpression InParameter(string name,
                object? value,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null)
            {
                var valueType = value?.GetType() ?? typeof(object);
                return this.Parameter(name, value, valueType, ParameterDirection.Input, configure);
            }

            public IMicronCommandFactoryParameterExpression InParameter<T>(string name,
                T value,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, value, typeof(T), ParameterDirection.Input, configure);

            public IMicronCommandFactoryParameterExpression OutParameter<T>(string name,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, null, typeof(T), ParameterDirection.Output, configure);

            public IMicronCommandFactoryParameterExpression ReturnParameter<T>(string name,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, null, typeof(T), ParameterDirection.ReturnValue, configure);
        }

        private class ParameterConfigurer : IMicronCommandFactoryParameterConfigurationExpression
        {
            private readonly MicronParameter parameter;

            public ParameterConfigurer(MicronParameter parameter) =>
                this.parameter = parameter;

            public void Configure(byte precision, byte scale)
            {
                this.parameter.Precision = precision;
                this.parameter.Scale = scale;
            }

            public void Configure(int size) =>
                this.parameter.Size = size;
        }
    }
}
