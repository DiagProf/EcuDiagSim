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


using ISO22900.II;
using Microsoft.Extensions.Logging;

namespace EcuDiagSim
{
    public class EcuDiagSimManagerEventArgs : EventArgs
    {
        public int Threshold { get; set; }
        public DateTime TimeReached { get; set; }
    }


    public class EcuDiagSimManager : IDisposable
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<EcuDiagSimManager>();

        private readonly DiagPduApiOneFactory _diagPduApiOneFactory = new();
        private readonly string _dPduApiLibraryPath;

        private readonly List<FileInfo> _luaFilesList;
        private readonly string _vciModuleName;

        // Create a new FileSystemWatcher and set its properties.  
        private readonly FileSystemWatcher _fileSysWatcher = new()
        {
            // Watch both files and subdirectories.  
            IncludeSubdirectories = true,
            // Watch for changes specified in the NotifyFilters enumeration.  
            NotifyFilter = NotifyFilters.LastWrite,
            // Watch all files.  
            Filter = "*.lua" //ToDo is upper or lower case important?
        };

        private DiagPduApiOneSysLevel? _dPduApi;
        private readonly List<LuaEcuDiagSimUnit> _ecuDiagSimUnits = new();
        private Module? _vci;
        public event EventHandler? EcuDiagSimManagerEventLog;

        internal EcuDiagSimManager(string luaDirectoryPath, List<FileInfo> luaFilesList, string dPduApiLibraryPath,
            string vciModuleName = "")
        {
            _fileSysWatcher.Path = luaDirectoryPath;
            _luaFilesList = luaFilesList;
            _dPduApiLibraryPath = dPduApiLibraryPath;
            _vciModuleName = vciModuleName;
        }

        protected virtual void OnEcuDiagSimManagerEventLog(EcuDiagSimManagerEventArgs e)
        {
            EcuDiagSimManagerEventLog?.Invoke(this, e);
        }

        /// <summary>
        ///     Evaluate and Build Simulation Environment
        ///     Evaluate if the Lua-Files are okay and the VCI can do the job and later again the same to build is relative time
        ///     consuming.
        ///     That's why i evaluate and build in one step but of course in individual stages
        /// </summary>
        /// <returns></returns>
        public bool EvaluateAndBuildSimEnv()
        {
            if ( !EvaluateAndBuildApiAndVci() )
            {
                return false;
            }

            if ( !EvaluateAndBuildEcuDiagSimUnits() )
            {
                return false;
            }

            if ( !EvaluateAndPairingCoreTableWithComLogicalLink() )
            {
                return false;
            }

            if ( !SetupCllData() )
            {
                return false;
            }

            RegistrationEcuDiagSimUnitsOnWatcherEvent();
            _fileSysWatcher.EnableRaisingEvents = true;
            return true;
        }


        private bool EvaluateAndPairingCoreTableWithComLogicalLink()
        {
            var allGood = true;
            foreach ( var simUnit in _ecuDiagSimUnits )
            {
                if ( !simUnit.EvaluateAndPairingCoreTableWithComLogicalLink(_vci) )
                {
                    simUnit.Dispose();
                    _ecuDiagSimUnits.Remove(simUnit);
                    allGood = false;
                }
            }

            return allGood;
        }

        private bool EvaluateAndBuildApiAndVci()
        {
            try
            {
                _dPduApi = DiagPduApiOneFactory.GetApi(DiagPduApiHelper.FullLibraryPathFormApiShortName(_dPduApiLibraryPath));
                _vci = _dPduApi.ConnectVci(_vciModuleName);
            }
            catch ( DiagPduApiException ex )
            {
                _logger.LogCritical(ex, _dPduApiLibraryPath);
                OnEcuDiagSimManagerEventLog(new EcuDiagSimManagerEventArgs()); //ToDo Event Args
                _dPduApi?.Dispose(); //Kills all VCIs too, so no need of vci.Dispose()
                return false;
            }

            return true;
        }

        private bool EvaluateAndBuildEcuDiagSimUnits()
        {
            _ecuDiagSimUnits.Clear();
            foreach ( var luaPath in _luaFilesList )
            {
                LuaEcuDiagSimUnit? luaChunk = null;
                try
                {
                    luaChunk = new LuaEcuDiagSimUnit.Builder(luaPath)
                        .EnrichLuaWorld()
                        .CompileChunk()
                        .DoChunk()
                        .EntryPoints()
                        .Build();
                    if ( luaChunk.IsEcuDiagSimLua )
                    {
                        _ecuDiagSimUnits.Add(luaChunk);
                    }
                    else
                    {
                        //This ist not a LuaEcuDiagSim Lua
                        luaChunk.Dispose();
                        _logger.LogError("LUA with name {LuaFilePath} is not a EcuDiagSimLua", luaPath);
                        return false;
                    }
                }
                catch ( Exception e )
                {
                    luaChunk?.Dispose();
                    _logger.LogCritical(e, "LUA with name {LuaFilePath} makes trouble", luaPath);
                    return false;
                }
            }

            return _ecuDiagSimUnits.Any();
        }


        private bool SetupCllData()
        {
            var allGood = true;
            foreach ( var simUnit in _ecuDiagSimUnits )
            {
                if ( !simUnit.SetupCllData() )
                {
                    simUnit.Dispose();
                    _ecuDiagSimUnits.Remove(simUnit);
                    allGood = false;
                }
            }

            return allGood;
        }

        public void RegistrationEcuDiagSimUnitsOnWatcherEvent()
        {
            foreach ( var simUnit in _ecuDiagSimUnits )
            {
                _fileSysWatcher.Changed += simUnit.FileChanged;
            }
        }

        public async Task ConnectAndRunAsync(CancellationToken ctsToken)
        {
            var tasks = new List<Task>();
            foreach ( var simUnit in _ecuDiagSimUnits )
            {
                tasks.AddRange(simUnit.Connect(ctsToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private void ReleaseUnmanagedResources()
        {
            foreach ( var simUnit in _ecuDiagSimUnits )
            {
                //UnRegistrationEcuDiagSimUnitsOnWatcherEvent
                _fileSysWatcher.Changed -= simUnit.FileChanged;
                simUnit.Dispose();
            }

            _diagPduApiOneFactory.Dispose();
            _fileSysWatcher.Dispose();
        }


        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~EcuDiagSimManager()
        {
            ReleaseUnmanagedResources();
        }
    }
}
