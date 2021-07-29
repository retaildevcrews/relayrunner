// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Database.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ngsa.Application.Model;
using Ngsa.Middleware;

namespace Ngsa.Application
{
    /// <summary>
    /// Cosmos Health Check
    /// </summary>
    public partial class CosmosHealthCheck : IHealthCheck
    {
        private const int MaxResponseTime = 200;
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Build the response
        /// </summary>
        /// <param name="uri">string</param>
        /// <param name="targetDurationMs">double (ms)</param>
        /// <param name="ex">Exception (default = null)</param>
        /// <param name="data">Dictionary(string, object)</param>
        /// <param name="testName">Test Name</param>
        /// <returns>HealthzCheck</returns>
        private HealthzCheck BuildHealthzCheck(string uri, double targetDurationMs, Exception ex = null, Dictionary<string, object> data = null, string testName = null)
        {
            stopwatch.Stop();

            // create the result
            HealthzCheck result = new HealthzCheck
            {
                Endpoint = uri,
                Status = HealthStatus.Healthy,
                Duration = stopwatch.Elapsed,
                TargetDuration = new System.TimeSpan(0, 0, 0, 0, (int)targetDurationMs),
                ComponentId = testName,
                ComponentType = "datastore",
            };

            // check duration
            if (result.Duration.TotalMilliseconds > targetDurationMs)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = HealthzCheck.TimeoutMessage;
            }

            // add the exception
            if (ex != null)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Message = ex.Message;
            }

            // add the results to the dictionary
            if (data != null && !string.IsNullOrEmpty(testName))
            {
                data.Add(testName + ":responseTime", result);
            }

            return result;
        }

        /// <summary>
        /// Get Generic by Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetGenericByIdAsync(string genericId, Dictionary<string, object> data = null)
        {
            const string name = "getGenericById";
            string path = "/api/generic/" + genericId;

            stopwatch.Restart();

            try
            {
                if (App.Config.AppType == AppType.App)
                {
                    _ = await dal.GetGenericAsync(genericId).ConfigureAwait(false);
                }

                return BuildHealthzCheck(path, MaxResponseTime / 2, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime / 2, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Search Generic Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchGenericsAsync(string query, Dictionary<string, object> data = null)
        {
            const string name = "searchGeneric";

            GenericQueryParameters genericQuery = new GenericQueryParameters { Q = query };

            string path = "/api/generic?q=" + genericQuery.Q;

            stopwatch.Restart();

            try
            {
                if (App.Config.AppType == AppType.App)
                {
                    _ = (await dal.GetGenericsAsync(genericQuery).ConfigureAwait(false)).ToList<Generic>();
                }

                return BuildHealthzCheck(path, MaxResponseTime, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }
    }
}
