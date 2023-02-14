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

using System;
using System.Data;
using System.Text;
using ISO22900.II;
using Microsoft.Extensions.Logging;
using Neo.IronLua;

namespace EcuDiagSim
{
    internal abstract class EcuDiagSimLuaCoreTableBase
    {
        protected readonly string TableName;
        protected readonly LuaTable Table;
        protected readonly ComLogicalLink Cll;

        protected EcuDiagSimLuaCoreTableBase(string tableName, LuaTable table, ComLogicalLink cll)
        {
            TableName = tableName;
            Table = table;
            Cll = cll;
        }

        public abstract bool SetupComLogicalLink();

        public abstract Task Connect(CancellationToken ct);
    }

    internal class EcuDiagSimLuaCoreTableForIso157653OnIso157652 : EcuDiagSimLuaCoreTableBase
    {
        private static bool IsLightweightHeader(LuaTable luaTable)
        {
            var requestId = (uint?)(int?)luaTable.Members["RequestId"];
            var responseId = (uint?)(int?)luaTable.Members["ResponseId"];
            return responseId != null && requestId != null;
        }

        public new static bool IsThisClassForThisLuaTable(LuaTable luaTable)
        {
            if ( IsLightweightHeader(luaTable) )
            {
                return true;
            }

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
                if (table.Members["ProtocolShortName"] is string protocolShortName )
                {
                    return protocolShortName.Equals("ISO_15765_3_on_ISO_15765_2") || protocolShortName.Equals("ISO_14229_3_on_ISO_15765_2");
                }
            }
            return false;
        }

