EPS_G01 = {
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
		CP_CanPhysReqExtAddr = 0x30;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x630;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 24 00",
		["22 F1 8A"] = "62 F1 8A 00 00 BF",
		["22 F1 8B"] = "62 F1 8B 21 08 19",
		["19 06 48 23 F9 FF"] = "59 06 48 23 F9 28 01 00 02 01",
		["19 04 48 23 F9 FF"] = "59 04 48 23 F9 28 01 18 17 00 00 BA A7 17 50 AA 28 05 09 28 66 B8 28 67 00 29 50 02 40 09 2F 40 0A 76 50 01 3D 50 02 73 50 06 01 50 07 80 20 B4 8D 50 08 70 50 0E 00 00 50 14 01 50 16 13 64 50 18 FF B5 50 19 00 00 50 1A E4 50 1B 03 50 40 00 FD 12 00 AF FD 14 00 17 01 02 2F A7 EF",
		["22 F1 8C"] = "62 F1 8C 33 35 30 34 30 38 31 38 30 31",
		["22 F1 01"] = "62 F1 01 01 01 00 08 21 11 10 8D 00 6E 01 00 00 00 24 00 00 01 00 00 49 61 04 04 06 01 00 00 45 0A 04 00 00 02 00 00 33 CD FF FF FF 06 00 00 4E 96 0A 0A 08 08 00 00 4E 97 0A BE 00 08 00 00 58 71 01 C3 00 0D 00 00 58 78 01 C3 00 05 00 00 33 C9 15 04 02",
		["22 25 41"] = "62 25 41 31 46 33 32 30 37 42 34 00 00 00 00 00 00 00 00 1F 32 07 B4",
    }
}