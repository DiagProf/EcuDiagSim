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

using DiagEcuSim;
using ISO22900.II;
using Microsoft.Extensions.Logging;
using Neo.IronLua;

namespace EcuDiagSim
{
    internal abstract class AbstractEcuDiagSimLuaCoreTableWithRaw : AbstractEcuDiagSimLuaCoreTable
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<AbstractEcuDiagSimLuaCoreTableWithRaw>();
        protected LuaTable RawTable;

        protected AbstractEcuDiagSimLuaCoreTableWithRaw(LuaEcuDiagSimUnit luaEcuDiagSimUnit, string tableName, ComLogicalLink cll): base(luaEcuDiagSimUnit, tableName, cll)
        {
            RawTable = (LuaTable)Table.Members["Raw"];
        }

        //private void SendRaw(string simulatorResponseString)
        //{
        //    _logger.LogInformation("Table: {TableName}, SimuResp: {responseString}", TableName, simulatorResponseString);
        //    _ = SendAsync(simulatorResponseString);
        //}

        protected virtual async Task<bool> SendAsync(string simulatorResponseString)
        {
            bool isOksy = true;
            using (var simulatorResponseCop = Cll.StartCop(PduCopt.PDU_COPT_SENDRECV, 1, 0, ByteAndBitUtility.BytesFromHex(simulatorResponseString)))
            {
                var resultResponse = await simulatorResponseCop.WaitForCopResultAsync(Ct).ConfigureAwait(false);

                //for the information quite good... but breaks the order of how the events were fired
                resultResponse.PduEventItemResults().ForEach(result =>
                {
                    _logger.LogInformation("ReceiveThread - SendResponse: {SendResponse}",
                        ByteAndBitUtility.BytesToHexString(result.ResultData.DataBytes, spacedOut: true));
                    isOksy = false;
                });
                resultResponse.PduEventItemErrors().ForEach(error => { _logger.LogError("ReceiveThread - Error {error}", error.ErrorCodeId);
                    isOksy = false;
                });
                resultResponse.PduEventItemInfos().ForEach(info => { _logger.LogInformation("ReceiveThread - Info {error}", info.InfoCode); });
            }

            return isOksy;
        }

        internal override void Refresh()
        {
            base.Refresh();
            RawTable = (LuaTable)Table.Members["Raw"];
            SetupAdditionalLuaFunctions();
        }

        public virtual bool SetupAdditionalLuaFunctions()
        {
            try
            {
                ((dynamic)Table).sendRaw = new Action<string>(SendRawLuaCallback);
            }
            catch ( Exception e )
            {
                return false;
            }
            return true;
        }

        protected override string? GetResponseString(string testerRequest)
        {
            try
            {
                _luaEcuDiagSimUnit.RwLocker.EnterReadLock();

                var testerRequestTrimmed = testerRequest.Replace(" ", "").upper();
                object? rawTableResponseObj = null;
                foreach (var rawTableItem in RawTable)
                {
                    var requestKeyTrimmed = (((string)rawTableItem.Key)).Replace(" ", "").upper();
                    if (testerRequestTrimmed.Equals(requestKeyTrimmed))
                    {
                        rawTableResponseObj = rawTableItem.Value;
                        break;
                    }
                }

                if (rawTableResponseObj == null)
                {
                    foreach (var rawTableItem in RawTable)
                    {
                        var requestKeyTrimmed = (((string)rawTableItem.Key)).Replace(" ", "").upper();
                        if (requestKeyTrimmed.EndsWith("*"))
                        {
                            if (testerRequestTrimmed.StartsWith(requestKeyTrimmed.Replace("*", "")))
                            {
                                rawTableResponseObj = rawTableItem.Value;
                                break;
                            }
                        }
                    }
                }


                string simulatorResponseString = null;

                if (rawTableResponseObj is string str)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        //like UDS Suppress Positive Response
                        simulatorResponseString = String.Empty;
                    }
                    else
                    {
                        simulatorResponseString = str.Trim();
                    }
                }
                else if (rawTableResponseObj is Delegate anonymousFunc)
                {
                    try
                    {
                        simulatorResponseString = ((dynamic)anonymousFunc)(testerRequest);
                    }
                    catch (Exception ex)
                    {
                        var luaExceptionData = LuaExceptionData.GetData(ex); // get stack trace
                        _logger.LogCritical(ex, "LUA intern {luaExceptionData} makes trouble with File {fullFileName}", luaExceptionData, _luaEcuDiagSimUnit.FullLuaFileName.FullName);
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
