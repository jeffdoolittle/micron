namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;

    public class MicronCommandFactory : IMicronCommandFactory
    {
        private static readonly ReaderWriterLockSlim CacheLock =
            new ReaderWriterLockSlim();

        private static readonly IDictionary<string, MicronCommand> Cache =
            new Dictionary<string, MicronCommand>();

        private readonly string defaultParameterPrefix = "@";

        public MicronCommandFactory() : this("@") { }

        public MicronCommandFactory(string defaultParameterPrefix = "@") =>
            this.defaultParameterPrefix = defaultParameterPrefix;

        public MicronCommand CreateCommand(string commandText,
            params string[] parameterNames) =>
                this.CreateCommand(commandText, parameterNames.Select((x, i) => new
                {
                    Name = $"{this.defaultParameterPrefix}{i}",
                    Type = typeof(object)
                }).ToDictionary(x => x.Name, x => x.Type));

        public MicronCommand CreateCommand(string commandText,
            IDictionary<string, Type> parameterNamesAndTypes) =>
                this.CreateCommand(commandText, _ =>
                {
                    foreach (var kv in parameterNamesAndTypes)
                    {
                        _ = _.InParameter(kv.Key, kv.Value);
                    }
                });

        public MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameterExpression) =>
                this.CreateCommand(commandText, parameterExpression, null);

        public MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameterExpression,
            Action<IMicronCommandFactoryExpression>? configure)
        {
            var parameterBuilder = new ParameterBuilder();
            parameterExpression(parameterBuilder);
            var parameters = parameterBuilder.Parameters;

            var commandConfiguration = new CommandConfiguration();
            configure?.Invoke(commandConfiguration);

            try
            {
                CacheLock.EnterUpgradeableReadLock();

                var hash = MicronCommand.GetHash(commandText, commandConfiguration.CommandType,
                    parameters);

                if (Cache.ContainsKey(hash))
                {
                    return Cache[hash];
                }

                CacheLock.EnterWriteLock();

                var command = new MicronCommand(commandText,
                    commandConfiguration.CommandType,
                    commandConfiguration.TimeoutSeconds,
                    parameters.ToArray());

                Cache[hash] = command;

                return command;
            }
            finally
            {
                CacheLock.ExitWriteLock();
                CacheLock.ExitReadLock();
            }
        }

        private class CommandConfiguration : IMicronCommandFactoryExpression
        {
            public void Configure(int commandTimeoutSeconds = 5, CommandType commandType = CommandType.Text)
            {
                this.TimeoutSeconds = commandTimeoutSeconds;
                this.CommandType = commandType;
            }

            public int TimeoutSeconds { get; private set; } = 10;
            public CommandType CommandType { get; private set; } = CommandType.Text;
        }

        private class ParameterBuilder : IMicronCommandFactoryParameterExpression
        {
            private readonly List<MicronParameter> parameters = new List<MicronParameter>();
            public MicronParameter[] Parameters => this.parameters.ToArray();

            private IMicronCommandFactoryParameterExpression Parameter(
                string name,
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
                    DataType = parameterDataType,
                    IsNullable = MicronTypeMap.Instance.IsNullable(valueType)
                };

                if (configure != null)
                {
                    var configurer = new ParameterConfigurer(parameter);
                    configure(configurer);
                }

                this.parameters.Add(parameter);

                return this;
            }

            public IMicronCommandFactoryParameterExpression InParameter(string name, Type? valueType = null,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, valueType ?? typeof(object), ParameterDirection.Input, configure);

            public IMicronCommandFactoryParameterExpression InParameter<T>(string name,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, typeof(T), ParameterDirection.Input, configure);

            public IMicronCommandFactoryParameterExpression OutParameter<T>(string name,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, typeof(T), ParameterDirection.Output, configure);

            public IMicronCommandFactoryParameterExpression ReturnParameter<T>(string name,
                Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null) =>
                    this.Parameter(name, typeof(T), ParameterDirection.ReturnValue, configure);
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
