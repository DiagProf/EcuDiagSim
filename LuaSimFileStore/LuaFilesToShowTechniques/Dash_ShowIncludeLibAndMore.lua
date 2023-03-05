local conv = require ("C:\\Users\\admin\\source\\repos\\EcuDiagSim\\LuaSimFileStore\\LuaLib\\Conversion")
--local conv = require ("\\..\\LuaLib\\Conversion") -- not yet working        


local DID_2203_Odo = "odo"
did = {
	["DID_0600_Coding"] = "14 0B 01 04 00",
	[DID_2203_Odo] = "05 AF",
	["22 35"] = "01",
	[0x2237] = "01",
	["F1 99"] = "19 04 13",
	["F1 98"] = "01 81 C8 F6 30 39",
}

local Language = 3

local VoltageK30 = 12000 --mV

Dash = {
	DataForComLogicalLinkCreation = {
		BusTypeShortName = "ISO_11898_2_DWCAN",
		ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
		--No DlcPinData is like ["6"] = "HI" and ["14"] = "LOW"	   
	 },
	ComParamsFromTesterPointOfView = { 
	    --only this two params is like 11-bit CANId with no extended addressing
		CP_CanPhysReqId = 0x714,
		CP_CanRespUSDTId = 0x77E,  		   
	},
	Raw = {
	    ["10 03"] = "50 03 00 28 00 C8",
		["3E 00"] = "7E 00",
		["22 F1 9E"] = "62 F1 9E 45 56 5F 4B 6F 6D 62 69 5F 55 44 53 5F 56 44 44 5F 52 4D 30 39 00",
		["22 F1 A2"] = "62 F1 A2 41 30 35 37 33 33",
		["22 F1 87"] = "62 F1 87 " .. ascii("7E0920870S "),
		["22 F1 89"] = "62 F1 89 31 31 30 34",
		["22 F1 91"] = "62 F1 91 37 45 30 39 32 30 38 37 30 53 20",
		["22 F1 A3"] = "62 F1 A3 48 30 33",
		["22 F1 AA"] = "62 F1 AA 4A 32 38 35 20",
		["22 F1 DF"] = "62 F1 DF 40",
		["22 F1 97"] = "62 F1 97 4B 4F 4D 42 49 20 20 20 20 20 20 20 20",
		["22 F1 90"] = "62 F1 90 " .. str2sbs("WVWZZZ50ZPK009944"),
		["22 F1 7C"] = "62 F1 7C 56 44 44 2D 30 32 34 30 39 2E 30 32 2E 31 37 31 39 30 31 33 36 38 39",
		["22 F1 8C"] = "62 F1 8C 30 30 30 30 30 30 30 30 30 30 30 30 30 30",
		["22 F1 E0"] = "62 F1 E0 00",
		["22 F1 A5"] = "62 F1 A5 06 24 66 2C 0A B1",
		["22 F1 7B"] = "62 F1 7B 00 00 00",
		["22 F1 87 F1 89 F1 91 F1 A3 F1 A5 F1 DF"] = "62 F1 87 37 45 30 39 32 30 38 37 30 53 20 F1 89 31 31 30 34 F1 91 37 45 30 39 32 30 38 37 30 53 20 F1 A3 48 30 33 F1 A5 06 24 66 2C 0A B1 F1 DF 40",
		["22 F1 B1"] = "7F 22 31",
		["22 F4 42"] = function(r) 
		               VoltageK30 = VoltageK30 + 100 		
					   if VoltageK30 > 15000 then
			              VoltageK30 = 9000
					   end 
					   return "62 F4 42 " .. conv:num2uInt8Sbs(VoltageK30 / 100) end, -- only a 100mV stepp
		
		
		["22 22 03"] = function(r) return "62 22 03 " .. did[DID_2203_Odo] end,
		["2E 22 03 *"] = function(request)  did[DID_2203_Odo] = request:sub(10) return "6E 22 03" end,
		
		
		["22 22 33"] = function(r) return "62 22 33 " .. conv:num2uInt8Sbs(Language) end,
		["2E 22 33 *"] = function(request) Language = conv:sbs2uInt8Num(request:sub(10) ) return "6E 22 33" end,
		
		
		["22 22 35"] = function(r) return "62 22 35 " .. did[request:sub(4)] end,
		["2E 22 35 *"] = function(request) did[request:sub(4,8)] = request:sub(10) return "6E 22 35" end,
		
		
		["22 22 37"] = function(r) return "62 22 37 " .. did[0x2237] end,
		["2E 22 37 *"] = function(request) did[0x2237] = request:sub(10) return "6E 22 37" end,
		
		["22 22 92"] = "62 22 92 03 12",

		
		
		["22 06 00"] = function(r) return "62 06 00 " .. did["DID_0600_Coding"] end,
		["2E 06 00 *"] = function(request) did["DID_0600_Coding"] = request:sub(10) return "6E 06 00" end,
		

		-- ["2E F1 98 *"] = "6E F1 98",
		-- ["2E F1 99 *"] = "6E F1 99",

		--read did
		["22 F1 9*"] = function(request)
				did_id = request:sub(4)
				value = did[did_id]
				if(value ~= nil) then
					return "62 " .. did_id .. " " .. value
				end
				return "7F 22 12"
			end,
		--write did
		["2E F1 9*"] = function(request)
				did_id = request:sub(4,8)
				value = did[did_id]
				if(value ~= nil) then
					dids[did_id] = request:sub(10)
					return "6E " .. did_id
				end
				return "7F 2E 12"
			end,
		["19 02 04"] = "59 02 99",
		["19 02 08"] = "59 02 99",
		["19 02 AE"] = "59 02 99",
		
		--my love in case of flashing
		["36 *"] = function (request) return "76 " .. request:sub(4,5) end,
    }
}