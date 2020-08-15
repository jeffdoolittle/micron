/* 
 *  File: Class1.cs
 *  
 *  Copyright © 2020 Jeff Doolittle.
 *  All rights reserved.
 *  
 *  Licensed under the BSD 3-Clause License. See LICENSE in project root folder for full license text.
 */

namespace Micron.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public interface IResultMapper
    {
        Task Map(Func<DbDataReader, Exception, Task> mapper);
    }
}
