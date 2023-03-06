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
using System.Numerics;
using System.Text;
using ISO22900.II;

namespace DiagEcuSim
{
    public static class LuaWorldEnricher
    {
        internal static Dictionary<string, ComLogicalLink> DicCoreTableToComLogicalLink = new();

        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Convert the given string into another string that represents the hex bytes with space between.
        /// Short name of result ist Simulator Byte String (sbs)
        /// This is a convenience function to use ascii strings in responses.
        /// "Hello World" -> "48 65 6C 6C 6F 20 57 6F 72 6C 64"
        /// </summary>
        /// <param name="s">Hello World</param>
        /// <returns>"48 65 6C 6C 6F 20 57 6F 72 6C 64"</returns>
        public static string Str2Sbs(string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            return BitConverter.ToString(bytes).Replace('-', ' ');
        }

        /// <summary>
        /// Convert the given Simulator Byte String (sbs) into string.
        /// "48 65 6C 6C 6F 20 57 6F 72 6C 64" -> "Hello World"
        /// </summary>
        /// <param name="s">"48 65 6C 6C 6F 20 57 6F 72 6C 64"</param>
        /// <returns>"Hello World"</returns>
        public static string Sbs2Str(string s)
        {
            return BitConverter.ToString(ByteAndBitUtility.BytesFromHexFastest(s));
        }

        public static string Num2UInt8Sbs(int number)
        {
            return ((byte)number).ToString("X2");
        }

        public static int Sbs4UInt8Num(string sbs) {
            bool success = byte.TryParse(sbs, NumberStyles.HexNumber, null as IFormatProvider, out var byteValue);
            if ( success )
            {
                return byteValue;
            }
            return 0;
        }
        

        public static string Num2UInt16Sbs(int number) {
            var s = ((ushort)number).ToString("X4");
            return s[0] + s[1] + " " + s[2] + s[3]; //fastest variant
        }




        public static void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

    }
}
