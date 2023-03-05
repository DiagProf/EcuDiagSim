-- I borrowed this Lua from this project https://github.com/BugFix/Lua_SciTE/blob/d2a4bace2642044af1fbff6eac7e65938e81319e/conversion.md  Thanks a lot. 
-- And just added the last part " -- helper functions for Simulator Byte String (sbs) "
-- TIME_STAMP   2022-02-17 10:39:15   v 0.2
-- coding:utf-8

do
    conversion = {}
        --[[
        Note:
            With the exception of base10 values, all other values are used as strings.
            Conversion to base2 and to base16 can commit an optional length parameter to insert leading zeros.
            The length is limited for 32bit numbers (base2: 32, base16: 16).
            Base2 values also recognised, if they committed as number (without leading zeros) with max length of 19 characters.

        Syntax:
            load module:	module = require "ModuleName"			i.e. conv = require "conversion"
            function call:	module:FunctionName(param..)			i.e. result = conv:dec2bin(NUM)
                            or
                            module.FunctionName(module, param..)	i.e. result = conv.dec2bin(conv, NUM)
            or
                            require "conversion" 					(without variable assignment)
                            uses the name: "conversion" instead above the own variable "conv"

            universal function for all bases from 2 to 16:
                conversion.base2base(ValueToConvert, BaseForThisValue, BaseForTheResult[default:10] [,opt. length])
                default length: for base2 = the result length itself
                                for base16 = 6
                                increases automatically, if the result is longer as the given length
            predefined functions for usual base types (2, 8, 10, 12, 16):
                conversion.FUNCNAME(ValueToConvert [,opt. length])
        --]]

        ------------------------------------------------------------------------------------------------
        -- universal base to base conversion (2...16)
        -- default target is base10
        ------------------------------------------------------------------------------------------------
        conversion.base2base = function(self, _v, _basesource, _basetarget, _len)
            _basetarget = _basetarget or 10
            if _basesource == _basetarget then return _v end
            if (_basesource < 2 or _basesource > 16) or
               (_basetarget < 2 or _basetarget > 16) then return nil end
            local decsource = self:_base2dec(_v, _basesource)
            if _basetarget == 2 then
                return self:dec2bin(decsource, _len)
            elseif _basetarget == 16 then
                return self:dec2hex(decsource, _len)
            else
                return self:_dec2base(decsource, _basetarget)
            end
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- predefined functions for usual base types (2, 8, 10, 12, 16)
        ------------------------------------------------------------------------------------------------
        -- binary
        ------------------------------------------------------------------------------------------------
        conversion.bin2oct = function(self, _b)
            return self:dec2oct(self:bin2dec(_b))
        end
        conversion.bin2dec = function(self, _b)
            return self:_base2dec(_b, 2)
        end
        conversion.bin2duodec = function(self, _b)
            return self:dec2duodec(self:bin2dec(_b))
        end
        conversion.bin2hex = function(self, _b, _len)
            return self:dec2hex(self:bin2dec(_b), _len)
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- octal
        ------------------------------------------------------------------------------------------------
        conversion.oct2bin = function(self, _o, _len)
            return self:dec2bin(self:oct2dec(_o), _len)
        end
        conversion.oct2dec = function(self, _o)
            return self:_base2dec(_o, 8)
        end
        conversion.oct2duodec = function(self, _o)
            return self:dec2duodec(self:oct2dec(_o))
        end
        conversion.oct2hex = function(self, _o, _len)
            return self:dec2hex(self:oct2dec(_o), _len)
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- decimal
        ------------------------------------------------------------------------------------------------
        conversion.dec2bin = function(self, _d, _len)
            local _len = _len or 1
            if _len < 1 then _len = 1 end
            if _len > 32 then _len = 32 end
            local sRet = self:_dec2base(_d, 2)
            local retlen = #sRet
            if _len < retlen then _len = retlen end
            return ('0'):rep(_len-retlen)..sRet
        end
        conversion.dec2oct = function(self, _d)
            return self:_dec2base(_d, 8)
        end
        conversion.dec2duodec = function(self, _d)
            return self:_dec2base(_d, 12)
        end
        conversion.dec2hex = function(self, _d, _len)
            _len = _len or 6
            if _len < 1 then _len = 1 end
            if _len > 16 then _len = 16 end
            local sRet = self:_dec2base(_d, 16)
            local retlen = #sRet
            if retlen > _len then _len = retlen end
            return '0x'..('0'):rep(_len-retlen)..sRet
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- duodecimal
        ------------------------------------------------------------------------------------------------
        conversion.duodec2bin = function(self, _dd, _len)
            return self:dec2bin(self:duodec2dec(_dd), _len)
        end
        conversion.duodec2oct = function(self, _dd)
            return self:dec2oct(self:duodec2dec(_dd))
        end
        conversion.duodec2dec = function(self, _dd)
            return self:_base2dec(_dd, 12)
        end
        conversion.duodec2hex = function(self, _dd, _len)
            return self:dec2hex(self:duodec2dec(_dd), _len)
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- hexadecimal
        ------------------------------------------------------------------------------------------------
        conversion.hex2bin = function(self, _h, _len)
            return self:dec2bin(self:hex2dec(_h), _len)
        end
        conversion.hex2oct = function(self, _h)
            return self:dec2oct(self:hex2dec(_h))
        end
        conversion.hex2dec = function(self, _h)
            return self:_base2dec(_h, 16)
        end
        conversion.hex2duodec = function(self, _h)
            return self:dec2duodec(self:hex2dec(_h))
        end
        ------------------------------------------------------------------------------------------------

        ------------------------------------------------------------------------------------------------
        -- helper functions
        ------------------------------------------------------------------------------------------------
        conversion._dec2base = function(self, _d, _baseto)
            local tNumstr = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'}
            local sRet, rest = '', 0
            repeat
                rest = (_d % _baseto) +1
                sRet = tNumstr[rest]..sRet
                _d = math.floor(_d/_baseto)
            until _d == 0
            return sRet
        end
        conversion._base2dec = function(self, _v, _basefrom)
            if _basefrom == 16 then return tonumber(_v) end
            local iRet = 0
            local tNum = {['0']=0,['1']=1,['2']=2,['3']=3,['4']=4,['5']=5,['6']=6,['7']=7,['8']=8,['9']=9,['A']=10,['B']=11,['C']=12,['D']=13,['E']=14}
            if type(_v) ~= 'string' then _v = tostring(_v) end
            for i = #_v, 1, -1 do
                iRet = iRet + tNum[_v:sub(i,i)] * (_basefrom^(#_v -i))
            end
            return iRet
        end
        ------------------------------------------------------------------------------------------------
		------------------------------------------------------------------------------------------------
        -- helper functions for Simulator Byte String (sbs) 
        ------------------------------------------------------------------------------------------------
        conversion.num2uInt8Sbs = function(self, num)
		   local hexStr = self:dec2hex(num,2)
		   return hexStr[2] .. hexStr[3]
		end
		
		conversion.num2uInt16Sbs = function(self, num)
		   local hexStr = self:dec2hex(num,4)
		   return hexStr[2] .. hexStr[3] .. ' ' .. hexStr[4] .. hexStr[5]
		end
		
		conversion.num2uInt16LeSbs = function(self, num)
		   local hexStr = self:dec2hex(num,4)
		   return hexStr[4] .. hexStr[5] .. ' ' .. hexStr[2] .. hexStr[3]
		end
		
		conversion.num2uInt24Sbs = function(self, num)
		   local hexStr = self:dec2hex(num,6)
		   return hexStr[2] .. hexStr[3] .. ' ' .. hexStr[4] .. hexStr[5] .. ' ' .. hexStr[6] .. hexStr[7]
		end
		
		conversion.num2uInt32Sbs = function(self, num)
		   local hexStr = self:dec2hex(num,8)
		   return hexStr[2] .. hexStr[3] .. ' ' .. hexStr[4] .. hexStr[5] .. ' ' .. hexStr[6] .. hexStr[7] .. ' ' .. hexStr[8] .. hexStr[9]
		end
		
		conversion.num2uInt32LeSbs = function(self, num)
		   local hexStr = self:dec2hex(num,8)
		   return hexStr[8] .. hexStr[9] .. ' ' .. hexStr[6] .. hexStr[7] .. ' ' .. hexStr[4] .. hexStr[5] .. ' ' .. hexStr[2] .. hexStr[3]
		end

		conversion.sbs2uInt8Num = function(self, sbs)
			local sbsLen = #sbs
			if sbsLen < 2 then return 0 end
			return self:hex2dec('0x'.. sbs[0] .. sbs[1])
		end
		
		conversion.sbs2uInt16Num = function(self, sbs)
			local sbsLen = #sbs
			if sbsLen < 5 then return 0 end
			return self:hex2dec('0x'.. sbs[0] .. sbs[1] .. bs[3] .. sbs[4])
		end
        ------------------------------------------------------------------------------------------------

    return conversion
end