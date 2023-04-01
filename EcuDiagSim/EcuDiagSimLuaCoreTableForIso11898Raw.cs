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
    internal class EcuDiagSimLuaCoreTableForIso11898Raw : AbstractEcuDiagSimLuaCoreTableWithRaw
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<EcuDiagSimLuaCoreTableForIso11898Raw>();

        private readonly List<UniqueRespIdentifierDataSet> _UniqueRespIdentifierDataSet = new();

        internal EcuDiagSimLuaCoreTableForIso11898Raw(LuaEcuDiagSimUnit luaEcuDiagSimUnit, string tableName,
            ComLogicalLink cll) : base(luaEcuDiagSimUnit, tableName, cll)
        {
            _UniqueRespIdentifierDataSet.Insert(0, new UniqueRespIdentifierDataSet());
        }

        public static bool IsThisClassForThisLuaTable(LuaTable luaTable)
        {
            if ( luaTable.Members["DataForComLogicalLinkCreation"] is LuaTable table )
            {
                //in LUA File it looks e.g. like this
                //EcuName = {
                //    DataForComLogicalLinkCreation = {
                //        BusTypeShortName = "ISO_11898_2_DWCAN",
                //        ProtocolShortName = "ISO_11898_RAW",
                //        DlcPinData = {
                //            ["6"] = "HI",
                //            ["14"] = "LOW",
                //        },
                //    },
                //    Raw = ....
                if ( table.Members["ProtocolShortName"] is string protocolShortName )
                {
                    return protocolShortName.Equals("ISO_11898_RAW");
                }
            }

            return false;
        }

        private bool CollectingUniqueComParams()
        {
            if ( TryToGetUniqueComParamsForPageIndex(0) )
            {
                var cpCanPhysReqFormat = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqFormat;
                var cpCanPhysReqId = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqId;
                var cpCanPhysReqExtAddr = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqExtAddr;

                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqFormat = _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTFormat;
                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqId = _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTId;
                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqExtAddr = _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTExtAddr;

                _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTFormat = cpCanPhysReqFormat & 0xF;
                _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTId = cpCanPhysReqId;
                _UniqueRespIdentifierDataSet[0].CP_CanRespUUDTExtAddr = cpCanPhysReqExtAddr;

                return true;
            }

            return false;
        }

        public override async Task Connect(CancellationToken ct)
        {
            Cll.Connect();

            using ( var receiveCop = Cll.StartCop(PduCopt.PDU_COPT_SENDRECV, 0, -1, new byte[] {}) )
            {
                while ( !ct.IsCancellationRequested )
                {
                    var result = await receiveCop.WaitForCopResultAsync(ct).ConfigureAwait(false);
                    if ( result.DataMsgQueue().Count > 0 )
                    {
                        var testerRequestString = ByteAndBitUtility.BytesToHexString(result.DataMsgQueue().First().ToArray(), true);
                        _logger.LogInformation("Table: {TableName} < {testerRequest}", TableName, testerRequestString);


                        while ( _luaEcuDiagSimUnit.ResourceBusy == 1 )
                        {
                            _logger.LogInformation("Table: {TableName} , Simulator really busy", TableName);
                            await Task.Delay(10, ct).ConfigureAwait(false);
                        }

                        var simulatorResponseString = GetResponseString(testerRequestString);
                        if ( String.IsNullOrWhiteSpace(simulatorResponseString) )
                        {
                            //no entry found in lua.... nothing matched
                            //we are doing nothing
                            continue;
                        }
                        _logger.LogInformation("Table: {TableName} > {responseString}", TableName, simulatorResponseString);
                        await SendAsync(simulatorResponseString, ct).ConfigureAwait(false);
                    }
                }
            }

            Cll.Disconnect();
        }


        private bool TryToGetUniqueComParamsForPageIndex(int pageIndex)
        {
            if ( Table.Members["ComParamsFromTesterPointOfView"] is not LuaTable table )
            {
                return false;
            }

            if ( table.Members["CP_CanPhysReqId"] is int cpCanPhysReqId && table.Members["CP_CanRespUUDTId"] is int cpCanRespUudtId)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqId = (uint)cpCanPhysReqId;
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTId = (uint)cpCanRespUudtId;
            }
            else
            {
                return false; //CP_CanPhysReqId and CP_CanRespUUDTId is a must have 
            }


            if ( table.Members["CP_CanPhysReqFormat"] is int cpCanPhysReqFormat )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqFormat = (uint)cpCanPhysReqFormat;
            }

            if ( table.Members["CP_CanPhysReqExtAddr"] is int cpCanPhysReqExtAddr )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqExtAddr = (uint)cpCanPhysReqExtAddr;
            }


            if ( table.Members["CP_CanRespUUDTFormat"] is int cpCanRespUudtFormat )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTFormat = (uint)cpCanRespUudtFormat;
            }

            if ( table.Members["CP_CanRespUUDTExtAddr"] is int cpCanRespUudtExtAddr )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTExtAddr = (uint)cpCanRespUudtExtAddr;
            }

            return true;
        }


        public override bool SetupCllData()
        {
            if ( !SetupAdditionalLuaFunctions() )
            {
                return false;
            }

            if ( !CollectingUniqueComParams() )
            {
                return false;
            }

            if ( !SetUniqueRespIdTablePageOneForSim() )
            {
                return false;
            }

            return true;
        }

        private bool SetUniqueRespIdTablePageOneForSim()
        {
            var ecuUniqueRespDataPages = new List<PduEcuUniqueRespData>();

            foreach ( var dataSet in _UniqueRespIdentifierDataSet )
            {
                ecuUniqueRespDataPages.Add(new PduEcuUniqueRespData(dataSet.CP_CanRespUUDTId, //<- this is the UniqueRespIdentifier
                    new List<PduComParam>
                    {
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqFormat", dataSet.CP_CanPhysReqFormat, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqId", dataSet.CP_CanPhysReqId, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqExtAddr", dataSet.CP_CanPhysReqExtAddr, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),

                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTFormat", dataSet.CP_CanRespUUDTFormat, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTId", dataSet.CP_CanRespUUDTId, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTExtAddr", dataSet.CP_CanRespUUDTExtAddr, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID)
                    }
                ));
            }

            try
            {
                Cll.SetUniqueRespIdTable(ecuUniqueRespDataPages);
            }
            catch ( DiagPduApiException e )
            {
                _logger.LogCritical(e, "Can't set UniqueRespIdTable");
                return false;
            }

            return true;
        }

        protected record UniqueRespIdentifierDataSet
        {
            public uint CP_CanPhysReqExtAddr { get; set; }
            public uint CP_CanPhysReqFormat { get; set; } 
            public uint CP_CanPhysReqId { get; set; } = 0x7E0;

            public uint CP_CanRespUUDTExtAddr { get; set; }
            public uint CP_CanRespUUDTFormat { get; set; } = 0x30;
            public uint CP_CanRespUUDTId { get; set; } = 0x7E8;
        }
    }
}
