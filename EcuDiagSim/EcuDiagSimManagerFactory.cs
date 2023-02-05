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

using System.Runtime.InteropServices;
using Neo.IronLua;
using DiagEcuSim;
using System.Diagnostics;


using ISO22900.II;
using System.Collections.Generic;
//using Microsoft.Extensions.Hosting;

namespace EcuDiagSim
{
    //https://codeopinion.com/cap-event-bus-outbox-pattern/
    //https://github.com/dotnetcore/CAP/issues/275
    public class EcuDiagSimManagerFactory : IDisposable
    {
        private static readonly Dictionary<string, EcuDiagSimManager> Cache = new();
        private static ILogger _logger = NullLogger.Instance;


        //public static Module GetVci(string dPduApiLibraryPath, ILoggerFactory loggerFactory, string optionStr, ApiModifications apiModFlags,
        //    string vciModuleName = "")
        //{
        //    return GetApi(dPduApiLibraryPath, loggerFactory, optionStr, apiModFlags).ConnectVci(vciModuleName);
        //}

        public static async Task<EcuDiagSimManager> Create(ILoggerFactory loggerFactory, CancellationTokenSource cts, string luaFilePath, string dPduApiLibraryPath,
            string vciModuleName = "" )
        {
            ApiLibLogging.LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = ApiLibLogging.CreateLogger<EcuDiagSimManagerFactory>();

            using ( var factory = new DiagPduApiOneFactory() )
            {
                var api = DiagPduApiOneFactory.GetApi(DiagPduApiHelper.FullLibraryPathFormApiShortName(dPduApiLibraryPath));
                var vci = api.ConnectVci(vciModuleName);

                var (directoryPath, luaFilesList) = GetDirectoryAndFiles(luaFilePath);

                // Create a new FileSystemWatcher and set its properties.  
                FileSystemWatcher watcher = new();
                watcher.Path = directoryPath;
                // Watch both files and subdirectories.  
                watcher.IncludeSubdirectories = true;
                // Watch for changes specified in the NotifyFilters enumeration.  
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                // Watch all files.  
                watcher.Filter = "*.lua"; //ToDo is upper or lower case important?

                List<LuaEcuDiagSimUnit> simUnits = new List<LuaEcuDiagSimUnit>();
                var director = new LuaEcuDiagSimUnitBuildDirector();
                foreach ( var luaPath in luaFilesList )
                {
                    var builder = new LuaEcuDiagSimDefaultUnitBuilder(watcher, luaPath,vci);
                    director.Construct(builder);
                    simUnits.Add(builder.GetResult());
                }
                

                var tasks = new List<Task>();
                foreach ( var simUnit in simUnits)
                {
                   tasks.AddRange(simUnit.Connect(cts.Token));
                }

                foreach ( var t in tasks )
                {
                    t.Start();
                }

                Task tt = Task.WhenAll(tasks.ToArray());
                await tt;

            }

            return new EcuDiagSimManager();
        }

        private static (string directoryPath, List<FileInfo> luaFilesList ) GetDirectoryAndFiles(string path)
        {
            string directory = string.Empty;
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

        #region DisposeBehavior

        public void Dispose()
        {
            foreach (var sys in Cache.Values.ToArray())
            {
                sys.Dispose();
            }
        }

        #endregion
    }
    //https://www.dofactory.com/net/builder-design-pattern

    class LuaEcuDiagSimDefaultUnitBuilder : LuaEcuDiagSimUnitBuilder
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly FileInfo _fullLuaFileName;
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<LuaEcuDiagSimDefaultUnitBuilder>();
        private readonly Module _vci;
        private LuaEcuDiagSimUnit _luaEcuDiagSimUnit = new LuaEcuDiagSimUnit();

        public LuaEcuDiagSimDefaultUnitBuilder(FileSystemWatcher fileSystemWatcher, FileInfo fullLuaFileName, Module module) : base()
        {
            _fileSystemWatcher = fileSystemWatcher;
            _fullLuaFileName = fullLuaFileName;
            _vci = module;
        }
        public override void JoinLuaUnitWithFileName()
        {
            _luaEcuDiagSimUnit.FullLuaFileName = _fullLuaFileName;
            _fileSystemWatcher.Changed += _luaEcuDiagSimUnit.FileChanged;
        }

