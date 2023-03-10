ABS = {
	DataForComLogicalLinkCreation = {
		BusTypeShortName = "ISO_11898_2_DWCAN",
		ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
		--default is Pin 6 High and 14 Low
	 },
	ComParamsFromTesterPointOfView = { 
		--only this two params is like 11-bit CANId with no extended addressing
		CP_CanPhysReqId = 0x7E2; 
		CP_CanRespUSDTId = 0x7EA;   		   
	},
	Raw = {
		["22 F1 00"] = "62 F1 00 00 A0 8F 03",
		["22 F1 54"] = "62 F1 54 00 27",
		["19 01 0D"] = "59 01 7F 01 00 02",
		["22 F1 50"] = "62 F1 50 13 03 00",
		["22 F1 11"] = "62 F1 11 32 30 35 39 30 31 33 32 31 38",
		["22 F1 53"] = "62 F1 53 11 1C 01",
		["22 F1 55"] = "62 F1 55 00 27",
		["22 F1 51"] = "62 F1 51 14 1A 00",
		["22 F1 21"] = "62 F1 21 32 30 35 39 30 32 34 35 33 32",
		["10 03"] = "50 03 00 14 00 C8",
		["19 02 0D"] = "59 02 7F 06 2F 43 28 A3 D1 76 28 D1 C3 00 28",
		["19 06 06 2F 43 FF"] = "59 06 06 2F 43 28 01 01 05 42 05 42 01 00 02 7C A7 0A 9B 00 99 9B 92 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 1C 0D 10 02 39 05 00 00 00 FF 00 00 00 00 07 00 6D 99 B7 97 01 03 7C A7 0A 9B 00 99 9B 92 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 1C 0D 10 04 39 05 00 00 00 FF 00 00 00 00 07 00 6D 99 B7 97 01",
		["19 06 A3 D1 76 FF"] = "59 06 A3 D1 76 28 01 03 05 42 05 42 02 00 02 36 93 0B A7 00 A5 A5 9F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0C 0D 30 02 D9 05 07 04 00 FF 00 00 00 00 07 00 6E 82 80 8C 01 03 48 6E 0B A7 00 A5 A5 9F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0C 0D 30 04 D9 05 07 04 00 FF 00 00 00 00 07 00 6E 83 E5 B2 01",
		["19 06 D1 C3 00 FF"] = "59 06 D1 C3 00 28 01 03 04 13 05 42 02 00 02 00 1F 13 96 00 94 96 8D 00 00 00 00 03 FF 01 00 12 12 00 00 00 00 00 00 1D 0D 10 02 99 0F 00 00 00 FF 00 00 00 00 07 00 5E 78 8F C1 03 03 00 07 0B 95 00 93 95 8D 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 1C 0D 10 04 CD 05 00 00 00 FF 00 00 00 00 07 00 6E 7E 3D B0 03",
		["3E 00"] = "7E 00",
		["10 01"] = "50 01 00 14 00 C8",
		["22 F1 5B"] = "62 F1 5B 01 FF FF FF FF FF FF FF FF FF",
		["22 F1 0B"] = "62 F1 0B 03",
		["22 F1 03"] = "62 F1 03 32 30 35 39 30 31 33 32 31 38 00 27 42 16 01 1C 15 2A 67 A5",
		["11 01"] = "51 01",
		["22 20 02"] = "62 20 02 FF FF 00 00 00 00 00 28 00 00 72 00 00 38 00 00 25 00 00 00 00 2E 2E 2E 2C 2E 2E FF 82 00 FF 82 00 02 46 00 02 08 00 05 0C",
		["22 20 01"] = "62 20 01 00 00 00 00 00 00 00 00 AF 1F 1F 01 01 00 00 01 00",
		["22 20 15"] = "62 20 15 06",
		["22 F1 90"] = "62 F1 90 57 31 4E 32 35 33 39 38 31 31 47 30 35 32 37 31 36",
		["22 F1 A0"] = "62 F1 A0 57 31 4E 32 35 33 39 38 31 31 47 30 35 32 37 31 36",
		["22 20 50"] = "62 20 50 05 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 03 03 00 0F 00 0F 7F FF FF FF FF FF",
		["27 01"] = "67 01 0F 8A AA 2F BC EC D0 01",
		["27 02 B5 6E D4 A0"] = "67 02",
		["31 01 30 50 01"] = "71 01 30 50 01",
		["31 01 30 50 00"] = "71 01 30 50 01",
    }
}