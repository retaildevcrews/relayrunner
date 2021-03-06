// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using RelayRunner.Application.Data;
using RelayRunner.Application.DataAccessLayer;

namespace RelayRunner.Application
{
    /// <summary>
    /// Configurations for App
    /// </summary>
    public class Config
    {
        public string SecretsVolume { get; set; } = "secrets";
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public string CosmosName { get; set; } = string.Empty;
        public bool IsLogLevelSet { get; set; }
        public Secrets Secrets { get; set; }
        public int Port { get; set; } = 8080;
        public int Retries { get; set; } = 10;
        public int Timeout { get; set; } = 10;
        public LogLevel RequestLogLevel { get; set; } = LogLevel.Information;
        public IDAL CosmosDal { get; set; }
        public ICache Cache { get; set; }
        public string UrlPrefix { get; set; }

        /// <summary>
        /// Set configs
        /// </summary>
        /// <param name="config">Config</param>
        public void SetConfig(Config config)
        {
            IsLogLevelSet = config.IsLogLevelSet;
            Secrets = config.Secrets;
            Port = config.Port;
            Retries = config.Retries;
            Timeout = config.Timeout;
            CosmosDal = config.CosmosDal;
            Cache = config.Cache;
            UrlPrefix = string.IsNullOrWhiteSpace(config.UrlPrefix) ? string.Empty : config.UrlPrefix;

            // remove trailing / if present
            if (UrlPrefix.EndsWith('/'))
            {
                UrlPrefix = UrlPrefix[0..^1];
            }

            // LogLevel.Information is the min
            LogLevel = config.LogLevel <= LogLevel.Information ? LogLevel.Information : config.LogLevel;
            RequestLogLevel = config.RequestLogLevel <= LogLevel.Information ? LogLevel.Information : config.RequestLogLevel;

            // clean up string values
            SecretsVolume = string.IsNullOrWhiteSpace(config.SecretsVolume) ? string.Empty : config.SecretsVolume.Trim();
            CosmosName = string.IsNullOrWhiteSpace(config.CosmosName) ? string.Empty : config.CosmosName.Trim();
        }
    }
}
