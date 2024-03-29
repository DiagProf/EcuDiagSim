ACSM5 = {
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
		CP_CanPhysReqExtAddr = 0x01;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x601;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 20 40",
		["22 F1 8A"] = "62 F1 8A 00 00 96",
		["22 F1 8B"] = "62 F1 8B 21 10 31",
		["22 F1 8C"] = "62 F1 8C 32 31 33 30 34 30 31 33 34 38",
		["22 F1 01"] = "62 F1 01 01 01 00 09 21 10 31 8B 00 96 01 00 00 00 00 00 00 01 00 00 46 B2 06 04 00 06 00 00 1B 2E 06 3C 00 08 00 00 1B 2F 06 3C 04 05 00 00 1B 29 09 01 27 05 00 00 1B 2A 09 08 01 05 00 00 1B 2B 05 11 00 05 00 00 1B 2C 02 0A 26 05 00 00 1B 2D 02 0B 5E 05 00 00 2A BF 04 00 5C",
		["22 25 41"] = "62 25 41 36 45 35 44 46 44 43 43 00 00 00 00 00 00 00 00 6E 5D FD CC",
		["22 16 00"] = "62 16 00 08",
		["22 16 01"] = "62 16 01 57 30 FF FF FF FF FF FF 42 08 00 10 80 50 00 2B 50 00 7D 2D 7B BC 7A 49",
		["22 16 02"] = "62 16 02 57 20 FF FF FF FF FF FF 42 08 00 14 80 50 00 2B 50 00 7D 2D 7B BC 7A 49",
		["22 16 03"] = "62 16 03 57 38 FF FF FF FF FF FF 42 08 00 10 80 50 00 2B 50 00 7D 2D 7B BC 80 45",
		["22 16 04"] = "62 16 04 57 28 FF FF FF FF FF FF 42 08 00 14 80 50 00 2B 50 00 7D 2D 7B BC 80 45",
		["22 16 05"] = "62 16 05 57 10 FF FF FF FF FF FF 42 08 00 88 70 30 01 2B 4D 00 A8 18 90 7C C5 21",
		["22 16 06"] = "62 16 06 57 18 FF FF FF FF FF FF 42 08 00 88 70 30 01 2B 4D 00 A8 18 90 7C B5 F0",
		["22 16 07"] = "62 16 07 57 68 FF FF FF FF FF FF 42 08 00 88 70 20 01 2B 54 00 00 00 0A 1F 03 7D",
		["22 16 08"] = "62 16 08 57 70 FF FF FF FF FF FF 42 08 00 88 70 20 01 2B 54 00 00 00 8D 70 13 7D",
		["31 01 A0 DC"] = "7F 31 22",
		["22 25 02"] = "62 25 02 00 00 00 00",
		["22 40 20"] = "62 40 20 00 31 F3 5A",
    }
}