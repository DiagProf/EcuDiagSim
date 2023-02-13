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
        private static readonly ILogger _logger = ApiLibLogging.CreateLogger<EcuDiagSimManager>();
        private readonly CancellationTokenSource _cts;
        private readonly string _dPduApiLibraryPath;
        private readonly string _vciModuleName;

        // Create a new FileSystemWatcher and set its properties.  
        private readonly FileSystemWatcher _watcher = new()
        {
            // Watch both files and subdirectories.  
            IncludeSubdirectories = true,
            // Watch for changes specified in the NotifyFilters enumeration.  
            NotifyFilter = NotifyFilters.LastWrite,
            // Watch all files.  
            Filter = "*.lua" //ToDo is upper or lower case important?
        };

        private readonly DiagPduApiOneFactory diagPduApiOneFactory = new();

        // private string _luaDirectoryPath;
        private readonly List<FileInfo> _luaFilesList;

        private DiagPduApiOneSysLevel? dPduApi;
        private List<LuaEcuDiagSimUnit>? ecuDiagSimUnits;
        private Module? vci;
        public event EventHandler EcuDiagSimManagerEventLog;

        internal EcuDiagSimManager(CancellationTokenSource cts, string luaDirectoryPath, List<FileInfo> luaFilesList, string dPduApiLibraryPath,
            string vciModuleName = "")
        {
            _cts = cts;
            _watcher.Path = luaDirectoryPath;
            _luaFilesList = luaFilesList;
            _dPduApiLibraryPath = dPduApiLibraryPath;
            _vciModuleName = vciModuleName;
        }

        protected virtual void OnEcuDiagSimManagerEventLog(EcuDiagSimManagerEventArgs e)
        {
            EcuDiagSimManagerEventLog?.Invoke(this, e);
        }

        public bool Build()
        {
            if ( !EvaluateAndBuildApiAndVci() )
            {
                return false;
            }

            if ( !EvaluateAndBuildEcuDiagSimUnits() )
            {
                return false;
            }

            if ( !CreateLuaRrTableWithComLogicalLink() )
            {
                return false;
            }


            return true;
        }

        private bool CreateLuaRrTableWithComLogicalLink()
        {
            var allGood = true;
            foreach ( var simUnit in ecuDiagSimUnits )
            {
                if ( !simUnit.BuiltRrTable(vci) )
                {
                    simUnit.Dispose();
                    ecuDiagSimUnits.Remove(simUnit);
                    allGood = false;
                }
            }

            return allGood;
        }

        private bool EvaluateAndBuildApiAndVci()
        {
            try
            {
                dPduApi = DiagPduApiOneFactory.GetApi(DiagPduApiHelper.FullLibraryPathFormApiShortName(_dPduApiLibraryPath));
                vci = dPduApi.ConnectVci(_vciModuleName);
            }
            catch ( DiagPduApiException ex )
            {
                _logger.LogCritical(ex, _dPduApiLibraryPath);
                OnEcuDiagSimManagerEventLog(new EcuDiagSimManagerEventArgs()); //ToDo Event Args
                dPduApi?.Dispose(); //Kills all VCIs too
                return false;
            }

            return true;
        }

        private bool EvaluateAndBuildEcuDiagSimUnits()
        {
            ecuDiagSimUnits = new List<LuaEcuDiagSimUnit>();
            foreach ( var luaPath in _luaFilesList )
            {
                LuaEcuDiagSimUnit? luaChunk = null;
                try
                {
                    luaChunk = new LuaEcuDiagSimUnit.Builder(luaPath).EnrichLuaWorld().CompileChunk().DoChunk().EntryPoints()
                        .Build();
                    if ( luaChunk.IsEcuDiagSimLua )
                    {
                        ecuDiagSimUnits.Add(luaChunk);
                    }
                    else
                    {
                        //This ist not a LuaEcuDiagSim Lua
                        luaChunk.Dispose();
                        _logger.LogError("LUA with name { fullFileName} is not a EcuDiagSimLua", luaPath);
                        return false;
                    }
                }
                catch ( Exception e )
                {
                    luaChunk?.Dispose();
                    _logger.LogCritical(e, "LUA with name {fullFileName} makes trouble", luaPath);
                    return false;
                }
            }

            return ecuDiagSimUnits.Any();
        }

        public async Task ConnectAsync(CancellationToken ctsToken)
        {
            var tasks = new List<Task>();
            foreach ( var simUnit in ecuDiagSimUnits )
            {
                tasks.AddRange(simUnit.Connect(ctsToken));
            }

            foreach ( var t in tasks )
            {
                t.Start();
            }

            var tt = Task.WhenAll(tasks.ToArray());
            await tt;
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
            if ( ecuDiagSimUnits != null )
            {
                foreach ( var simUnit in ecuDiagSimUnits )
                {
                    simUnit.Dispose();
                }
            }

            diagPduApiOneFactory.Dispose();
            _watcher.Dispose();
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
