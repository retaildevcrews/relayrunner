// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using RelayRunner.Model;

namespace RelayRunner.Application.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class CosmosDal
    {
        /// <summary>
        /// Retrieve a single LoadClient from CosmosDB by loadClientId
        ///
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        ///
        /// Throws an exception if not found
        /// </summary>
        /// <param name="loadClientId">LoadClient ID</param>
        /// <returns>LoadClient object</returns>
        public async Task<LoadClient> GetLoadClientByIdAsync(string loadClientId)
        {
            if (string.IsNullOrWhiteSpace(loadClientId))
            {
                throw new ArgumentNullException(nameof(loadClientId));
            }

            // get the partition key for the loadClientId
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            // ComputePartitionKey will throw an ArgumentException if the loadClientId isn't valid
            // get a load client by ID

            LoadClient g = await cosmosDetails.SourceContainer
                .ReadItemAsync<LoadClient>(loadClientId, new PartitionKey(LoadClient.ComputePartitionKey(loadClientId)))
                .ConfigureAwait(false);

            return g;
        }

        public async Task<IEnumerable<LoadClient>> GetLoadClientsAsync()
        {
            // create query
            QueryDefinition sql = new QueryDefinition("select * from loadClients");

            // run query
            FeedIterator<LoadClient> query = cosmosDetails.SourceContainer.GetItemQueryIterator<LoadClient>(sql);

            // return results
            return await InternalCosmosDbResults(query).ConfigureAwait(false);
        }
    }
}
