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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EcuDiagSim
{
    public class EcuDiagSimManagerFactory
    {
        private static ILogger _logger = NullLogger.Instance;

        public static EcuDiagSimManager Create(ILoggerFactory? loggerFactory, string luaFilePath, string dPduApiLibraryPath, string vciModuleName = "")
        {
            ApiLibLogging.LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = ApiLibLogging.CreateLogger<EcuDiagSimManagerFactory>();
            var (directoryPath, luaFilesList) = GetDirectoryAndFiles(luaFilePath);
            return new EcuDiagSimManager(directoryPath, luaFilesList, dPduApiLibraryPath, vciModuleName);
        }

        private static (string directoryPath, List<FileInfo> luaFilesList ) GetDirectoryAndFiles(string path)
        {
            var directory = string.Empty;
            List<FileInfo> luaFileInfos = new();
            FileInfo[] luaFiles;
            if ( File.Exists(path) )
            {
                // This path is a file
                var fileInfo = new FileInfo(path);
                directory = fileInfo.DirectoryName ?? string.Empty;
                luaFileInfos.Add(fileInfo);
            }
            else if ( Directory.Exists(path) )
            {
                // This path is a directory
                directory = path;
                var d = new DirectoryInfo(path);
                luaFileInfos = d.GetFiles("*.lua", SearchOption.AllDirectories).ToList(); //Getting lua files
            }
            else
            {
                _logger.LogError("{path} is not a valid file or directory", path);
            }

            return (directory, luaFileInfos);
        }
    }
}
