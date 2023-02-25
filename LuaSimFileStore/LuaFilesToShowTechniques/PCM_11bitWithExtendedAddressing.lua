
-- This example is only to show how a communication parameter must be set for 11bit CAN with extended addressing.
PCM = {
	DataForComLogicalLinkCreation = {
		BusTypeShortName = "ISO_11898_2_DWCAN",
		ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
		DlcPinData = {
				   ["6"] = "HI",
				   ["14"] = "LOW",
		},
	 },
	ComParamsFromTesterPointOfView = { 
		CP_CanFuncReqFormat = 0x0D;   
		CP_CanFuncReqId = 0x600;      
		CP_CanFuncReqExtAddr = 0xF1;  

		CP_CanPhysReqFormat = 0x0D;   
		CP_CanPhysReqId = 0x612;      
		CP_CanPhysReqExtAddr = 0xF1;  

		CP_CanRespUSDTFormat = 0x0D;  
		CP_CanRespUSDTId = 0x6F1;     
		CP_CanRespUSDTExtAddr = 0x12; 

		CP_CanRespUUDTFormat = 0x00;  
		CP_CanRespUUDTId = 0xFFFFFFFF;
		CP_CanRespUUDTExtAddr = 0x00; 		   
	},
	Raw = {
		["3E 00"] = "7E 00",
    }
}