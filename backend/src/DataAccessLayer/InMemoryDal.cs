// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// TODO: Delete file and references when CosmosDb is enabled

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using RelayRunner.Model;

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
            string loadClientsPath = "src/data/loadClients.json";
            SetLoadClientsFromFile(settings, loadClientsPath);
        }

        public static List<LoadClient> LoadClients { get; set; }

        // O(1) dictionary for retrieving by ID
        public static Dictionary<string, LoadClient> LoadClientsIndex { get; set; } = new Dictionary<string, LoadClient>();

        /// <summary>
        /// Get Load Client by ID
        /// </summary>
        /// <param name="loadClientId">ID</param>
        /// <returns>LoadClient</returns>
        public async Task<LoadClient> GetLoadClientByIdAsync(string loadClientId)
        {
            return await Task<LoadClient>.Factory.StartNew(() => GetLoadClientById(loadClientId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get LoadClient By ID
        /// </summary>
        /// <param name="loadClientId">ID</param>
        /// <returns>LoadClient</returns>
        public LoadClient GetLoadClientById(string loadClientId)
        {
            if (LoadClientsIndex.ContainsKey(loadClientId))
            {
                return LoadClientsIndex[loadClientId];
            }

            throw new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 404, string.Empty, 0);
        }

        /// <summary>
        /// Get List of LoadClients
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
        /// Get list of LoadClients
        /// </summary>
        /// <returns>List of LoadClient</returns>
        public Task<IEnumerable<LoadClient>> GetLoadClientsAsync()
        {
            return Task<IEnumerable<LoadClient>>.Factory.StartNew(GetLoadClients);
        }

        private static void SetLoadClientsFromFile(JsonSerializerOptions settings, string path)
        {
            if (LoadClients?.Count == null)
            {
                // load the data from the json file
                LoadClients = JsonSerializer.Deserialize<List<LoadClient>>(File.ReadAllText(path), settings);
                if (LoadClients != null)
                {
                    foreach (LoadClient l in LoadClients)
                    {
                        // Loads an O(1) dictionary for retrieving by ID
                        // Could also use a binary search to reduce memory usage
                        LoadClientsIndex.Add(l.Id, l);
                    }
                }
            }
        }
    }
}
