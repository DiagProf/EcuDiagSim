﻿#region License

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
    //delegate void ActionRef<T1, T2>(T1 itemI, ref T2 item);

    internal class LuaEcuDiagSimUnit : Lua
    {
        private readonly ILogger _logger = ApiLibLogging.CreateLogger<LuaEcuDiagSimUnit>();
        private readonly List<string> _entryPointCoreTableNames = new();
        private LuaChunk _chunk;
        private LuaResult _result;

        private readonly SemaphoreSlim _semaphoreHotReload = new(0, 1);
      

        private DateTime _lastRead = DateTime.MinValue;
        internal int ResourceBusy;
        internal ReaderWriterLockSlim RwLocker = new();
        public List<AbstractEcuDiagSimLuaCoreTable> EcuDiagSimLuaCoreTables = new();
        public dynamic DynamicEnvironment => Environment;
        public LuaGlobal Environment { get; }
        public FileInfo FullLuaFileName { get; internal set; }
        public bool IsEcuDiagSimLua => _entryPointCoreTableNames.Any();

        public LuaEcuDiagSimUnit(FileInfo fullLuaFileName)
        {
            FullLuaFileName = fullLuaFileName;
            Environment = CreateEnvironment<LuaGlobal>();
        }

        public bool EvaluateAndPairingCoreTableWithComLogicalLink(Module vci)
        {
            foreach ( var entryPointCoreTableName in _entryPointCoreTableNames )
            {
                var table = (LuaTable)Environment[entryPointCoreTableName];

                if ( EcuDiagSimLuaCoreTableForIso157653OnIso157652.IsThisClassForThisLuaTable(table) )
                {
                    var cllCreationData = AbstractEcuDiagSimLuaCoreTable.GetDataForComLogicalLinkCreation(table);
                    var resourceIds = vci.GetResourceIds(cllCreationData.BusTypeShortName, cllCreationData.ProtocolShortName, cllCreationData.DlcPinData.ToList());
                    if ( resourceIds.Any() )
                    {
                        EcuDiagSimLuaCoreTables.Add(new EcuDiagSimLuaCoreTableForIso157653OnIso157652(this, entryPointCoreTableName,
                            vci.OpenComLogicalLink(resourceIds.First())));
                    }
                    else
                    {
                        _logger.LogError("VCI does not support the resource requested by CoreTable: {entryPointCoreTableName} (inside LUA File: {FullLuaFileName}) ",
                            entryPointCoreTableName, FullLuaFileName.FullName);
                        return false;
                    }
                }
                else if ( EcuDiagSimLuaCoreTableForIso11898Raw.IsThisClassForThisLuaTable(table) )
                {
                    var cllCreationData = AbstractEcuDiagSimLuaCoreTable.GetDataForComLogicalLinkCreation(table);
                    var resourceIds = vci.GetResourceIds(cllCreationData.BusTypeShortName, cllCreationData.ProtocolShortName, cllCreationData.DlcPinData.ToList());
                    if ( resourceIds.Any() )
                    {
                        EcuDiagSimLuaCoreTables.Add(new EcuDiagSimLuaCoreTableForIso11898Raw(this, entryPointCoreTableName, vci.OpenComLogicalLink(resourceIds.First())));
                    }
                    else
                    {
                        _logger.LogError("VCI does not support the resource requested by CoreTable: {entryPointCoreTableName} (inside LUA File: {FullLuaFileName}) ",
                            entryPointCoreTableName, FullLuaFileName.FullName);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("EcuDiagSim does not support the resource requested by CoreTable: {entryPointCoreTableName} (inside LUA File: {FullLuaFileName}) ",
                        entryPointCoreTableName, FullLuaFileName.FullName);
                    return false;
                }
            }

            return true;
        }


        public List<Task> Connect(CancellationToken ct)
        {
            var tasks = new List<Task>();
            foreach ( var coreTable in EcuDiagSimLuaCoreTables )
            {
                tasks.Add(coreTable.Connect(ct));
            }

            return tasks;
        }

        public bool SetupCllData()
        {
            foreach ( var coreTable in EcuDiagSimLuaCoreTables )
            {
                if ( !coreTable.SetupCllData() )
                {
                    return false;
                }
            }

            return true;
        }

        // Define the event handlers.  
        internal void FileChanged(object source, FileSystemEventArgs ev)
        {
            if ( ev.ChangeType == WatcherChangeTypes.Changed )
            {
                //Writing a file takes place in more than one step.
                //Therefore there are several changed events when a file is written
                //This workaround is necessary: because we need only one event 
                var lastWriteTime = File.GetLastWriteTime(ev.FullPath);
                if ( lastWriteTime >= _lastRead + TimeSpan.FromMilliseconds(100) )
                {
                    if ( ev.FullPath.Equals(FullLuaFileName.FullName) )
                    {
                        _lastRead = lastWriteTime;
                        _semaphoreHotReload.Release();
                    }
                }
                // else discard the (duplicated) OnChanged event
            }
        }

        public void StartHotReloadTask(CancellationToken ct)
        {
            Task.Factory.StartNew(async _ =>
            {
                while ( !ct.IsCancellationRequested )
                {
                    await _semaphoreHotReload.WaitAsync(ct).ConfigureAwait(false);
                    //This delay ensures that we don't reload the file again
                    //while the operating system is still performing various file write actions.
                    await Task.Delay(100, ct).ConfigureAwait(false);

                    //we use ResourceBusy to really send "Response Pending" for some protocols :-)
                    Interlocked.Exchange(ref ResourceBusy, 1);
                    await LuaFileHotReload().ConfigureAwait(false);
                    Interlocked.Exchange(ref ResourceBusy, 0);
                }
            }, ct, TaskCreationOptions.LongRunning);
        }

        private Task LuaFileHotReload()
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            //EnterWriteLock: Acquires a writer lock.
            //If any reader or writer locks are already held, this method will block until all locks are released.
            RwLocker.EnterWriteLock();
            try
            {
                // Specify what is done when a file is changed.  
                new Builder(this)
                    //.EnrichLuaWorld()
                    .CompileChunk()
                    .DoChunk();

                foreach ( var coreTable in EcuDiagSimLuaCoreTables )
                {
                    coreTable.Refresh();
                }
                
                _logger.LogInformation("HotReload for file {FullLuaFileName}", FullLuaFileName);
                
            }
            finally
            {
                tcs.SetResult();
                RwLocker.ExitWriteLock();
            }

            return tcs.Task;
        }

        protected override void Dispose(bool disposing)
        {
            _semaphoreHotReload.Dispose();
            RwLocker.Dispose();
            base.Dispose(disposing);
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

            internal Builder(LuaEcuDiagSimUnit self)
            {
                _diagSimUnit = self;
            }

            public Builder EnrichLuaWorld()
            {
                _diagSimUnit.Environment["str2sbs"] = new Func<string, string>(LuaWorldEnricher.Str2Sbs);
                //ascii is only for comatility reasons
                _diagSimUnit.Environment["ascii"] = new Func<string, string>(LuaWorldEnricher.Str2Sbs);
                _diagSimUnit.Environment["sbs2str"] = new Func<string, string>(LuaWorldEnricher.Sbs2Str);
                _diagSimUnit.Environment["sleep"] = new Action<int>(LuaWorldEnricher.Sleep);

                _diagSimUnit.Environment["num2uInt8Sbs"] = new Func<int, string>(LuaWorldEnricher.Num2UInt8Sbs);
                _diagSimUnit.Environment["sbs4uInt8Num"] = new Func<string, int>(LuaWorldEnricher.Sbs4UInt8Num);

                //_diagSimUnit.Environment["num2UInt16Sbs"] = new Func<int, string>(LuaWorldEnricher.Num2UInt16Sbs);
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
                    _logger.LogCritical(ex, "LUA intern {luaExceptionData} makes trouble with File {fullFileName}", luaExceptionData,
                        _diagSimUnit.FullLuaFileName.FullName);
                }

                return this;
            }


            public Builder EntryPoints()
            {
                _diagSimUnit._entryPointCoreTableNames.Clear();
                foreach ( var item in _diagSimUnit.Environment.Members )
                {
                    if ( item.Value is LuaTable table )
                    {
                        foreach ( var item2 in table.Members )
                        {
                            if ( item2.Key.Equals("Raw") )
                            {
                                _diagSimUnit._entryPointCoreTableNames.Add(item.Key);
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