        public override void EnrichLuaWorld()
        {
            _luaEcuDiagSimUnit.Environment["ascii"] = new Func<string, string>(LuaWorldEnricher.Ascii);
        }

        public override void CompileChunk()
        {
            var buildCompileChunk = _luaEcuDiagSimUnit.BuildCompileChunk;
        }

        public override void DoChunk()
        {
            var buildDoChunk = _luaEcuDiagSimUnit.BuildDoChunk;
        }

        public override void IsLuaFileContentsEcuDiagSimLua()
        {
            var hasEntryPoint = _luaEcuDiagSimUnit.NumberOfEntryPoints;
        }

        public override void CreateComLogicalLink()
        {
            var counter = _luaEcuDiagSimUnit.NumberOfEntryPoints;
            if ( counter > 0 )
            {
                for ( var i = 0; i < counter; i++ )
                {
                    var cllCreationData = _luaEcuDiagSimUnit.GetRequiredDataForComLogicalLinkCreationByIndex(i);
                    if ( cllCreationData is not null )
                    {
                        var resourceIds = _vci.GetResourceIds(cllCreationData.BusTypeShortName, cllCreationData.ProtocolShortName,
                            cllCreationData.DlcPinData.ToList());
                        if ( resourceIds.Any() )
                        {
                            if ( EcuDiagSimLuaCoreTableForIso157653OnIso157652.IsProtocolSuitableForThisCoreTable(cllCreationData.ProtocolShortName) )
                            {
                                _luaEcuDiagSimUnit.Add(new EcuDiagSimLuaCoreTableForIso157653OnIso157652(
                                                        _luaEcuDiagSimUnit.GetEntryPointNameByIndex(i),
                                                        _luaEcuDiagSimUnit.GetLuaCoreTableByIndex(i),
                                                        _vci.OpenComLogicalLink(resourceIds.First()) )
                                                    );
                            }
                        }
                    }
                }

                if ( _luaEcuDiagSimUnit.NumberWorkingEcus == counter )
                {
                    //alles gut
                }
            }
        }

        public override void SetupComLogicalLink()
        {
            foreach ( var coreTable in _luaEcuDiagSimUnit._ecuDiagSimLuaCoreTables)
            {
                coreTable.SetupComLogicalLink();
            }
        }


        public override LuaEcuDiagSimUnit GetResult()
        {
            return _luaEcuDiagSimUnit;
        }


    }

    abstract class LuaEcuDiagSimUnitBuilder
    {
        public abstract void JoinLuaUnitWithFileName();
        public abstract void EnrichLuaWorld();
        public abstract void CompileChunk();
        public abstract void DoChunk();
        public abstract void IsLuaFileContentsEcuDiagSimLua();

        public abstract void CreateComLogicalLink();

        public abstract void SetupComLogicalLink();
        public abstract LuaEcuDiagSimUnit GetResult();



    }

    class LuaEcuDiagSimUnitBuildDirector
    {
        // LuaEcuDiagSimBuilder uses a complex series of steps
        public void Construct(LuaEcuDiagSimUnitBuilder luaEcuDiagSimBuilder)
        {
            Load(luaEcuDiagSimBuilder);
            Verify(luaEcuDiagSimBuilder);
            luaEcuDiagSimBuilder.CreateComLogicalLink();
            luaEcuDiagSimBuilder.SetupComLogicalLink();
            
        }

        private void Load(LuaEcuDiagSimUnitBuilder luaEcuDiagSimBuilder)
        {
            luaEcuDiagSimBuilder.JoinLuaUnitWithFileName();
            luaEcuDiagSimBuilder.EnrichLuaWorld();
            luaEcuDiagSimBuilder.CompileChunk();
            luaEcuDiagSimBuilder.DoChunk();
        }

        private void Verify(LuaEcuDiagSimUnitBuilder luaEcuDiagSimBuilder)
        {
            luaEcuDiagSimBuilder.IsLuaFileContentsEcuDiagSimLua();
        }

    }

    class LuaEcuDiagSimUnit : Lua
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<LuaEcuDiagSimUnit>();

        public FileInfo FullLuaFileName { get; internal set; }
        public List<EcuDiagSimLuaCoreTableBase> _ecuDiagSimLuaCoreTables = new();
        private List<DataForComLogicalLinkCreation> _dataSetsForCllCreation = new();

        private readonly List<string> _entryPointTableNames = new();

        public LuaEcuDiagSimUnit()
        {
            _environment = CreateEnvironment<LuaGlobal>();
        }

