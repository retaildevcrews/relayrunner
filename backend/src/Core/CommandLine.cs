// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Extensions.Logging;
using RelayRunner.Application.ChangeFeed;
using RelayRunner.Middleware;

namespace RelayRunner.Application
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        // capture parse errors from env vars
        private const string ChangeFeedLeaseName = "RRAPI";
        private static readonly List<string> EnvVarErrors = new ();

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
                SetConfig(config);

                // build the host
                IWebHost host = BuildHost();

                if (host == null)
                {
                    return -1;
                }

                // setup sigterm handler
                CancellationTokenSource ctCancel = SetupSigTermHandler(host, logger);

                // log startup messages
                LogStartup(logger);

                // start the webserver
                Task w = host.RunAsync();

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

                IChangeFeedProcessor processor = await Processor.RunAsync($"Host - {Guid.NewGuid()}", feedCollectionInfo, leaseCollectionInfo);

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

        /// <summary>
        /// Build the RootCommand for parsing
        /// </summary>
        /// <returns>RootCommand</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new ()
            {
                Name = "RelayRunner.Application",
                Description = "RelayRunner Validation App",
                TreatUnmatchedTokensAsErrors = true,
            };

            // add the options
            root.AddOption(EnvVarOption(new string[] { "--url-prefix" }, "URL prefix for ingress mapping", string.Empty));
            root.AddOption(EnvVarOption(new string[] { "--port" }, "Listen Port", 8080, 1, (64 * 1024) - 1));
            root.AddOption(EnvVarOption(new string[] { "--retries" }, "Cosmos 429 retries", 10, 0));
            root.AddOption(EnvVarOption(new string[] { "--timeout" }, "Request timeout", 10, 1));
            root.AddOption(EnvVarOption(new string[] { "--secrets-volume", "-v" }, "Secrets Volume Path", "secrets"));
            root.AddOption(EnvVarOption(new string[] { "--log-level", "-l" }, "Log Level", LogLevel.Error));
            root.AddOption(EnvVarOption(new string[] { "--request-log-level", "-q" }, "Request Log Level", LogLevel.Information));

            // validate dependencies
            root.AddValidator(ValidateDependencies);

            return root;
        }

        // validate combinations of parameters
        private static string ValidateDependencies(CommandResult result)
        {
            string msg = string.Empty;

            if (EnvVarErrors.Count > 0)
            {
                msg += string.Join('\n', EnvVarErrors) + '\n';
            }

            try
            {
                // get the values to validate
                string secrets = result.Children.FirstOrDefault(c => c.Symbol.Name == "secrets-volume") is OptionResult secretsRes ? secretsRes.GetValueOrDefault<string>() : string.Empty;
                string urlPrefix = result.Children.FirstOrDefault(c => c.Symbol.Name == "urlPrefix") is OptionResult urlRes ? urlRes.GetValueOrDefault<string>() : string.Empty;

                // validate url-prefix
                if (!string.IsNullOrWhiteSpace(urlPrefix))
                {
                    urlPrefix = urlPrefix.Trim();

                    if (urlPrefix.Length < 2)
                    {
                        msg += "--url-prefix is invalid";
                    }

                    if (!urlPrefix.StartsWith('/'))
                    {
                        msg += "--url-prefix must start with /";
                    }
                }

                if (string.IsNullOrWhiteSpace(secrets))
                {
                    msg += "--secrets-volume cannot be empty\n";
                }
                else
                {
                    try
                    {
                        // validate secrets-volume exists
                        if (!Directory.Exists(secrets))
                        {
                            msg += $"--secrets-volume ({secrets}) does not exist\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        msg += $"--secrets-volume exception: {ex.Message}\n";
                    }
                }
            }
            catch
            {
                // system.commandline will catch and display parse exceptions
            }

            // return error message(s) or string.empty
            return msg;
        }

        // insert env vars as default
        private static Option EnvVarOption<T>(string[] names, string description, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            T value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (defaultValue.GetType().IsEnum)
                {
                    if (Enum.TryParse(defaultValue.GetType(), env, true, out object result))
                    {
                        value = (T)result;
                    }
                    else
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
                else
                {
                    try
                    {
                        value = (T)Convert.ChangeType(env, typeof(T));
                    }
                    catch
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
            }

            return new Option<T>(names, () => value, description);
        }

        // insert env vars as default with min val for ints
        private static Option<int> EnvVarOption(string[] names, string description, int defaultValue, int minValue, int? maxValue = null)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            int value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (!int.TryParse(env, out value))
                {
                    EnvVarErrors.Add($"Environment variable {key} is invalid");
                }
            }

            Option<int> opt = new (names, () => value, description);

            opt.AddValidator((res) =>
            {
                string s = string.Empty;
                int val;

                try
                {
                    val = (int)res.GetValueOrDefault();

                    if (val < minValue)
                    {
                        s = $"{names[0]} must be >= {minValue}";
                    }
                }
                catch
                {
                }

                return s;
            });

            if (maxValue != null)
            {
                opt.AddValidator((res) =>
                {
                    string s = string.Empty;
                    int val;

                    try
                    {
                        val = (int)res.GetValueOrDefault();

                        if (val > maxValue)
                        {
                            s = $"{names[0]} must be <= {maxValue}";
                        }
                    }
                    catch
                    {
                    }

                    return s;
                });
            }

            return opt;
        }

        // check for environment variable value
        private static string GetValueFromEnvironment(string[] names, out string key)
        {
            if (names == null ||
                names.Length < 1 ||
                names[0].Trim().Length < 4)
            {
                throw new ArgumentNullException(nameof(names));
            }

            for (int i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]) ||
                    names[i].Length != 2 ||
                    names[i][0] != '-')
                {
                    throw new ArgumentException($"Invalid command line parameter at position {i}", nameof(names));
                }
            }

            key = names[0][2..].Trim().ToUpperInvariant().Replace('-', '_');

            return Environment.GetEnvironmentVariable(key);
        }

        // set config values from command line
        private static void SetConfig(Config config)
        {
            // copy command line values
            Config.SetConfig(config);

            // create data access layer
            if (Config.AppType == AppType.App)
            {
                LoadSecrets();

                // create the cosmos data access layer
                Config.CosmosDal = new DataAccessLayer.CosmosDal(Config.Secrets, Config);
            }

            SetLoggerConfig();
        }

        // set the logger config
        private static void SetLoggerConfig()
        {
            RequestLogger.CosmosName = Config.CosmosName;
            NgsaLog.LogLevel = Config.LogLevel;
        }
    }
}
