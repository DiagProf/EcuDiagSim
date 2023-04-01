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
    internal abstract class AbstractEcuDiagSimLuaCoreTable
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<AbstractEcuDiagSimLuaCoreTable>();
        protected readonly ComLogicalLink Cll;
        protected readonly LuaEcuDiagSimUnit _luaEcuDiagSimUnit;
        protected readonly string TableName;
        protected readonly FlashFileReconstruction FlashFileReconstruction = new();

        protected LuaTable Table;

        protected AbstractEcuDiagSimLuaCoreTable(LuaEcuDiagSimUnit luaEcuDiagSimUnit, string tableName, ComLogicalLink cll)
        {
            _luaEcuDiagSimUnit = luaEcuDiagSimUnit;
            TableName = tableName;
            Cll = cll;
            Table = (LuaTable)luaEcuDiagSimUnit.Environment[tableName];
        }

        public static DataForComLogicalLinkCreation GetDataForComLogicalLinkCreation(LuaTable luaTable)
        {
            // DataForComLogicalLinkCreation default is suitable for the LightweightHeader
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
                //        CllCreateFlagRawMode = true
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

                //This we need that in the upcoming releases
                if ( (LuaTable)table.Members["CllCreateFlagRawMode"] is LuaTable flagRawMode )
                {
                    dataSetsForCllCreation.ComLogicalLinkCreationCreateFlag.RawMode = (bool)table.Members["CllCreateFlagRawMode"];
                }
            }

            return dataSetsForCllCreation;
        }

        internal virtual void Refresh()
        {
            Table = (LuaTable)_luaEcuDiagSimUnit.Environment[TableName];
            SetupAdditionalLuaFunctions();
        }

        public virtual bool SetupAdditionalLuaFunctions()
        {
            try
            {
                ((dynamic)Table).sendRaw = new Action<string>(SendRawLuaCallback);
                ((dynamic)Table).restartFlashFileReconstruction = new Action(FlashFileReconstruction.RestartReconstruction);
                ((dynamic)Table).addFlashBlock = new Action(FlashFileReconstruction.AddFlashBlock);
                ((dynamic)Table).setRequestDownloadInfos = new Func<string, string, string, bool>(FlashFileReconstruction.SetRequestDownloadInfos);
                ((dynamic)Table).addRequestPayload = new Func<string, bool>(FlashFileReconstruction.AddRequestPayload);
                ((dynamic)Table).reconstructFlashFile = new Func<string, bool>(FlashFileReconstruction.Reconstruct);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        protected virtual void SendRawLuaCallback(string simulatorResponseString)
        {
            _logger.LogInformation("Table: {TableName} > {responseString}", TableName, simulatorResponseString);
            _ = SendAsync(simulatorResponseString);
        }

        protected virtual async Task<bool> SendAsync(string simulatorResponseString, CancellationToken ct = default)
        {
            var isOkay = true;
            using ( var sendCop = Cll.StartCop(PduCopt.PDU_COPT_SENDRECV, 1, 0, 
                                                             ByteAndBitUtility.BytesFromHex(simulatorResponseString)) )
            {
                var sendCopOutcomes = await sendCop.WaitForCopResultAsync(ct).ConfigureAwait(false);

                //in this constellation, the sendCopOutcomes.RawQueue is best empty
                foreach ( var outcome in sendCopOutcomes.RawQueue )
                {
                    switch (outcome.PduItemType)
                    {
                        case PduIt.PDU_IT_ERROR:
                           var error = ((PduEventItemError)outcome);
                           _logger.LogError("Task SendAsync - Error code: {SendCopError}", error.ErrorCodeId);
                            //_logger.LogError("Task SendAsync - Error code more specifically for the D-PDU API supplier: {SendCopErrorSpecifically}", error.ExtraErrorInfoId);
                           isOkay = false;
                           break;
                      
                        default:
                            _logger.LogError("Task SendAsync - unexpected outcome PduItemType: {SendCopUnexpectedOutcomePduItemType}", outcome.PduItemType);
                            isOkay = false;
                            //throw new ArgumentOutOfRangeException();
                            break;  
                    }
                }

            }
            return isOkay;
        }

       protected abstract string? GetResponseString(string testerRequestString);

        public abstract bool SetupCllData();
        public abstract Task Connect(CancellationToken ct);
    }
}
