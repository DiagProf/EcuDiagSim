FLM02_R = {
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
		CP_CanFuncReqId = 0x6F1;
		CP_CanFuncReqExtAddr = 0xEF;

		CP_CanPhysReqFormat = 0x0D;
		CP_CanPhysReqId = 0x6F1;    
		CP_CanPhysReqExtAddr = 0x44;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x644;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 26 D0",
		["22 F1 8A"] = "62 F1 8A 00 00 09",
		["22 F1 8B"] = "62 F1 8B 21 10 06",
		["22 F1 8C"] = "62 F1 8C 30 30 30 34 35 39 31 38 32 34",
		["22 F1 01"] = "62 F1 01 01 01 00 04 21 11 10 8D 00 6E 01 00 00 00 24 00 00 01 00 00 81 CB 64 01 00 06 00 00 5C FA 64 00 02 08 00 00 5C FB 66 06 00 05 00 00 43 AF 0F AD 00",
    }
}