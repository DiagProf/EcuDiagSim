
-- This example is only to show how a communication parameter must be set for 29bit CAN with normal addressing.
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
			
		-- Comment it out if you do not want the simulator to respond to functional requests.
		-- CP_CanFuncReqFormat = 0x05;   
		-- CP_CanFuncReqId = 0x7DF;      
		-- CP_CanFuncReqExtAddr = 0x00;  

		CP_CanPhysReqFormat = 0x07;   
		CP_CanPhysReqId = 0x18DA29F1;      
		CP_CanPhysReqExtAddr = 0x00;  

		CP_CanRespUSDTFormat = 0x07;  
		CP_CanRespUSDTId = 0x18DAF129;     
		CP_CanRespUSDTExtAddr = 0x00; 

		CP_CanRespUUDTFormat = 0x00;  
		CP_CanRespUUDTId = 0xFFFFFFFF;
		CP_CanRespUUDTExtAddr = 0x00; 		   
	},
	Raw = {
		["3E 00"] = "7E 00",
    }
}