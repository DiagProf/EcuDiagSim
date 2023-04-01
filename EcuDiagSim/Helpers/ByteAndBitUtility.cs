#region License

// // MIT License
// //
// // Copyright (c) 2023 Joerg Frank
// // http://www.diagprof.com/
// //
// // Permission is hereby granted, free of charge, to any person obtaining a copy
// // of this software and associated documentation files (the "Software"), to deal
// // in the Software without restriction, including without limitation the rights
// // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// // copies of the Software, and to permit persons to whom the Software is
// // furnished to do so, subject to the following conditions:
// //
// // The above copyright notice and this permission notice shall be included in all
// // copies or substantial portions of the Software.
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// // SOFTWARE.

#endregion

using System.Globalization;
using System.Text;

namespace DiagEcuSim
{
    /// <summary>
    /// Utilities for bit and byte operations.
    /// (Frequently copied-and-pasted across my projects)
    /// </summary>
    public static class ByteAndBitUtility
    {
        /// <summary>
        /// Converts an array of bytes into a printable hex-string
        /// </summary>
        /// <param name="hexString">Input hex-string to convert into a byte array</param>
        /// <returns>Byte array based on the input hex-string</returns>
        public static byte[] BytesFromHex(string hexString)
        {
            return StringToByteArray(hexString.Replace(" ", ""));
        }
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }


        /// <summary>
        /// Converts an array of bytes into a printable hex-string
        /// </summary>
        /// <param name="hexString">Input hex-string to convert into a byte array</param>
        /// <returns>Byte array based on the input hex-string</returns>
        public static byte[] BytesFromHexFastest(string hexString)
        {
            return StringToByteArrayFastest(hexString.Replace(" ", ""));
        }

        private static byte[] StringToByteArrayFastest(string hex)
        {
            // Internally used by StringToByteArrayFastest
            static int GetHexValue(char hex)
            {
                var val = (int)hex;
                return val - (val < 58 ? 48 : 55);
            }

            // see https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
            if ( hex.Length % 2 == 1 )
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }

            var arr = new byte[hex.Length >> 1];
            for ( var i = 0; i < hex.Length >> 1; ++i )
            {
                arr[i] = (byte)((GetHexValue(hex[i << 1]) << 4) + (GetHexValue(hex[(i << 1) + 1])));
            }

            return arr;
        }


        /// <summary>
        /// Converts an array of bytes into its hex-string equivalent
        /// </summary>
        /// <param name="inBytes">Input byte array</param>
        /// <param name="spacedOut">Option to add spaces between individual bytes</param>
        /// <returns>Hex-string based on the input byte array</returns>
        public static string BytesToHexString(byte[] inBytes, bool spacedOut = false)
        {
            return BitConverter.ToString(inBytes).Replace("-", spacedOut ? " " : "");
        }


        public static string BytesToDecimalString(byte[] inArray)
        {
            var sb = new StringBuilder();
            foreach (byte b in inArray)
            {
                sb.Append(Convert.ToString(b, toBase: 10).PadLeft(3, '0') + " ");
            }
            return sb.ToString().TrimEnd();
        }


        /// <summary>
        /// eg. 0x4711080F or 0x47 11 080F or 4711 080F   to   1192298511dez inside uint
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static uint HexStringToUint32(string hex)
        {
            bool success = uint.TryParse(hex.Replace(" ",""), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier, null as IFormatProvider, out var byteValue);
            if (success)
            {
                return byteValue;
            }
            return 0;
        }

        /// <summary>
        /// eg. 0x0F or 0F t0 15dez  inside uint
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte HexStringToUint8(string hex)
        {
            bool success = byte.TryParse(hex, NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier, null as IFormatProvider, out var byteValue);
            if (success)
            {
                return byteValue;
            }
            return 0;
        }
    }
}
