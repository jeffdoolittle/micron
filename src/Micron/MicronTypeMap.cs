namespace Micron
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class MicronTypeMap
    {
        private static readonly Lazy<MicronTypeMap> LazyInstance =
            new Lazy<MicronTypeMap>(() => new MicronTypeMap());

        private MicronTypeMap() { }

        public static MicronTypeMap Instance => LazyInstance.Value;

        public DbType MapTypeToDbType(Type? type)
        {
            if (type == null)
            {
                return TypeMap[typeof(object)];
            }

            if (TypeMap.ContainsKey(type))
            {
                return TypeMap[type];
            }

            return TypeMap[typeof(object)];
        }

        public DbType MapMicronParameterTypeToDbType(MicronParameterDataType? dataType)
        {
            if (dataType == null)
            {
                return DbType.String;
            }
            return (DbType)dataType.Value;
        }

        public bool IsNullable(Type type) =>
            type.IsClass
                || type == typeof(byte[])
                || NullableStructs.Contains(type);

        private static readonly List<Type> NullableStructs =
            new List<Type>(9)
            {
                typeof(int?),
                typeof(long?),
                typeof(double?),
                typeof(decimal?),
                typeof(bool?),
                typeof(char?),
                typeof(Guid?),
                typeof(DateTimeOffset?),
                typeof(TimeSpan?),
            };

        private static readonly Dictionary<Type, DbType> TypeMap =
            new Dictionary<Type, DbType>(21)
            {
                [typeof(int)] = DbType.Int32,
                [typeof(long)] = DbType.Int64,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = DbType.Time,
                [typeof(byte[])] = DbType.Binary,
                [typeof(int?)] = DbType.Int32,
                [typeof(long?)] = DbType.Int64,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = DbType.Time,
                [typeof(object)] = DbType.Object
            };
    }
}
