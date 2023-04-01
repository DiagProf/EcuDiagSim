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

using System.Xml.Linq;

namespace DiagEcuSim
{
    public class FlashFileReconstruction
    {
        private readonly List<FlashBlock> _blocks = new();

        public void RestartReconstruction()
        {
            _blocks.Clear();
        }

        public void AddFlashBlock()
        {
            _blocks.Add(new FlashBlock());
        }

        public bool SetEraseBlockInfos(string sbsMemoryEraseStartAddress, string sbsMemoryEraseSize)
        {
            if ( !_blocks.Any() )
            {
                return false;
            }

            _blocks.Last().SetEraseBlockInfos(
                ByteAndBitUtility.HexStringToUint32(sbsMemoryEraseStartAddress),
                ByteAndBitUtility.HexStringToUint32(sbsMemoryEraseSize));
            return true;
        }

        public bool SetFlashEraseIdentifier(string sbsFlashEraseIdentifier)
        {
            if (!_blocks.Any())
            {
                return false;
            }

            _blocks.Last().SetFlashEraseIdentifier(ByteAndBitUtility.BytesFromHexFastest(sbsFlashEraseIdentifier));
            return true;
        }

        public bool SetRequestDownloadInfos(string sbsMemoryStartAddress, string sbsMemorySize, string sbsDataFormatIdentifier = "00")
        {
            if ( !_blocks.Any() )
            {
                return false;
            }

            _blocks.Last().SetRequestDownloadInfos(
                ByteAndBitUtility.HexStringToUint32(sbsMemoryStartAddress),
                ByteAndBitUtility.HexStringToUint32(sbsMemorySize),
                ByteAndBitUtility.HexStringToUint8(sbsDataFormatIdentifier));
            return true;
        }

        public bool AddRequestPayload(string sbsRequestPayload)
        {
            if (!_blocks.Any())
            {
                return false;
            }
            _blocks.Last().AddRequestPayload(ByteAndBitUtility.BytesFromHexFastest(sbsRequestPayload));
            return true;
        }

        public bool SetCrcBytes(string sbsCrcBytes)
        {
            if (!_blocks.Any())
            {
                return false;
            }

            _blocks.Last().SetCrcBytes(ByteAndBitUtility.BytesFromHexFastest(sbsCrcBytes));
            return true;
        }

        public bool Reconstruct(string fullyQualifiedFileName)
        {
            if ( !_blocks.Any() )
            {
                return false;
            }

            var extension = $"{Path.GetExtension(fullyQualifiedFileName)}".ToLower();

            for ( var index = 0; index < _blocks.Count; index++ )
            {
                var block = _blocks[index];
                var dump = block.Dump();

                switch ( extension )
                {
                    case ".hex":
                        //ToDo
                        break;
                    case ".s19":
                        //ToDo
                        break;
                    case ".odx-f":
                        //ToDo perhaps
                        break;
                    //case ".bin":
                    default:
                        if ( !extension.Equals(".bin") )
                        {
                            fullyQualifiedFileName = $"{fullyQualifiedFileName}.bin";
                        }

                        if ( _blocks.Count > 1 )
                        {
                            fullyQualifiedFileName = $"{Path.GetFileNameWithoutExtension(fullyQualifiedFileName)}_{index}.bin";
                        }

                        using ( var binWriter = new BinaryWriter(File.Open(fullyQualifiedFileName, FileMode.Create)) )
                        {
                            binWriter.Write(dump);
                        }

                        break;
                }
            }

            return true;
        }
    }


    public class FlashBlock
    {
        private readonly List<byte> _payload = new();
        private uint _flashBlockEraseStartAddress;
        private uint _flashBlockEraseSize;
        //alternative to EraseStartAddress and EraseSize some ECUs do this
        private byte[] _flashEraseIdentifier = Array.Empty<byte>();

        private uint _flashBlockStartAddress;
        private uint _flashBlockSize;
        private byte _dataFormatIdentifier;
        private byte[] _crcBytes = Array.Empty<byte>();

        public void SetEraseBlockInfos(uint memoryEraseStartAddress, uint memoryEraseSize)
        {
            _flashBlockEraseStartAddress = memoryEraseStartAddress;
            _flashBlockEraseSize = memoryEraseSize;
        }

        public void SetFlashEraseIdentifier(byte[] flashEraseIdentifier)
        {
            _flashEraseIdentifier = flashEraseIdentifier;
        }

        public void AddRequestPayload(byte[] requestPayload)
        {
            _payload.AddRange(requestPayload);
        }

        public void SetRequestDownloadInfos(uint memoryStartAddress, uint memorySize, byte dataFormatIdentifier)
        {
            _flashBlockStartAddress = memoryStartAddress;
            _flashBlockSize = memorySize;
            _dataFormatIdentifier = dataFormatIdentifier;
        }

        public void SetCrcBytes(byte[] crcBytes)
        {
            _crcBytes = crcBytes;
        }

        public byte[] Dump()
        {
            return _payload.ToArray();
        }
    }
}
