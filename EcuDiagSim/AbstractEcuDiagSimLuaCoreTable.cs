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
using static System.Net.Mime.MediaTypeNames;

namespace EcuDiagSim
{
    internal abstract class AbstractEcuDiagSimLuaCoreTable
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<AbstractEcuDiagSimLuaCoreTable>();
        protected readonly ComLogicalLink Cll;
        protected CancellationToken Ct;
        protected readonly LuaTable Table;
        private readonly LuaEcuDiagSimUnit _luaEcuDiagSimUnit;
        protected readonly string TableName;

        protected AbstractEcuDiagSimLuaCoreTable(LuaEcuDiagSimUnit luaEcuDiagSimUnit, string tableName, LuaTable table, ComLogicalLink cll)
        {
            _luaEcuDiagSimUnit = luaEcuDiagSimUnit;
            TableName = tableName;
            Table = table;
            Cll = cll;
        }

        public static DataForComLogicalLinkCreation GetDataForComLogicalLinkCreation(LuaTable luaTable)
        {
            // DataForComLogicalLinkCreation default data is suitable for the LightweightHeader
            var dataSetsForCllCreation = new DataForComLogicalLinkCreation();

            if ( luaTable.Members["DataForComLogicalLinkCreation"] is LuaTable table )
            {
                //in LUA File it looks like this
                //EcuName = {
                //    DataForComLogicalLinkCreation = {
                //        BusTypeShortName = "ISO_11898_2_DWCAN",
                //        ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
                //        DlcPinData = {
                //            ["6"] = "HI",
                //            ["14"] = "LOW",
                //        },
                //    },
                //    Raw = ....
                var busTypeShortName = (string)table.Members["BusTypeShortName"];
                var protocolShortName = (string)table.Members["ProtocolShortName"];
                if ( (LuaTable)table.Members["DlcPinData"] is LuaTable dlcPinData )
                {
                    Dictionary<uint, string> dlcPinDataDic = new();
                    foreach ( var pair in dlcPinData )
                    {
                        var success = uint.TryParse((string)pair.Key, out var number);
                        if ( success )
                        {
                            dlcPinDataDic.Add(number, (string)pair.Value);
                        }
                    }

                    dataSetsForCllCreation = new DataForComLogicalLinkCreation
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName,
                        DlcPinData = dlcPinDataDic
                    };
                }
                else
                {
                    //if DlcPinData is missing 
                    dataSetsForCllCreation = new DataForComLogicalLinkCreation
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName
                        //DlcPinData DLC is default in this case { { 6, "HI" }, { 14, "LOW" } };
                    };
                }
            }

            return dataSetsForCllCreation;
        }

        public virtual bool SetupComLogicalLink()
        {
            _luaEcuDiagSimUnit.DynamicEnvironment[TableName].sendRaw = new Action<string>(SendRaw);
            return true;
        }

        private void SendRaw(string simulatorResponseString)
        {
            _logger.LogInformation("Table: {TableName}, SimuResp: {responseString}", TableName, simulatorResponseString);
            _ = SendAsync(simulatorResponseString);
        }

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

        public abstract Task Connect(CancellationToken ct);

        protected string? GetResponseString(LuaTable rawTable, string testerRequest)
        {
            var testerRequestTrimmed = testerRequest.Replace(" ", "").upper();
            object? rawTableResponseObj = null;
            foreach (var rawTableItem in rawTable)
            {
                var requestKeyTrimmed = (((string)rawTableItem.Key)).Replace(" ", "").upper();
                if (testerRequestTrimmed.Equals(requestKeyTrimmed))
                {
                    rawTableResponseObj = rawTableItem.Value;
                    break;
                }
            }

            if ( rawTableResponseObj == null )
            {
                foreach (var rawTableItem in rawTable)
                {
                    var requestKeyTrimmed = (((string)rawTableItem.Key)).Replace(" ", "").upper();
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


            string simulatorResponseString = null;

            if (rawTableResponseObj is string str)
            {
                if ( string.IsNullOrWhiteSpace(str) )
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
                simulatorResponseString = ((dynamic)anonymousFunc)(testerRequest);
            }

            return simulatorResponseString;
        }
    }
}
