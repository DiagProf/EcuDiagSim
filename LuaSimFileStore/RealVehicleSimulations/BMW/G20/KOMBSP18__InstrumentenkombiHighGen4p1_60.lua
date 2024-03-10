KOMBSP18 = {
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
		CP_CanPhysReqExtAddr = 0x60;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x660;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 27 10",
		["22 F1 01"] = "62 F1 01 01 01 00 08 99 99 99 8B 00 00 00 00 00 00 00 00 00 01 00 00 46 80 03 07 00 06 00 00 5E A5 64 03 05 08 00 00 5C 5A 64 03 05 07 00 00 5C 56 64 03 05 08 00 00 5C 58 64 03 05 08 00 00 5C 57 64 03 05 08 00 00 5C 59 64 03 05 05 00 00 45 08 06 00 06",
		["22 F1 8A"] = "62 F1 8A 00 00 08",
		["22 F1 8B"] = "62 F1 8B 21 10 26",
		["22 D1 22"] = "62 D1 22 00 00 D7 8F",
		["22 F1 8C"] = "62 F1 8C 30 30 30 33 37 32 36 32 32 37",
		["22 17 25"] = "62 17 25 BE DB 2E 0E",
		["22 D1 0D"] = "62 D1 0D 00 00 D7 8F 00 00 D7 8F",
		["31 01 10 AC"] = "71 01 10 AC 00 00 00 00 00 02 02 49 44 00 00 0E 49 50 73 65 63 2D 69 6E 74 65 72 6E 61 6C 00 00 02 0E 49 50 73 65 63 2D 69 6E 74 65 72 6E 61 6C 09 48 55 2D 4D 47 55 5F 30 31 00 0E 49 50 73 65 63 2D 69 6E 74 65 72 6E 61 6C 06 41 54 4D 2D 30 32 00 00",
		["22 D1 0B"] = "62 D1 0B",
		["22 17 01"] = "62 17 01 02 6E 33 35",
		["22 D1 0A"] = "62 D1 0A 37",
		["22 D1 12"] = "62 D1 12 61 64",
		["22 D1 1F"] = "62 D1 1F 0B 80 0F 9E 1B 1E 1A D3 01",
		["22 D1 20"] = "62 D1 20 00 00 00 00",
		["22 D1 25"] = "62 D1 25 02 AD 02 9B 02 EA",
		["22 D1 26"] = "62 D1 26 FF FD FF FD FF FD 1B 08",
		["22 D1 27"] = "62 D1 27 02 AB 1C C0 08 18 3C 0A 44 FD",
		["22 D1 2D"] = "62 D1 2D 08 CF 00 D7 8F 02 6E 2D 58 04 03 DB 00 D7 8F 02 6E 2D 58 02 02 2D 00 D7 73 02 6E 08 A7 01 08 37 00 D6 7B 02 6B D5 90 0A 08 39 00 D6 7B 02 6B D5 90 0A 03 91 00 D3 EC 02 62 FB 7E 01 01 D3 00 D3 1E 02 62 AD E2 02 00 CB 00 D2 87 02 62 73 0A 02 01 51 00 CD 11 02 59 72 77 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
		["2E D1 13 0C 09 1F 1F 03 07 E7 88"] = "7F 2E 11",
		["22 D1 13"] = "62 D1 13 0C 09 22 1F 03 07 E7 02",
		["22 10 01"] = "62 10 01 03 80 00 01 E2 64 00 33 D9 18 FF FF 64 17 70 01 E3 64 3C 33 DB 30 FF FF",
		["2E 10 01 01 03 64 1F 80 00 0F FF 0F 3F FF 00"] = "6E 10 01",
		["2E 10 01 01 64 64 1F 80 00 0F FF 0F 3F FF 00"] = "6E 10 01",
		["2E D1 13 0C 15 12 1F 03 07 E7 88"] = "7F 2E 11",
    }
}