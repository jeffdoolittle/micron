namespace Micron
{
    using System.Data;

    public static class MicronParameterDataTypeExtensions
    {
        public static DbType ToDbType(this MicronParameterDataType dataType) =>
            MicronTypeMap.Instance.MapMicronParameterTypeToDbType(dataType);
    }
}
