// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Database.Model;
using Microsoft.Azure.Cosmos;
using RelayRunner.Middleware;

/// <summary>
/// This code is used to support performance testing
///
/// This loads the Database data into memory which removes the roundtrip to Cosmos
/// This provides higher performance and less variability which allows us to establish
/// baseline performance metrics
/// </summary>
namespace RelayRunner.Application.DataAccessLayer
{
    public class InMemoryDal : IDAL
    {
        private const string LoadClientSQL = "select g.name, g.id, g.partitionKey, g.region, g.zone, g.scheduler, g.metrics, g.status, g.dateCreated from g";

        // benchmark results buffer
        private readonly string benchmarkData;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDal"/> class.
        /// </summary>
        public InMemoryDal()
        {
            JsonSerializerOptions settings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            // load the data
            LoadLoadClients(settings);

            // 16 bytes
            benchmarkData = "0123456789ABCDEF";

            // 1 MB
            while (benchmarkData.Length < 1024 * 1024)
            {
                benchmarkData += benchmarkData;
            }
        }

        public async Task<string> GetBenchmarkDataAsync(int size)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return benchmarkData[0..size];
            }).ConfigureAwait(false);
        }

        public static List<LoadClient> LoadClients { get; set; }

        // O(1) dictionary for retriving by ID
        public static Dictionary<string, LoadClient> LoadClientsIndex { get; set; } = new Dictionary<string, LoadClient>();

        /// <summary>
        /// Get Load Client by ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClient</returns>
        public async Task<LoadClient> GetLoadClientAsync(string id)
        {
            return await Task<LoadClient>.Factory.StartNew(() => { return GetLoadClient(id); }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get LoadClient By ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClient</returns>
        public LoadClient GetLoadClient(string id)
        {
            if (LoadClientsIndex.ContainsKey(id))
            {
                return LoadClientsIndex[id];
            }

            throw new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 404, string.Empty, 0);
        }

        /// <summary>
        /// Get Cosmos query string based on query parameters
        /// </summary>
        /// <param name="loadClientQueryParameters">query params</param>
        /// <returns>Cosmos query string</returns>
        public string GetLoadClientIds(LoadClientQueryParameters loadClientQueryParameters)
        {
            List<LoadClient> cache;
            string ids = string.Empty;

            if (loadClientQueryParameters == null)
            {
                cache = GetLoadClients(string.Empty);
            }
            else
            {
                cache = GetLoadClients(loadClientQueryParameters.Q);
            }

            // TODO: depends internal object id

            foreach (LoadClient g in cache)
            {
                ids += $"'{g.LoadClientId}',";
            }

            // nothing found
            if (string.IsNullOrWhiteSpace(ids))
            {
                return string.Empty;
            }

            return LoadClientSQL.Replace("{0}", ids[0..^1], StringComparison.Ordinal);
        }

        /// <summary>
        /// Get list of LoadClient based on query parameters
        /// </summary>
        /// <param name="loadClientQueryParameters">query params</param>
        /// <returns>List of LoadClient</returns>
        public List<LoadClient> GetLoadClients(LoadClientQueryParameters loadClientQueryParameters)
        {
            if (loadClientQueryParameters == null)
            {
                return GetLoadClients(string.Empty);
            }

            return GetLoadClients(loadClientQueryParameters.Q);
        }

        /// <summary>
        /// Get List of LoadClient by search params
        /// </summary>
        /// <param name="q">match property</param>
        /// <returns>List of LoadClient</returns>
        public List<LoadClient> GetLoadClients(string q)
        {
            List<LoadClient> res = new List<LoadClient>();
            foreach (LoadClient l in LoadClients)
            {
                res.Add(l);
            }

            return res;
        }

        /// <summary>
        /// Get list of LoadClient based on query parameters
        /// </summary>
        /// <param name="loadClientQueryParameters">query params</param>
        /// <returns>List of LoadClient</returns>
        public Task<IEnumerable<LoadClient>> GetLoadClientsAsync(LoadClientQueryParameters loadClientQueryParameters)
        {
            return Task<IEnumerable<LoadClient>>.Factory.StartNew(() =>
            {
                return GetLoadClients(loadClientQueryParameters);
            });
        }

        /// <summary>
        /// Upsert a load client
        ///
        /// Do not store in index or WebV tests will break
        /// </summary>
        /// <param name="loadClient">LoadClient to upsert</param>
        /// <returns>LoadClient</returns>
        public async Task<LoadClient> UpsertLoadClientsAsync(LoadClient loadClient)
        {
            // TODO: depends on internal object id
            {
                await Task.Run(() =>
                {
                    if (LoadClientsIndex.ContainsKey(loadClient.LoadClientId))
                    {
                        loadClient = LoadClientsIndex[loadClient.LoadClientId];
                    }
                    else
                    {
                        LoadClientsIndex.Add(loadClient.LoadClientId, loadClient);
                    }
                }).ConfigureAwait(false);

                return loadClient;
            }
        }

        /// <summary>
        /// Delete the loadClient from temporary storage
        /// </summary>
        /// <param name="loadClientId">LoadClient ID</param>
        /// <returns>void</returns>
        public async Task DeleteLoadClientAsync(string loadClientId)
        {
            await Task.Run(() =>
            {
                if (LoadClientsIndex.ContainsKey(loadClientId))
                {
                    LoadClientsIndex.Remove(loadClientId);
                }
            }).ConfigureAwait(false);
        }

        public Task<LoadClient> UpsertLoadClientAsync(LoadClient loadClient)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LoadClient>> GetLoadClientAsync(LoadClientQueryParameters loadClientQueryParameters)
        {
            throw new NotImplementedException();
        }

        private static void LoadLoadClients(JsonSerializerOptions settings)
        {
            if (LoadClients?.Count == null)
            {
                // load the data from the json file
                LoadClients = JsonSerializer.Deserialize<List<LoadClient>>(File.ReadAllText("src/data/loadClients.json"), settings);
            }

            if (LoadClientsIndex.Count == 0)
            {
                foreach (LoadClient l in LoadClients)
                {
                    // Loads an O(1) dictionary for retrieving by ID
                    // Could also use a binary search to reduce memory usage
                    LoadClientsIndex.Add(l.LoadClientId, l);
                }
            }
        }
    }
}