        private LuaGlobal _environment;
        private LuaChunk _chunk;
        private LuaResult _result;

        public Lua LuaScriptEngine { get; set; }

        public LuaGlobal Environment
        {
            get => _environment;
        }

        public dynamic DynamicEnvironment
        {
            get => _environment;
        }

        public void Add(EcuDiagSimLuaCoreTableBase coreTable)
        {
            _ecuDiagSimLuaCoreTables.Add(coreTable);
        }

        public string GetEntryPointNameByIndex(int index)
        {
            return _entryPointTableNames[index];
        }

        public List<Task> Connect(CancellationToken ct)
        {
            List<Task> tl = new List<Task>();
            foreach ( var coreTable in _ecuDiagSimLuaCoreTables)
            {
                tl.Add(coreTable.Connect(ct));
            }

            return tl;
        }

        public LuaTable GetLuaCoreTableByIndex(int index)
        {
            var a = (LuaTable)_environment[_entryPointTableNames[index]];
            
            return (LuaTable)_environment[_entryPointTableNames[index]];
        }

        public int NumberWorkingEcus => _ecuDiagSimLuaCoreTables.Count();

        public DataForComLogicalLinkCreation? GetRequiredDataForComLogicalLinkCreationByIndex(int index)
        {
            DataForComLogicalLinkCreation dataSetsForCllCreation = null;
            var entryPointTableName = _entryPointTableNames[index];

            if ( IsLightweightHeader(entryPointTableName) )
            {
                //Fix value for LightweightHeader
                dataSetsForCllCreation = new DataForComLogicalLinkCreation();
            }
            else if ( ((LuaTable)_environment[entryPointTableName]).Members["DataForComLogicalLinkCreation"] is LuaTable table )
            {
                //in LUA File it looks like this
                //EcuName = {
                //    DataForComLogicalLinkCreation = {
                //        BusTypeShortName = "ISO_11898_2_DWCAN",
                //        ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
                //        DlcPinData = {
                //            ["6"] = "HI",
                //            ["14"] = "LOW",
                //        },
                //    },
                //    Raw = ....

                var busTypeShortName = (string)table.Members["BusTypeShortName"];
                var protocolShortName = (string)table.Members["ProtocolShortName"];
                if ( (LuaTable)table.Members["DlcPinData"] is LuaTable DlcPinData )
                {
                    Dictionary<uint, string> dlcPinDataDic = new();
                    foreach ( var pair in DlcPinData )
                    {
                        var success = uint.TryParse((string)(pair.Key), out var number);
                        if ( !success )
                        {
                            _logger.LogError("Attempted conversion of DlcPin '{DlcPin}' failed. Use default 0", (string)(pair.Key));
                        }

                        dlcPinDataDic.Add(number, (string)(pair.Value));
                    }

                    dataSetsForCllCreation = new DataForComLogicalLinkCreation()
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName,
                        DlcPinData = dlcPinDataDic
                    };
                }
                else
                {
                    //if DlcPinData is missing 
                    dataSetsForCllCreation = new DataForComLogicalLinkCreation()
                    {
                        BusTypeShortName = busTypeShortName,
                        ProtocolShortName = protocolShortName,
                        //DlcPinData DLC is default in this case { { 6, "HI" }, { 14, "LOW" } };
                    };
                }
            }

