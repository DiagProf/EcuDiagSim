USS_G05 = {
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
		CP_CanPhysReqExtAddr = 0x2C;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x62C;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 26 F0",
		["22 F1 8A"] = "62 F1 8A 00 00 08",
		["22 F1 8B"] = "62 F1 8B 21 10 28",
		["19 06 80 32 06 FF"] = "59 06 80 32 06 28 01 00 02 01",
		["19 04 80 32 06 FF"] = "59 04 80 32 06 28 01 05 17 00 00 C4 56 17 01 02 4E F5 A2 17 50 AA 17 51 01 12 02 40 05 00 EC",
		["19 06 80 32 67 FF"] = "59 06 80 32 67 28 01 00 02 05",
		["19 04 80 32 67 FF"] = "59 04 80 32 67 28 01 05 17 00 00 C4 56 17 01 02 4E F5 A5 17 50 AA 17 51 01 12 02 40 05 01 11 02 05 17 00 00 C4 81 17 01 02 4E FD 24 17 50 AA 17 51 01 12 02 40 05 01 11",
		["19 06 80 32 68 FF"] = "59 06 80 32 68 28 01 00 02 05",
		["19 04 80 32 68 FF"] = "59 04 80 32 68 28 01 05 17 00 00 C4 56 17 01 02 4E F5 A2 17 50 AA 17 51 01 12 02 40 05 01 12 02 05 17 00 00 C4 56 17 01 02 4E F6 1D 17 50 AA 17 51 01 12 02 40 05 01 12",
		["19 06 80 32 70 FF"] = "59 06 80 32 70 28 01 00 02 05",
		["19 04 80 32 70 FF"] = "59 04 80 32 70 28 01 05 17 00 00 C4 56 17 01 02 4E F5 A5 17 50 AA 17 51 01 12 02 40 05 01 14 02 05 17 00 00 C4 81 17 01 02 4E FD 24 17 50 AA 17 51 01 12 02 40 05 01 14",
		["19 06 D4 05 18 FF"] = "59 06 D4 05 18 28 01 00 02 01 03 00",
		["19 04 D4 05 18 FF"] = "59 04 D4 05 18 28 01 05 17 00 FF FF FF 17 01 00 00 00 00 17 50 33 17 51 01 00 00 40 05 00 11",
		["22 F1 8C"] = "62 F1 8C 00 00 00 00 00 00 02 02 00 01",
		["22 F1 01"] = "62 F1 01 01 01 00 04 21 01 27 8B 00 02 00 00 00 00 00 00 00 01 00 00 42 D5 64 04 01 06 00 00 5D 57 01 06 0A 08 00 00 5D 58 01 1B 02 05 00 00 43 0B 1D 01 27",
    }
}