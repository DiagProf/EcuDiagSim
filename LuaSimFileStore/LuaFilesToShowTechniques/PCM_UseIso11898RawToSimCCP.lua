PCM = {
	DataForComLogicalLinkCreation = {
		BusTypeShortName = "ISO_11898_2_DWCAN",
		ProtocolShortName = "ISO_11898_RAW",
		--default is Pin 6 High and 14 Low
	 },
	ComParamsFromTesterPointOfView = { 		
		CP_CanPhysReqId = 0x7F0; 
		CP_CanRespUUDTId = 0x7F8;   		   
	},
	Raw = {
	    --that's not a CCP that's something else... I'll change that soon
		["02 10 03*"] = "02 50 03",
		["01 27 02*"] = "00 00 00 00 01 67 06",
		["FF FF FF FF 02 27 06*"] = "34 03 67 03",
		["00 00 00 01 01 31 0C 10"] = "79 05 00 00 00 00 30",
		["7A 05 00 00 00 00 00 21"] = function(r) PCM.sendRaw("00 01 01 71 04") return "7A 05 00 00 00 00 30" end,
    }
}