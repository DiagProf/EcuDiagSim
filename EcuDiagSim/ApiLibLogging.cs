using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



namespace EcuDiagSim
{
    internal static class ApiLibLogging
    {
        //https://docs.microsoft.com/de-de/archive/msdn-magazine/2016/april/essential-net-logging-with-net-core
        internal static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

        internal static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}