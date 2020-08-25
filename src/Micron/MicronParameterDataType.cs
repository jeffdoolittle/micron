namespace Micron
{
    /// <summary>
    /// See <see cref="System.Data.DbType" />
    /// </summary>
    public enum MicronParameterDataType
    {
        Binary = 1,
        Boolean = 3,
        Decimal = 7,
        Double = 8,
        Guid = 9,
        Int32 = 11,
        Int64 = 12,
        Object = 13,
        String = 16,
        StringFixedLength = 23,
        DateTimeOffset = 27,
    }
}
