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

using DiagEcuSim;
using ISO22900.II;
using Microsoft.Extensions.Logging;
using Neo.IronLua;

namespace EcuDiagSim
{
    internal class LuaEcuDiagSimUnit : Lua
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<LuaEcuDiagSimUnit>();

        internal readonly List<string> _entryPointTableNames = new();
        private LuaChunk _chunk;
        private List<DataForComLogicalLinkCreation> _dataSetsForCllCreation = new();
        private LuaResult _result;
        public List<AbstractEcuDiagSimLuaCoreTable> _ecuDiagSimLuaCoreTables = new();
        public dynamic DynamicEnvironment => Environment;
        public LuaGlobal Environment { get; }
        public FileInfo FullLuaFileName { get; internal set; }
        public bool IsEcuDiagSimLua => _entryPointTableNames.Any();

        public LuaEcuDiagSimUnit(FileInfo fullLuaFileName)
        {
            FullLuaFileName = fullLuaFileName;
            Environment = CreateEnvironment<LuaGlobal>();
        }

        public bool BuiltRrTable(Module vci)
        {
            foreach ( var entryPointTableName in _entryPointTableNames )
            {
                var table = (LuaTable)Environment[entryPointTableName];

                if ( EcuDiagSimLuaCoreTableForIso157653OnIso157652.IsThisClassForThisLuaTable(table) )
                {
                    var cllCreationData = AbstractEcuDiagSimLuaCoreTable.GetDataForComLogicalLinkCreation(table);
                    var resourceIds = vci.GetResourceIds(cllCreationData.BusTypeShortName, cllCreationData.ProtocolShortName, cllCreationData.DlcPinData.ToList());
                    if ( resourceIds.Any() )
                    {
                        _ecuDiagSimLuaCoreTables.Add(new EcuDiagSimLuaCoreTableForIso157653OnIso157652(this, entryPointTableName, table,vci.OpenComLogicalLink(resourceIds.First())));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }


        public List<Task> Connect(CancellationToken ct)
        {
            var tasks = new List<Task>();
            foreach ( var coreTable in _ecuDiagSimLuaCoreTables )
            {
                tasks.Add(coreTable.Connect(ct));
            }
            return tasks;
        }

        public LuaTable GetLuaCoreTableByIndex(int index)
        {
            var a = (LuaTable)Environment[_entryPointTableNames[index]];

            return (LuaTable)Environment[_entryPointTableNames[index]];
        }


        // Define the event handlers.  
        internal void FileChanged(object source, FileSystemEventArgs e)
        {
            if ( e.FullPath.Equals(FullLuaFileName.FullName) )
            {
                // Specify what is done when a file is changed.  
                _logger.LogInformation("{Name}, with path {FullPath} has been {ChangeType}", e.Name, e.FullPath, e.ChangeType);
                //ToDo Lua Rebuilt
            }
        }


        //Nested builder
        public class Builder
        {
            private readonly LuaEcuDiagSimUnit _diagSimUnit;
            private readonly ILogger _logger = ApiLibLogging.CreateLogger<LuaEcuDiagSimUnit>();

            public Builder(FileInfo fullLuaFileName)
            {
                _diagSimUnit = new LuaEcuDiagSimUnit(fullLuaFileName);
            }

            private Builder(LuaEcuDiagSimUnit self)
            {
                _diagSimUnit = self;
            }

            public Builder EnrichLuaWorld()
            {
                _diagSimUnit.Environment["ascii"] = new Func<string, string>(LuaWorldEnricher.Ascii);
                _diagSimUnit.Environment["sleep"] = new Action<int>(LuaWorldEnricher.Sleep);
                return this;
            }

            public Builder CompileChunk()
            {
                try
                {
                    _diagSimUnit._chunk = _diagSimUnit.CompileChunk(_diagSimUnit.FullLuaFileName.FullName,
                        new LuaCompileOptions { DebugEngine = LuaExceptionDebugger.Default });
                }
                catch ( LuaParseException ex )
                {
                    _logger.LogCritical(ex, "LUA with name {fullFileName} makes trouble", _diagSimUnit.FullLuaFileName.FullName);
                }

                return this;
            }

            public Builder DoChunk()
            {
                try
                {
                    _diagSimUnit._result = _diagSimUnit.Environment.DoChunk(_diagSimUnit._chunk);
                }
                catch ( Exception ex )
                {
                    var luaExceptionData = LuaExceptionData.GetData(ex); // get stack trace
                    _logger.LogCritical(ex, "LUA intern {luaExceptionData} makes trouble", luaExceptionData);
                }

                return this;
            }


            public Builder EntryPoints()
            {
                _diagSimUnit._entryPointTableNames.Clear();
                foreach ( var item in _diagSimUnit.Environment.Members )
                {
                    if ( item.Value is LuaTable table )
                    {
                        foreach ( var item2 in table.Members )
                        {
                            if ( item2.Key.Equals("Raw") )
                            {
                                _diagSimUnit._entryPointTableNames.Add(item.Key);
                                _logger.LogInformation("Name of EntryPoint {EntryPointTableName}", item.Key);
                                break;
                            }
                        }
                    }
                }

                return this;
            }

            public LuaEcuDiagSimUnit Build()
            {
                return _diagSimUnit;
            }
        }
    }
}
