KAFAS04 = {
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
		CP_CanPhysReqExtAddr = 0x5D;

		CP_CanRespUSDTFormat = 0x0D;
		CP_CanRespUSDTId = 0x65D;
		CP_CanRespUSDTExtAddr = 0xF1;
	},
	Raw = {
		["22 F1 50"] = "62 F1 50 0F 24 F0",
		["22 F1 8A"] = "62 F1 8A 00 00 42",
		["22 F1 8B"] = "62 F1 8B 21 10 18",
		["19 06 80 0A BF FF"] = "59 06 80 0A BF 28 01 00 02 C8",
		["19 04 80 0A BF FF"] = "59 04 80 0A BF 28 01 25 17 50 AA 17 51 01 12 03 17 00 00 B5 03 6F FB 05 6F 09 2E 17 01 02 23 32 7F 6F 03 49 45 AA 00 45 AC 01 40 15 02 45 AF 00 00 00 00 45 B1 00 00 00 00 45 B3 FF FF FF FF 45 BD 00 00 00 00 45 AB 00 00 00 10 45 AD 00 00 00 00 45 C0 00 00 00 00 45 BB FF FF FF FF 45 B5 00 00 00 00 45 B7 FF FF FF FF 45 B9 00 00 00 00 45 AE 01 45 B4 01 45 B0 01 45 B2 FF 45 BF 01 45 BC 01 45 BA FF 45 B6 FF 45 B8 01 40 19 5E 40 17 4B 40 16 03 6F 02 94 40 18 00 6F 00 0A 6F 07 05 B4 02 25 17 50 AA 17 51 01 12 03 17 00 00 D7 8E 6F FB 05 6F 09 52 17 01 02 6E 2B 86 6F 03 55 45 AA 00 45 AC 01 40 15 01 45 AF 00 00 00 00 45 B1 00 00 00 00 45 B3 FF FF FF FF 45 BD 00 00 00 00 45 AB 00 00 00 08 45 AD 00 00 00 00 45 C0 00 00 00 00 45 BB FF FF FF FF 45 B5 00 00 00 00 45 B7 FF FF FF FF 45 B9 00 00 00 00 45 AE 01 45 B4 01 45 B0 01 45 B2 FF 45 BF 01 45 BC 01 45 BA FF 45 B6 FF 45 B8 01 40 19 61 40 17 19 40 16 05 6F 02 94 40 18 00 6F 00 0A 6F 07 06 04",
		["19 06 80 0A BE FF"] = "59 06 80 0A BE 28 01 00 02 03",
		["19 04 80 0A BE FF"] = "59 04 80 0A BE 28 01 25 17 50 AA 17 51 01 12 03 17 00 00 C0 BF 6F FB 05 6F 09 4C 17 01 02 3F 1B DE 6F 03 5F 45 AA 00 45 AC 01 40 15 01 45 AF 00 00 00 00 45 B1 00 00 00 00 45 B3 FF FF FF FF 45 BD 00 00 00 00 45 AB 00 00 80 00 45 AD 00 00 00 00 45 C0 00 00 00 00 45 BB FF FF FF FF 45 B5 00 00 00 00 45 B7 FF FF FF FF 45 B9 00 00 00 00 45 AE 01 45 B4 01 45 B0 01 45 B2 FF 45 BF 01 45 BC 01 45 BA FF 45 B6 FF 45 B8 01 40 19 60 40 17 4B 40 16 65 6F 02 94 40 18 01 6F 00 0A 6F 07 08 CA 02 25 17 50 AA 17 51 01 12 02 17 00 00 C8 5B 6F FB 05 6F 09 0C 17 01 02 52 BD 71 6F 03 5B 45 AA 00 45 AC 00 40 15 02 45 AF 00 00 00 00 45 B1 00 00 00 04 45 B3 FF FF FF FF 45 BD 00 00 00 04 45 AB 00 00 80 0C 45 AD 00 00 00 04 45 C0 00 00 00 00 45 BB FF FF FF FF 45 B5 00 00 00 00 45 B7 FF FF FF FF 45 B9 00 00 00 00 45 AE 01 45 B4 01 45 B0 00 45 B2 FF 45 BF 01 45 BC 00 45 BA FF 45 B6 FF 45 B8 01 40 19 51 40 17 00 40 16 05 6F 02 95 40 18 01 6F 00 0A 6F 07 09 38",
		["22 F1 8C"] = "62 F1 8C 32 31 32 39 31 34 32 38 33 34",
		["22 F1 01"] = "62 F1 01 01 01 00 07 21 07 01 8B 00 42 01 00 00 00 10 00 00 01 00 00 49 0E 03 01 03 06 00 00 49 08 04 14 31 08 00 00 49 09 7D 28 0A 08 00 00 49 0A 7D 28 0A 08 00 00 49 0B 7D 28 00 05 00 00 41 01 0C 04 03 05 00 00 41 02 0D 05 04",
		["2E D3 BD 01"] = "6E D3 BD",
		["11 01"] = "51 01",
    }
}