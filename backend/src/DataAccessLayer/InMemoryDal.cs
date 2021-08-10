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
        }

        public static List<LoadClients> LoadClients { get; set; }

        // O(1) dictionary for retriving by ID
        public static Dictionary<string, LoadClients> LoadClientsIndex { get; set; } = new Dictionary<string, LoadClients>();

        /// <summary>
        /// Get Load Client by ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClients</returns>
        public async Task<LoadClients> GetLoadClientAsync(string id)
        {
            return await Task<LoadClients>.Factory.StartNew(() => { return GetLoadClient(id); }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get LoadClients By ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClients</returns>
        public LoadClients GetLoadClient(string id)
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
            List<LoadClients> cache;
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

            foreach (LoadClients g in cache)
            {
                ids += $"'{g.Id}',";
            }

            // nothing found
            if (string.IsNullOrWhiteSpace(ids))
            {
                return string.Empty;
            }

            return LoadClientSQL.Replace("{0}", ids[0..^1], StringComparison.Ordinal);
        }

        /// <summary>
        /// Get list of LoadClients based on query parameters
        /// </summary>
        /// <param name="loadClientQueryParameters">query params</param>
        /// <returns>List of LoadClients</returns>
        public List<LoadClients> GetLoadClients(LoadClientQueryParameters loadClientQueryParameters)
        {
            if (loadClientQueryParameters == null)
            {
                return GetLoadClients(string.Empty);
            }

            return GetLoadClients(loadClientQueryParameters.Q);
        }

        /// <summary>
        /// Get List of LoadClients by search params
        /// </summary>
        /// <param name="q">match property</param>
        /// <returns>List of LoadClients</returns>
        public List<LoadClients> GetLoadClients(string q)
        {
            List<LoadClients> res = new List<LoadClients>();
            foreach (LoadClients l in LoadClients)
            {
                res.Add(l);
            }

            return res;
        }

        /// <summary>
        /// Get list of LoadClients based on query parameters
        /// </summary>
        /// <param name="loadClientQueryParameters">query params</param>
        /// <returns>List of LoadClients</returns>
        public Task<IEnumerable<LoadClients>> GetLoadClientsAsync(LoadClientQueryParameters loadClientQueryParameters)
        {
            return Task<IEnumerable<LoadClients>>.Factory.StartNew(() =>
            {
                return GetLoadClients(loadClientQueryParameters);
            });
        }

        /// <summary>
        /// Upsert a load client
        ///
        /// Do not store in index or WebV tests will break
        /// </summary>
        /// <param name="loadClient">LoadClients to upsert</param>
        /// <returns>LoadClients</returns>
        public async Task<LoadClients> UpsertLoadClientsAsync(LoadClients loadClient)
        {
            // TODO: depends on internal object id
            {
                await Task.Run(() =>
                {
                    if (LoadClientsIndex.ContainsKey(loadClient.Id))
                    {
                        loadClient = LoadClientsIndex[loadClient.Id];
                    }
                    else
                    {
                        LoadClientsIndex.Add(loadClient.Id, loadClient);
                    }
                }).ConfigureAwait(false);

                return loadClient;
            }
        }

        /// <summary>
        /// Delete the loadClient from temporary storage
        /// </summary>
        /// <param name="id">LoadClients ID</param>
        /// <returns>void</returns>
        public async Task DeleteLoadClientAsync(string id)
        {
            await Task.Run(() =>
            {
                if (LoadClientsIndex.ContainsKey(id))
                {
                    LoadClientsIndex.Remove(id);
                }
            }).ConfigureAwait(false);
        }

        public Task<LoadClients> UpsertLoadClientAsync(LoadClients loadClient)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LoadClients>> GetLoadClientAsync(LoadClientQueryParameters loadClientQueryParameters)
        {
            throw new NotImplementedException();
        }

        private static void LoadLoadClients(JsonSerializerOptions settings)
        {
            if (LoadClients?.Count == null)
            {
                // load the data from the json file
                LoadClients = JsonSerializer.Deserialize<List<LoadClients>>(File.ReadAllText("src/data/loadClients.json"), settings);
            }

            if (LoadClientsIndex.Count == 0)
            {
                foreach (LoadClients l in LoadClients)
                {
                    // Loads an O(1) dictionary for retrieving by ID
                    // Could also use a binary search to reduce memory usage
                    LoadClientsIndex.Add(l.Id, l);
                }
            }
        }
    }
}
