// namespace Micron
// {
//     using System.Collections.Generic;
//     using System.Data;
//     using System.Threading;

//     public class MicronCommandCache
//     {
//         private static readonly ReaderWriterLockSlim CacheLock =
//             new ReaderWriterLockSlim();

//         private readonly IDictionary<string, ParameterSupplier> commandCache =
//             new Dictionary<string, ParameterSupplier>();

//         private class ParameterSupplier
//         {
//             private readonly Dictionary<string, MicronParameterPrototype> prototypes =
//                 new Dictionary<string, MicronParameterPrototype>();

//             public void Add(MicronParameterPrototype prototype) =>
//                 this.prototypes.Add(prototype.Name, prototype);

//             public MicronParameter CreateParameter(string key,
//                 object value)
//             {
//                 var prototype = this.prototypes[key];
//                 var size = 0;
//                 if (value is string v)
//                 {
//                     size = v.Length;
//                 }
//                 return new MicronParameter
//                 {
//                     Name = prototype.Name,
//                     DataType = prototype.DataType,
//                     Direction = prototype.Direction,
//                     IsNullable = prototype.IsNullable,
//                     Value = value,
//                     Size = size
//                 };
//             }
//         }

//         public MicronCommand GetCommand(string commandText,
//             IDictionary<string, object> parameters)
//         {
//             var command = new MicronCommand
//             {
//                 CommandText = commandText
//             };

//             var hash = Hash.CreateBase64HashString(commandText);

//             CacheLock.EnterReadLock();

//             try
//             {
//                 if (!this.commandCache.ContainsKey(hash))
//                 {
//                     try
//                     {
//                         CacheLock.EnterWriteLock();

//                         var supplier = new ParameterSupplier();

//                         foreach (var kv in parameters)
//                         {
//                             var value = kv.Value;
//                             var type = kv.Value?.GetType() ?? typeof(object);
//                             var dbType = MicronTypeMap.Instance.MapTypeToDbType(value?.GetType());
//                             var parameterDataType = (MicronParameterDataType)dbType;

//                             var prototype = new MicronParameterPrototype
//                             {
//                                 Name = kv.Key,
//                                 Direction = ParameterDirection.Input,
//                                 DataType = parameterDataType,
//                                 IsNullable = MicronTypeMap.Instance.IsNullable(type)
//                             };

//                             supplier.Add(prototype);
//                         }

//                         this.commandCache.Add(hash, supplier);
//                     }
//                     finally
//                     {
//                         CacheLock.ExitWriteLock();
//                     }
//                 }
//                 else
//                 {
//                     var supplier = this.commandCache[hash];

//                     foreach (var kv in parameters)
//                     {
//                         command.Parameters.Add(supplier.CreateParameter(kv.Key, kv.Value));
//                     }
//                 }
//             }
//             finally
//             {
//                 CacheLock.ExitReadLock();
//             }

//             return command;
//         }
//     }
// }
