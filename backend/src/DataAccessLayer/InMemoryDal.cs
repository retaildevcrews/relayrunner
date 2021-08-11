// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// TODO: Delete file and references when CosmosDb is enabled

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Database.Model;
using Microsoft.Azure.Cosmos;
using RelayRunner.Middleware;

namespace RelayRunner.Application.DataAccessLayer
{
    /// <summary>
    /// This code is used to support performance testing
    ///
    /// This loads the Database data into memory which removes the roundtrip to Cosmos
    /// This provides higher performance and less variability which allows us to establish
    /// baseline performance metrics
    /// </summary>
    public class InMemoryDal : IDAL
    {
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

        public static List<LoadClient> LoadClients { get; set; }

        // O(1) dictionary for retrieving by ID
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
        /// Get List of LoadClients by search params
        /// </summary>
        /// <returns>List of LoadClient</returns>
        public List<LoadClient> GetLoadClients()
        {
            List<LoadClient> res = new List<LoadClient>();
            foreach (LoadClient l in LoadClients)
            {
                res.Add(l);
            }

            return res;
        }

        /// <summary>
        /// Get list of LoadClients based on query parameters
        /// </summary>
        /// <returns>List of LoadClient</returns>
        public Task<IEnumerable<LoadClient>> GetLoadClientsAsync()
        {
            return Task<IEnumerable<LoadClient>>.Factory.StartNew(GetLoadClients);
        }

        private static void LoadLoadClients(JsonSerializerOptions settings)
        {
            if (LoadClients?.Count != null)
            {
                return;
            }

            // load the data from the json file
            LoadClients = JsonSerializer.Deserialize<List<LoadClient>>(File.ReadAllText("src/data/loadClients.json"), settings);
            if (LoadClients == null)
            {
                return;
            }

            foreach (LoadClient l in LoadClients)
            {
                // Loads an O(1) dictionary for retrieving by ID
                // Could also use a binary search to reduce memory usage
                LoadClientsIndex.Add(l.Id, l);
            }
        }
    }
}
