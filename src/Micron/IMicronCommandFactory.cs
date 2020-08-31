namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public interface IMicronCommandFactory
    {
        MicronCommand CreateCommand(string commandText,
            params object[] parameters);

        MicronCommand CreateCommand(string commandText,
            IDictionary<string, object> parameters);

        MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameters);

        MicronCommand CreateCommand(string commandText,
            Action<IMicronCommandFactoryParameterExpression> parameters,
            Action<IMicronCommandFactoryExpression>? configure);
    }

    public interface IMicronCommandFactoryExpression
    {
        void Configure(int commandTimeoutSeconds = 5, CommandType commandType = CommandType.Text);
    }

    public interface IMicronCommandFactoryParameterExpression
    {
        IMicronCommandFactoryParameterExpression InParameter(string name, object? value,
            Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null);
        IMicronCommandFactoryParameterExpression InParameter<T>(string name, T value,
            Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null);
        IMicronCommandFactoryParameterExpression OutParameter<T>(string name,
            Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null);
        IMicronCommandFactoryParameterExpression ReturnParameter<T>(string name,
            Action<IMicronCommandFactoryParameterConfigurationExpression>? configure = null);
    }

    public interface IMicronCommandFactoryParameterConfigurationExpression
    {
        void Configure(byte precision, byte scale);

        void Configure(int size);
    }
}
