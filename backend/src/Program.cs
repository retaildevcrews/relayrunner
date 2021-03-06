// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Extensions.Logging;
using RelayRunner.Middleware;

namespace RelayRunner.Application
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        // capture parse errors from env vars
        private static readonly List<string> EnvVarErrors = new ();

        // App configuration values
        public static Config Config { get; } = new ();
        private static IChangeFeedProcessor ChangeFeedProcessor { get; set; }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>0 == success</returns>
        public static int Main(string[] args)
        {
            DisplayAsciiArt(args);

            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create<Config>(RunApp);

            // run the app
            return root.Invoke(args);
        }

        /// <summary>
        /// Run the app
        /// </summary>
        /// <param name="config">command line config</param>
        /// <returns>status</returns>
        public static async Task<int> RunApp(Config config)
        {
            NgsaLog logger = new () { Name = typeof(App).FullName };

            try
            {
                // copy command line values
                Config.SetConfig(config);

                // load secrets from volume
                LoadSecrets();

                // create the cosmos data access layer
                Config.CosmosDal = new DataAccessLayer.CosmosDal(Config.Secrets, Config);

                // create cache with initial values
                Config.Cache = new Data.Cache(Config);

                // set the logger config
                RequestLogger.CosmosName = Config.CosmosName;
                NgsaLog.LogLevel = Config.LogLevel;

                // build the host
                IWebHost host = BuildHost();

                if (host == null)
                {
                    return -1;
                }

                // setup sigterm handler
                CancellationTokenSource ctCancel = SetupSigTermHandler(host, logger);

                // start the webserver
                Task w = host.RunAsync();

                // log startup messages
                logger.LogInformation($"RelayRunner Backend Started", VersionExtension.Version);

                // start CosmosDB Change Feed Processor
                ChangeFeedProcessor = await RunChangeFeedProcessor();

                // this doesn't return except on ctl-c or sigterm
                await w.ConfigureAwait(false);

                // if not cancelled, app exit -1
                return ctCancel.IsCancellationRequested ? 0 : -1;
            }
            catch (Exception ex)
            {
                // end app on error
                logger.LogError(nameof(RunApp), "Exception", ex: ex);

                return -1;
            }
        }

        // load secrets from volume
        private static void LoadSecrets()
        {
            Config.Secrets = Secrets.GetSecretsFromVolume(Config.SecretsVolume);

            // set the Cosmos server name for logging
            Config.CosmosName = Config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
            int ndx = Config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);
            if (ndx > 0)
            {
                Config.CosmosName = Config.CosmosName.Remove(ndx);
            }
        }

        // Build the web host
        private static IWebHost BuildHost()
        {
            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://*:{Config.Port}/")
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(10))
                .ConfigureLogging(logger =>
                {
                    // log to XML
                    // this can be replaced when the dotnet XML logger is available
                    logger.ClearProviders();
                    logger.AddNgsaLogger(config => { config.LogLevel = Config.LogLevel; });

                    // if you specify the --log-level option, it will override the appsettings.json options
                    // remove any or all of the code below that you don't want to override
                    if (Config.IsLogLevelSet)
                    {
                        logger.AddFilter("Microsoft", Config.LogLevel)
                        .AddFilter("System", Config.LogLevel)
                        .AddFilter("Default", Config.LogLevel)
                        .AddFilter("RelayRunner.Application", Config.LogLevel);
                    }
                });

            // build the host
            return builder.Build();
        }

        // Create a CancellationTokenSource that cancels on ctl-c or sigterm
        private static CancellationTokenSource SetupSigTermHandler(IWebHost host, NgsaLog logger)
        {
            CancellationTokenSource ctCancel = new ();

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                ctCancel.Cancel();

                logger.LogInformation("Shutdown", "Shutting Down ...");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                await host.StopAsync().ConfigureAwait(false);

                // end the app
                Environment.Exit(0);
            };

            return ctCancel;
        }

        private static async Task<IChangeFeedProcessor> RunChangeFeedProcessor()
        {
            const string ChangeFeedLeaseName = "RRAPI";

            DocumentCollectionInfo feedCollectionInfo = new ()
            {
                DatabaseName = Config.Secrets.CosmosDatabase,
                CollectionName = Config.Secrets.CosmosCollection,
                Uri = new Uri(Config.Secrets.CosmosServer),
                MasterKey = Config.Secrets.CosmosKey,
            };

            DocumentCollectionInfo leaseCollectionInfo = new ()
            {
                DatabaseName = Config.Secrets.CosmosDatabase,
                CollectionName = ChangeFeedLeaseName,
                Uri = new Uri(Config.Secrets.CosmosServer),
                MasterKey = Config.Secrets.CosmosKey,
            };

            return await ChangeFeed.Processor.RunAsync($"Host - {Guid.NewGuid()}", feedCollectionInfo, leaseCollectionInfo);
        }
    }
}
