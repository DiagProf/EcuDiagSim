
dids = {
	["F1 99"] = "19 04 13",
	["F1 98"] = "01 81 C8 F6 30 39",
	["03 17"] = "00",
	["09 27"] = "01",
}

function trim(s)
	return (s:gsub("^%s*(.-)%s*$", "%1"))
end

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
	    --only this two params is like 11-bit CANId with no extended addressing
		CP_CanPhysReqId = 0x7E0; 
		CP_CanRespUSDTId = 0x7E8;   		   
	},
	Raw = {
		["3E 00"] = "7E 00",
		["10 03"] = "50 03 00 14 01 F4",
		["22 F1 9E"] = "62 F1 9E 45 56 5F 45 43 4D 32 30 54 44 49 30 33 30 30 34 4C 39 30 36 30 35 36 4B 4D 00",
		["22 F1 A2"] = "62 F1 A2 30 30 34 30 31 33",
		
		--read did
		["22 *"] = function (request)
				did_id = trim(request:sub(3))
				value = dids[did_id]
				if(value ~= nil) then
					return "62 " .. did_id .. " " .. value
				end
				return "7F 22 12"
			end,
		--write did
		["2E *"] = function (request)
			did_id = trim(request:sub(3,8))
			new_val = trim(request:sub(9))
			dids[did_id] = new_val
			return "6E " .. did_id
		end,
    }
}