            return dataSetsForCllCreation;
        }

        /// <summary>
        /// GetRequiredDataForComLogicalLinkCreation (used by this lua unit...comes from the lua file content)
        /// </summary>
        //public List<DataForComLogicalLinkCreation> GetRequiredDataForComLogicalLinkCreation()
        //{
        //    _dataSetsForCllCreation.Clear();
        //    foreach ( var entryPointTableName in _entryPointTableNames )
        //    {
        //        if ( IsLightweightHeader(entryPointTableName) )
        //        {
        //            //Fix value for LightweightHeader
        //            _dataSetsForCllCreation.Add(new DataForComLogicalLinkCreation());
        //        }
        //        else if (((LuaTable)_environment[entryPointTableName]).Members["DataForComLogicalLinkCreation"] is LuaTable table )
        //        {
        //            //in LUA File it looks like this
        //            //EcuName = {
        //            //    DataForComLogicalLinkCreation = {
        //            //        BusTypeShortName = "ISO_11898_2_DWCAN",
        //            //        ProtocolShortName = "ISO_15765_3_on_ISO_15765_2",
        //            //        DlcPinData = {
        //            //            ["6"] = "HI",
        //            //            ["14"] = "LOW",
        //            //        },
        //            //    },
        //            //    Raw = ....

        //            var busTypeShortName = (string)table.Members["BusTypeShortName"];
        //            var protocolShortName = (string)table.Members["ProtocolShortName"];
        //            if ( ((LuaTable)table.Members["DlcPinData"] is LuaTable DlcPinData) )
        //            {
        //                Dictionary<uint, string> dlcPinDataDic = new();
        //                foreach (var  pair in DlcPinData )
        //                {
        //                    uint number = 0;
        //                    bool success = uint.TryParse((string)(pair.Key), out number);
        //                    if (!success)
        //                    {
        //                        _logger.LogError("Attempted conversion of DlcPin '{DlcPin}' failed. Use default 0", (string)(pair.Key));
        //                    }
        //                    dlcPinDataDic.Add(number, (string)(pair.Value));
        //                }

        //                _dataSetsForCllCreation.Add(new DataForComLogicalLinkCreation()
        //                {
        //                    BusTypeShortName = busTypeShortName,
        //                    ProtocolShortName = protocolShortName,
        //                    DlcPinData = dlcPinDataDic
        //                });
        //            }
        //            else
        //            {
        //                //if DlcPinData is missing 
        //                _dataSetsForCllCreation.Add(new DataForComLogicalLinkCreation()
        //                {
        //                    BusTypeShortName = busTypeShortName,
        //                    ProtocolShortName = protocolShortName,
        //                    //DlcPinData DLC is default in this case { { 6, "HI" }, { 14, "LOW" } };
        //                });
        //            }
        //        }
        //    }
        //    return _dataSetsForCllCreation;
        //}



        private bool IsLightweightHeader(string entryPointName)
        {
            //in LUA File it looks like this
            //EcuName = {
            //    RequestId = 0x7e0,
            //    ResponseId = 0x7e8,
            //    RequestFunctionalId = 0x750,
            //    Raw = ...

            var requestId = (uint?)(int?)((LuaTable)_environment[entryPointName]).Members["RequestId"];
            var responseId = (uint?)(int?)((LuaTable)_environment[entryPointName]).Members["ResponseId"];
            return responseId != null && requestId != null;
        }

        internal string BuildCompileChunk
        {
            get
            {
                try
                {
                    _chunk = CompileChunk(FullLuaFileName.FullName, new LuaCompileOptions() { DebugEngine = LuaExceptionDebugger.Default });
                    return "";
                }
                catch (LuaParseException ex)
                {
                    _logger.LogCritical(ex, "LUA with name {fullFileName} makes trouble", FullLuaFileName.FullName);
                    return $"LUA with name {FullLuaFileName.FullName} makes trouble";
                }
            }
        }

        internal string BuildDoChunk
        {
            get
            {
                try
                {
                    _result = _environment.DoChunk(_chunk);
                    return "";
                }
                catch (Exception ex)
                {
                    var luaExceptionData = LuaExceptionData.GetData(ex); // get stack trace
                    _logger.LogCritical(ex, "LUA intern {luaExceptionData} makes trouble", luaExceptionData);
                    return $"LUA intern {ex.Message} makes trouble";
                }
            }
        }

        internal int NumberOfEntryPoints
        {
            get
            {
                _entryPointTableNames.Clear();
                foreach (var item in _environment.Members)
                {
                    if ( item.Value is LuaTable table )
                    {
                        foreach (var item2 in table.Members)
                        {
                            if (item2.Key.Equals("Raw"))
                            {
                                _entryPointTableNames.Add(item.Key);
                                _logger.LogInformation("Name of EntryPoint {EntryPointTableName}", item.Key);
                                break;
                            }
                        }
                    }
                }
                return _entryPointTableNames.Count();
            }
        }


        // Define the event handlers.  
        internal void FileChanged(object source, FileSystemEventArgs e)
        {
            if ( e.FullPath.Equals(this.FullLuaFileName.FullName) )
            {
                // Specify what is done when a file is changed.  
                _logger.LogInformation("{Name}, with path {FullPath} has been {ChangeType}", e.Name, e.FullPath, e.ChangeType);
                //ToDo Lua Rebuilt
            }
        }

    }

}
