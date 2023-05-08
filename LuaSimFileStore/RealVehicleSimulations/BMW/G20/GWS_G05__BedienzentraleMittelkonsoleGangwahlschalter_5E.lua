GWS_G05 = {
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
		CP_CanPhysReqExtAddr = 0x5E;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x65E;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 26 A0",
		["22 F1 8A"] = "62 F1 8A 00 00 21",
		["22 F1 8B"] = "62 F1 8B 21 10 27",
		["19 06 80 26 90 FF"] = "59 06 80 26 90 28 01 00 02 01 03 21",
		["19 04 80 26 90 FF"] = "59 04 80 26 90 28 01 05 17 00 00 D3 1E 17 01 02 62 AD BA 17 50 55 17 51 00 00 01 17 0C 30 0C",
		["22 F1 8C"] = "62 F1 8C 30 32 31 33 30 30 30 34 34 34",
		["22 F1 01"] = "62 F1 01 01 01 00 05 21 10 27 8B 00 21 00 00 00 00 00 00 00 02 00 00 59 39 FF FF FF 01 00 00 48 85 03 02 04 06 00 00 58 97 03 0E 00 08 00 00 58 98 03 0F 04 05 00 00 42 DA 06 01 04",
    }
}