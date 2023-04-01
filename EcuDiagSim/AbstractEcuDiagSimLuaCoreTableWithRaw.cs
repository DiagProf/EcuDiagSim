#region License

// // MIT License
// //
// // Copyright (c) 2023 Joerg Frank
// // http://www.diagprof.com/
// //
// // Permission is hereby granted, free of charge, to any person obtaining a copy
// // of this software and associated documentation files (the "Software"), to deal
// // in the Software without restriction, including without limitation the rights
// // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// // copies of the Software, and to permit persons to whom the Software is
// // furnished to do so, subject to the following conditions:
// //
// // The above copyright notice and this permission notice shall be included in all
// // copies or substantial portions of the Software.
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// // SOFTWARE.

#endregion

using System.Runtime.InteropServices.JavaScript;
using ISO22900.II;
using Microsoft.Extensions.Logging;
using Neo.IronLua;
using System.Text.RegularExpressions;
using DiagEcuSim;

namespace EcuDiagSim
{
    internal abstract class AbstractEcuDiagSimLuaCoreTableWithRaw : AbstractEcuDiagSimLuaCoreTable
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<AbstractEcuDiagSimLuaCoreTableWithRaw>();
        protected LuaTable RawTable;

        protected AbstractEcuDiagSimLuaCoreTableWithRaw(LuaEcuDiagSimUnit luaEcuDiagSimUnit, string tableName, ComLogicalLink cll) : base(
            luaEcuDiagSimUnit, tableName, cll)
        {
            RawTable = (LuaTable)Table.Members["Raw"];
        }

        internal override void Refresh()
        {
            base.Refresh();
            RawTable = (LuaTable)Table.Members["Raw"];
        }

        protected override string? GetResponseString(string testerRequest)
        {
            try
            {
                _luaEcuDiagSimUnit.RwLocker.EnterReadLock();

                var testerRequestTrimmed = testerRequest.Replace(" ", "").upper();
                object? rawTableResponseObj = null;
                foreach ( var rawTableItem in RawTable )
                {
                    var requestKeyTrimmed = ((string)rawTableItem.Key).Replace(" ", "").upper();
                    if ( testerRequestTrimmed.Equals(requestKeyTrimmed) )
                    {
                        rawTableResponseObj = rawTableItem.Value;
                        break;
                    }
                }

                if ( rawTableResponseObj == null )
                {
                    foreach ( var rawTableItem in RawTable )
                    {
                        var requestKeyTrimmed = ((string)rawTableItem.Key).Replace(" ", "").upper();
                        if ( requestKeyTrimmed.EndsWith("*") )
                        {
                            if ( testerRequestTrimmed.StartsWith(requestKeyTrimmed.Replace("*", "")) )
                            {
                                rawTableResponseObj = rawTableItem.Value;
                                break;
                            }
                        }
                    }
                }

                if (rawTableResponseObj == null) {
                    foreach (var rawTableItem in RawTable) {
                        var requestKeyTrimmed = ((string)rawTableItem.Key).Replace(" ", "").upper();
                        if (requestKeyTrimmed.Contains("x"))
                        {
                            var pattern = @"^" + requestKeyTrimmed.Replace("x", ".");
                            var m = Regex.Match(requestKeyTrimmed, pattern);
                            if ( m.Success )
                            {
                                rawTableResponseObj = rawTableItem.Value;
                                break;
                            }
                        }
                    }
                }

                string simulatorResponseString = null;

                if ( rawTableResponseObj is string str )
                {
                    if ( string.IsNullOrWhiteSpace(str) )
                    {
                        //like UDS Suppress Positive Response
                        simulatorResponseString = string.Empty;
                    }
                    else
                    {
                        simulatorResponseString = str.Trim();
                    }
                }
                else if ( rawTableResponseObj is Delegate anonymousFunc )
                {
                    try
                    {
                        simulatorResponseString = ((dynamic)anonymousFunc)(testerRequest);
                    }
                    catch ( Exception ex )
                    {
                        var luaExceptionData = LuaExceptionData.GetData(ex); // get stack trace
                        _logger.LogCritical(ex, "LUA intern {luaExceptionData} makes trouble with File {fullFileName}", luaExceptionData,
                            _luaEcuDiagSimUnit.FullLuaFileName.FullName);
                    }
                }

                return simulatorResponseString;
            }
            finally
            {
                _luaEcuDiagSimUnit.RwLocker.ExitReadLock();
            }
        }
    }
}
