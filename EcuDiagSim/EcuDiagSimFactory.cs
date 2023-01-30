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
//using Microsoft.Extensions.Hosting;

namespace EcuDiagSim
{
    //https://codeopinion.com/cap-event-bus-outbox-pattern/
    //https://github.com/dotnetcore/CAP/issues/275
    public class EcuDiagSimFactory : IDisposable
    {
        private static readonly Dictionary<string, EcuDiagSimManager> Cache = new();

        public EcuDiagSimFactory()
        {
            
        }

        //public static Module GetVci(string dPduApiLibraryPath, ILoggerFactory loggerFactory, string optionStr, ApiModifications apiModFlags,
        //    string vciModuleName = "")
        //{
        //    return GetApi(dPduApiLibraryPath, loggerFactory, optionStr, apiModFlags).ConnectVci(vciModuleName);
        //}

        //public static Module GetVci(string dPduApiLibraryPath, ILoggerFactory loggerFactory, ApiModifications apiModFlags,
        //    string vciModuleName = "")
        //{
        //    return GetApi(dPduApiLibraryPath, loggerFactory, apiModFlags).ConnectVci(vciModuleName);
        //}

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

    //public class PublishController : IHostedService
    //{
    //    private Timer _timer;
    //    private readonly ICapPublisher _eventBus;

    //    public PublishController(ICapPublisher eventBus)
    //    {
    //        _eventBus = eventBus;
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
            
    //        _timer = new Timer(Publish, null, TimeSpan.Zero,
    //            TimeSpan.FromSeconds(5));

    //        return Task.CompletedTask;
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        _timer?.Change(Timeout.Infinite, 0);
    //        return Task.CompletedTask;
    //    }

    //    private void Publish(object state)
    //    {
    //        _eventBus.Publish("test.show.time", DateTime.Now);
    //    }



    //    //[Route("~/send")]
    //    //public IActionResult SendMessage([FromServices] ICapPublisher capBus)
    //    //{
    //    //    capBus.Publish("test.show.time", DateTime.Now);

    //    //    return Ok();
    //    //}
    //}

internal class EcuDiagSimManager: IDisposable
    {
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
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
