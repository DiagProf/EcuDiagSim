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

using System;
using EcuDiagSim.App.Interfaces;

namespace EcuDiagSim.App.Services
{
    internal class PathService : IPathService
    {
        private readonly ISettingsService _settingsService;

        protected string LuaWorkingDirectorySettingsKey { get; set; } = "LuaWorkingDirectory";

        protected string SettingsName { get; set; }

        public PathService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }


        public string? LoadLuaWorkingDirectory()
        {
            if ( _settingsService.TryLoad(SettingsName, LuaWorkingDirectorySettingsKey, out string? workingDirectory) is true )
            {
                return workingDirectory;
            }

            return null;
        }

        public bool SaveLuaWorkingDirectory(string workingDirectory)
        {
            if ( workingDirectory == null )
            {
                throw new ArgumentNullException(nameof(workingDirectory));
            }

            return _settingsService.TrySave(SettingsName, LuaWorkingDirectorySettingsKey, workingDirectory);
        }
    }
}
