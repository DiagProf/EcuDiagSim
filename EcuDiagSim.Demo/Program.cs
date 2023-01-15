using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace EcuDiagSim.Demo
{
    internal class Program
    {
        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                //Next two only if need... not just now
                .AddJsonFile($"appsettings.json.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddEnvironmentVariables();
        }

        private class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", Thread.CurrentThread.ManagedThreadId));
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder();
                BuildConfig(builder);
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Build())
                    // .MinimumLevel.Debug() // <- Set the minimum level
                    .Enrich.FromLogContext()
                    .Enrich.With(new ThreadIdEnricher())
                    .Enrich.WithProperty("Version", "1.0")
                    //.WriteTo.Async(wt => wt.Console())
                   
                    .CreateLogger();

                using ( var host = Host.CreateDefaultBuilder(args)
                           .ConfigureServices((hostContext, services) =>
                           {
                               services.AddSingleton<IApp, App>();
                           })
                           .UseSerilog()
                           .Build() )
                {
                    var app = host.Services.GetRequiredService<IApp>();
                    await app.RunAsync();
                }
            }
            catch (Exception e)
            {
                //AnsiConsole.WriteException(e);
                Console.ReadLine();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}