        public static DataForComLogicalLinkCreation GetDataForComLogicalLinkCreation(LuaTable luaTable)
        {
            DataForComLogicalLinkCreation dataSetsForCllCreation = new DataForComLogicalLinkCreation();
            //if (IsLightweightHeader(luaTable))
            //{
            //    return dataSetsForCllCreation;
            //}

            if (luaTable.Members["DataForComLogicalLinkCreation"] is LuaTable table)
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
                if ((LuaTable)table.Members["DlcPinData"] is LuaTable dlcPinData)
                {
                    Dictionary<uint, string> dlcPinDataDic = new();
                    foreach (var pair in dlcPinData)
                    {
                        var success = uint.TryParse((string)(pair.Key), out var number);
                        if (success)
                        {
                            dlcPinDataDic.Add(number, (string)(pair.Value));
                        }
                    }

                    dataSetsForCllCreation = new DataForComLogicalLinkCreation()
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName,
                        DlcPinData = dlcPinDataDic
                    };
                }
                else
                {
                    //if DlcPinData is missing 
                    dataSetsForCllCreation = new DataForComLogicalLinkCreation()
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName,
                        //DlcPinData DLC is default in this case { { 6, "HI" }, { 14, "LOW" } };
                    };
                }
            }
            return dataSetsForCllCreation;
        }

        private readonly ILogger _logger = ApiLibLogging.CreateLogger<EcuDiagSimLuaCoreTableForIso157653OnIso157652>();
        
        private DataForComLogicalLinkCreation? _cllCreationData;
        private List<UniqueRespIdentifierDataSet> _UniqueRespIdentifierDataSet = new();

        private uint CP_CanFuncReqFormat { get; set; } = 0x05;
        private uint CP_CanFuncReqId { get; set; } = 0x7DF;
        private uint CP_CanFuncReqExtAddr { get; set; } = 0x00;

        protected record UniqueRespIdentifierDataSet()
        {
            public uint CP_CanPhysReqFormat { get; set; } = 0x05;
            public uint CP_CanPhysReqId { get; set; } = 0x7E0;
            public uint CP_CanPhysReqExtAddr { get; set; } = 0x00;
            public uint CP_CanRespUSDTFormat { get; set; } = 0x05;
            public uint CP_CanRespUSDTId { get; set; } = 0x7E8;
            public uint CP_CanRespUSDTExtAddr { get; set; } = 0x00;
            public uint CP_CanRespUUDTFormat { get; set; } = 0x00;
            public uint CP_CanRespUUDTId { get; set; } = 0xFFFFFFFF;
            public uint CP_CanRespUUDTExtAddr { get; set; } = 0x00;
        }

        internal EcuDiagSimLuaCoreTableForIso157653OnIso157652(string tableName, LuaTable table, ComLogicalLink cll ) : base(tableName, table, cll)
        {
            _UniqueRespIdentifierDataSet.Insert(0, new UniqueRespIdentifierDataSet());
        }

        private bool CollectingUniqueComParams()
        {
            if ( TryToGetUniqueComParamsFromLightweightHeaderForPageIndex(0) || TryToGetUniqueComParamsForPageIndex(0) )
            {
                var cpCanPhysReqFormat = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqFormat;
                var cpCanPhysReqId = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqId;
                var cpCanPhysReqExtAddr = _UniqueRespIdentifierDataSet[0].CP_CanPhysReqExtAddr;

                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqFormat = _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTFormat;
                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqId = _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTId;
                _UniqueRespIdentifierDataSet[0].CP_CanPhysReqExtAddr = _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTExtAddr;

                _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTFormat = cpCanPhysReqFormat & 0xF;
                _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTId = cpCanPhysReqId;
                _UniqueRespIdentifierDataSet[0].CP_CanRespUSDTExtAddr = cpCanPhysReqExtAddr;

                if ( TryToGetFunctionalComParamsFromLightweightHeader() || TryToGetFunctionalComParams() )
                {
                    _UniqueRespIdentifierDataSet.Add(new UniqueRespIdentifierDataSet()
                    {
                        CP_CanRespUSDTFormat = CP_CanFuncReqFormat & 0xF,
                        CP_CanRespUSDTId = CP_CanFuncReqId,
                        CP_CanRespUSDTExtAddr = CP_CanFuncReqExtAddr
                    });
                }

                return true;
            }

            return false;
        }

        public override Task Connect(CancellationToken ct)
        {
            SetupComLogicalLink();
           return new Task(() =>
            {
                if ( Table.Members["Raw"] is LuaTable raw )
                {
                    Cll.Connect();

                    using ( var receiveCop = Cll.StartCop(PduCopt.PDU_COPT_SENDRECV, 0, -1, new byte[] {}) )
                    {
                        while ( !ct.IsCancellationRequested )
                        {
                            var result = receiveCop.WaitForCopResultAsync(ct).Result;
                            if ( result.DataMsgQueue().Count > 0 )
                            {
                                var request = string.Join(",", result.DataMsgQueue().ConvertAll(bytes => { return BitConverter.ToString(bytes); }));
                                _logger.LogInformation("ReceiveThread - Req: {request}", request);

                                request = request.Replace("-", "" ).upper();

                                object response = null;
                                foreach (var testerReq in raw)
                                {
                                    var keySuperTrimed = (((string)testerReq.Key)).Replace(" ", "").upper(); ;
                                    if (request.Equals(keySuperTrimed))
                                    {
                                        _logger.LogInformation("Req:{Key}   ->  Res:{Value}", testerReq.Key, testerReq.Value);
                                        response = testerReq.Value;
                                        break;
                                    }
                                }

                                string responseString = "";

                                if (response is string str)
                                {
                                    responseString = str;
                                }
                                else if ( response is Delegate )
                                {
                                    dynamic a = (Delegate)response;
                                    string b = a("1A 01");
                                    responseString = b;
                                }

                                _logger.LogInformation("ReceiveThread - Response:{responeStr}", responseString);



                                using ( var responseCop = Cll.StartCop(PduCopt.PDU_COPT_SENDRECV, 1, 0, StringToByteArray(responseString.Replace(" ",""))) )
                                {
                                    var resultResponse = responseCop.WaitForCopResultAsync(ct).Result;

                                    //for the information quite good... but breaks the order of how the events were fired
                                    resultResponse.PduEventItemResults().ForEach(result =>
                                    {
                                        _logger.LogInformation("ReceiveThread - SendResponse: {SendResponse}",
                                            BitConverter.ToString(result.ResultData.DataBytes));
                                    });
                                    resultResponse.PduEventItemErrors().ForEach(error =>
                                    {
                                        _logger.LogError("ReceiveThread - Error {error}", error.ErrorCodeId);
                                    });
                                    resultResponse.PduEventItemInfos().ForEach(info =>
                                    {
                                        _logger.LogInformation("ReceiveThread - Info {error}", info.InfoCode);
                                    });
                                }
                            }
                        }
                    }
                }

            }, ct, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private bool TryToGetUniqueComParamsForPageIndex(int pageIndex)
        {
            if ( Table.Members["ComParamsFromTesterPointOfView"] is not LuaTable table )
            {
                return false;
            }

            if (table.Members["CP_CanPhysReqId"] is int cpCanPhysReqId && table.Members["CP_CanRespUSDTId"] is int cpCanRespUsdtId)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqId = (uint)cpCanPhysReqId;
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUSDTId = (uint)cpCanRespUsdtId;
            }
            else
            {
                return false; //CP_CanPhysReqId and CP_CanRespUSDTId is a must have 
            }
            

            if ( table.Members["CP_CanPhysReqFormat"] is int cpCanPhysReqFormat )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqFormat = (uint)cpCanPhysReqFormat;
            }

            if (table.Members["CP_CanPhysReqExtAddr"] is int cpCanPhysReqExtAddr)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqExtAddr = (uint)cpCanPhysReqExtAddr;
            }

            if (table.Members["CP_CanRespUSDTFormat"] is int cpCanRespUsdtFormat)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUSDTFormat = (uint)cpCanRespUsdtFormat;
            }

            if (table.Members["CP_CanRespUSDTExtAddr"] is int cpCanRespUsdtExtAddr)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUSDTExtAddr = (uint)cpCanRespUsdtExtAddr;
            }

            if (table.Members["CP_CanRespUUDTFormat"] is int cpCanRespUudtFormat)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTFormat = (uint)cpCanRespUudtFormat;
            }

            if (table.Members["CP_CanRespUUDTId"] is int cpCanRespUudtId)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTId = (uint)cpCanRespUudtId;
            }

            if (table.Members["CP_CanRespUUDTExtAddr"] is int cpCanRespUudtExtAddr)
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUUDTExtAddr = (uint)cpCanRespUudtExtAddr;
            }

            return true;

        }

        private bool TryToGetFunctionalComParams()
        {
            if ( Table.Members["ComParamsFromTesterPointOfView"] is not LuaTable table )
            {
                return false;
            }

            if (table.Members["CP_CanFuncReqId"] is int cpCanFuncReqId)
            {
                CP_CanFuncReqId = (uint)cpCanFuncReqId;
            }
            else
            {
                return false; //CP_CanFuncReqId is a must have
            }
                

            if (table.Members["CP_CanFuncReqFormat"] is int cpCanFuncReqFormat)
            {
                CP_CanFuncReqFormat = (uint)cpCanFuncReqFormat;
            }

            if (table.Members["CP_CanFuncReqExtAddr"] is int cpCanFuncReqExtAddr)
            {
                CP_CanFuncReqExtAddr = (uint)cpCanFuncReqExtAddr;
            }

            return true;

        }

        private bool TryToGetUniqueComParamsFromLightweightHeaderForPageIndex(int pageIndex)
        {
            var requestId = (uint?)(int?)(Table.Members["RequestId"]);
            var responseId = (uint?)(int?)(Table.Members["ResponseId"]);

            if ( responseId != null && requestId != null )
            {
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanPhysReqId = requestId.Value;
                _UniqueRespIdentifierDataSet[pageIndex].CP_CanRespUSDTId = responseId.Value; 
            }

            return responseId != null && requestId != null;
        }

        private bool TryToGetFunctionalComParamsFromLightweightHeader()
        {
            var requestFunctionalId = (uint?)(int?)(Table.Members["RequestFunctionalId"]);
            

            if (requestFunctionalId != null)
            {
                CP_CanFuncReqId = (uint)requestFunctionalId;
            }

            return requestFunctionalId != null;
        }

        //public DataForComLogicalLinkCreation? DataForComLogicalLinkCreation()
        //{

        //    if ( IsLightweightHeader() )
        //    {
        //        //Fix value for LightweightHeader
        //        _cllCreationData = new DataForComLogicalLinkCreation();
        //        return _cllCreationData;
        //    }

        //    if ( Table.Members["DataForComLogicalLinkCreation"] is LuaTable table )
        //    {
        //        //in LUA File it looks like this
        //        //EcuName = {
        //        //    DataForComLogicalLinkCreation = {
        //        //        BusTypeShortName = "ISO_11898_2_DWCAN",
        //        //        ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
        //        //        DlcPinData = {
        //        //            ["6"] = "HI",
        //        //            ["14"] = "LOW",
        //        //        },
        //        //    },
        //        //    Raw = ....

        //        var busTypeShortName = (string)table.Members["BusTypeShortName"];
        //        var protocolShortName = (string)table.Members["ProtocolShortName"];
        //        if ( table.Members["DlcPinData"] is LuaTable dlcPinData )
        //        {
        //            Dictionary<uint, string> dlcPinDataDic = new();
        //            foreach ( var pair in dlcPinData )
        //            {
        //                var success = uint.TryParse((string)pair.Key, out var number);
        //                if ( !success )
        //                {
        //                    _logger.LogError("Attempted conversion of DlcPin '{DlcPin}' failed. Use default 0", (string)pair.Key);
        //                }

        //                dlcPinDataDic.Add(number, ((string)pair.Value).Trim());
        //            }

        //            _cllCreationData = new DataForComLogicalLinkCreation
        //            {
        //                BusTypeShortName = busTypeShortName,
        //                ProtocolShortName = protocolShortName,
        //                DlcPinData = dlcPinDataDic
        //            };
        //            return _cllCreationData;
        //        }

        //        //if DlcPinData is missing 
        //        _cllCreationData = new DataForComLogicalLinkCreation
        //        {
        //            BusTypeShortName = busTypeShortName,
        //            ProtocolShortName = protocolShortName
        //            //DlcPinData DLC is default in this case { { 6, "HI" }, { 14, "LOW" } };
        //        };
        //        return _cllCreationData;
        //    }

        //    return null;
        //}


        public override bool SetupComLogicalLink()
        {
            bool a = SetUniqueRespIdTablePageOneForSim();
            return a;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private bool SetUniqueRespIdTablePageOneForSim()
        {
            CollectingUniqueComParams();

            var ecuUniqueRespDatas = new List<PduEcuUniqueRespData>();
            //for CAN OBD 11bit
            
            foreach ( var dataSet in _UniqueRespIdentifierDataSet)
               
            {
                ecuUniqueRespDatas.Add(new PduEcuUniqueRespData(uniqueRespIdentifier: dataSet.CP_CanRespUSDTId,   //<- this is the UniqueRespIdentifier
                    new List<PduComParam>
                    {
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqFormat", dataSet.CP_CanPhysReqFormat, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqId", dataSet.CP_CanPhysReqId, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanPhysReqExtAddr", dataSet.CP_CanPhysReqExtAddr, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        
                        DiagPduApiComParamFactory.Create("CP_CanRespUSDTFormat", dataSet.CP_CanRespUSDTFormat, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUSDTId", dataSet.CP_CanRespUSDTId, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUSDTExtAddr", dataSet.CP_CanRespUSDTExtAddr, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),

                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTFormat", dataSet.CP_CanRespUUDTFormat, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTId", dataSet.CP_CanRespUUDTId, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                        DiagPduApiComParamFactory.Create("CP_CanRespUUDTExtAddr", dataSet.CP_CanRespUUDTExtAddr, PduPt.PDU_PT_UNUM32, PduPc.PDU_PC_UNIQUE_ID),
                    }
                ));
            }
           Cll.SetUniqueRespIdTable(ecuUniqueRespDatas);


            return true;
        }

    }
}
