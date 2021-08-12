// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using RelayRunner.Model;

namespace RelayRunner.Application.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class CosmosDal : IDAL, IDisposable
    {
        private readonly MemoryCache cache = new MemoryCache("cache");
        private readonly CosmosConfig cosmosDetails;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDal"/> class.
        /// </summary>
        /// <param name="secrets">Cosmos connection info</param>
        /// <param name="config">App config</param>
        public CosmosDal(Secrets secrets, Config config)
        {
            if (secrets == null)
            {
                throw new ArgumentNullException(nameof(secrets));
            }

            cosmosDetails = new CosmosConfig
            {
                CosmosCollection = secrets.CosmosCollection,
                CosmosDatabase = secrets.CosmosDatabase,
                CosmosKey = secrets.CosmosKey,
                CosmosUrl = secrets.CosmosServer,
                Retries = config.Retries,
                Timeout = config.Timeout,
            };

            // create the CosmosDB client and container
            cosmosDetails.Client = OpenAndTestCosmosClient(secrets.CosmosServer, secrets.CosmosKey, secrets.CosmosDatabase, secrets.CosmosCollection).GetAwaiter().GetResult();
            cosmosDetails.Container = cosmosDetails.Client.GetContainer(secrets.CosmosDatabase, secrets.CosmosCollection);
        }

        /// <summary>
        /// Return the generic Cosmos DB results
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="query">Cosmos Query</param>
        /// <returns>IEnumerable T</returns>
        private static async Task<IEnumerable<T>> InternalCosmosDbResults<T>(FeedIterator<T> query)
        {
            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                foreach (T doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Open and test the Cosmos Client / Container / Query
        /// </summary>
        /// <param name="cosmosServer">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <returns>An open and validated CosmosClient</returns>
        private async Task<CosmosClient> OpenAndTestCosmosClient(string cosmosServer, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // validate required parameters
            if (cosmosServer == null)
            {
                throw new ArgumentNullException(nameof(cosmosServer));
            }

            if (string.IsNullOrWhiteSpace(cosmosKey))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosKey not set correctly {cosmosKey}"));
            }

            if (string.IsNullOrWhiteSpace(cosmosDatabase))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosDatabase not set correctly {cosmosDatabase}"));
            }

            if (string.IsNullOrWhiteSpace(cosmosCollection))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosCollection not set correctly {cosmosCollection}"));
            }

            // open and test a new client / container
            CosmosClient c = new CosmosClient(cosmosServer, cosmosKey, cosmosDetails.CosmosClientOptions);
            Container con = c.GetContainer(cosmosDatabase, cosmosCollection);
            await con.ReadItemAsync<dynamic>("action", new PartitionKey("0")).ConfigureAwait(false);

            return c;
        }

        // implement IDisposable
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "clarity")]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (cache != null)
                    {
                        cache.Dispose();
                    }
                }

                disposedValue = true;
            }
        }
    }
}
