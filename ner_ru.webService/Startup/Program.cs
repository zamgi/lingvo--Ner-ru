using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using captcha;
using lingvo.sentsplitting;

namespace lingvo.ner.webService
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        public const string SERVICE_NAME = "ner_ru.webService";

        /// <summary>
        /// 
        /// </summary>
        private sealed class environment : IDisposable
        {
            private environment() { }
            public void Dispose()
            {
                if ( SentSplitterConfig != null )
                {
                    SentSplitterConfig.Dispose();
                    SentSplitterConfig = null;
                }
            }

            public  NerProcessorConfig NerProcessorConfig { get; private set; }
            private SentSplitterConfig SentSplitterConfig { get; set; }

            public static environment Create( Config opts, bool print2Console = true )
            {
                var sw = default(Stopwatch);
                if ( print2Console )
                {
                    sw = Stopwatch.StartNew();
                    Console.Write( "init ner-environment..." );
                }

                var (config, ssc) = opts.CreateNerProcessorConfig();

                var posEnv = new environment()
                {
                    SentSplitterConfig = ssc,
                    NerProcessorConfig = config,
                };

                if ( print2Console )
                {
                    sw.Stop();
                    Console.WriteLine( $"end, (elapsed: {sw.Elapsed}).\r\n----------------------------------------------------\r\n" );
                }

                return (posEnv);
            }
        }

        private static async Task Main( string[] args )
        {
            var hostApplicationLifetime = default(IHostApplicationLifetime);
            var logger                  = default(ILogger);
            try
            {
                //---------------------------------------------------------------//
                var opts = new Config();
                using var env = environment.Create( opts );

                using var concurrentFactory = new ConcurrentFactory( env.NerProcessorConfig, opts );
                //---------------------------------------------------------------//

                var host = Host.CreateDefaultBuilder( args )
                               .ConfigureLogging( loggingBuilder => loggingBuilder.ClearProviders().AddDebug().AddConsole().AddEventSourceLogger()
                                                              .AddEventLog( new EventLogSettings() { LogName = SERVICE_NAME, SourceName = SERVICE_NAME } ) )
                               //---.UseWindowsService()
                               .ConfigureServices( (hostContext, services) => services.AddSingleton< IConfig >( opts ).AddSingleton< IAntiBotConfig >( opts ).AddSingleton( concurrentFactory ) )
                               .ConfigureWebHostDefaults( webBuilder => webBuilder.UseStartup< Startup >() )
                               .Build();
                hostApplicationLifetime = host.Services.GetService< IHostApplicationLifetime >();
                logger                  = host.Services.GetService< ILoggerFactory >()?.CreateLogger( SERVICE_NAME );
                await host.RunAsync();
            }
            catch ( OperationCanceledException ex ) when ((hostApplicationLifetime?.ApplicationStopping.IsCancellationRequested).GetValueOrDefault())
            {
                Debug.WriteLine( ex ); //suppress
            }
            catch ( Exception ex ) when (logger != null)
            {
                logger.LogCritical( ex, "Global exception handler" );
            }
        }

        private static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > t ) => t.ConfigureAwait( false );
        private static ConfiguredTaskAwaitable CAX( this Task t ) => t.ConfigureAwait( false );
    }
}
