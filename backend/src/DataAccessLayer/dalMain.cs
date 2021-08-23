// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
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
                CosmosLease = secrets.CosmosLease,
                CosmosDatabase = secrets.CosmosDatabase,
                CosmosKey = secrets.CosmosKey,
                CosmosUrl = secrets.CosmosServer,
                Retries = config.Retries,
                Timeout = config.Timeout,
            };

            // create the CosmosDB client and source container
            cosmosDetails.Client = new CosmosClient(secrets.CosmosServer, secrets.CosmosKey, cosmosDetails.CosmosClientOptions);
            cosmosDetails.SourceContainer = cosmosDetails.Client.GetContainer(secrets.CosmosDatabase, secrets.CosmosCollection);
            cosmosDetails.LeaseContainer = cosmosDetails.Client.GetContainer(secrets.CosmosDatabase, secrets.CosmosLease);
            cosmosDetails.ChangeFeedProcessor = StartChangeFeedProcessorAsync(cosmosDetails).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Start the Change Feed Processor to listen for changes and process them with the HandleChangesAsync implementation
        /// </summary>
        /// <param name="cosmosDetails">Cosmos details</param>
        /// <returns>ChangeFeedProcessor</returns>
        private static async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(CosmosConfig cosmosDetails)
        {
            ChangeFeedProcessor changeFeedProcessor = cosmosDetails.SourceContainer
                .GetChangeFeedProcessorBuilder<ClientStatus>(processorName: "clientStatusProcessor", onChangesDelegate: HandleChangesAsync)
                .WithInstanceName("clientStatusProcessorInstance")
                .WithLeaseContainer(cosmosDetails.LeaseContainer)
                .Build();

            Console.WriteLine("Starting Change Feed Processor...");
            await changeFeedProcessor.StartAsync();
            Console.WriteLine("Change Feed Processor started.");
            return changeFeedProcessor;
        }

        /// <summary>
        /// The delegate receives batches of changes as they are generated in the change feed and can process them
        /// </summary>
        private static async Task HandleChangesAsync(IReadOnlyCollection<ClientStatus> changes, CancellationToken cancellationToken)
        {
            Console.WriteLine("Started handling changes...");
            foreach (ClientStatus item in changes)
            {
                Console.WriteLine($"Detected operation for item with id {item.Id}, last updated at {item.LastUpdated}.");
                await Task.Delay(10);
            }

            Console.WriteLine("Finished handling changes.");
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
