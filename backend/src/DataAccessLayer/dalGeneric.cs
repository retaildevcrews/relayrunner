// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// TODO: COSMOS

//using System;
//using System.Collections.Generic;
//using System.Runtime.Caching;
//using System.Threading.Tasks;
//using Database.Model;
//using Microsoft.Azure.Cosmos;
//using RelayRunner.Middleware;

//namespace RelayRunner.Application.DataAccessLayer
//{
//    /// <summary>
//    /// Data Access Layer for CosmosDB
//    /// </summary>
//    public partial class CosmosDal
//    {
//        /// <summary>
//        /// Retrieve a single LoadClients from CosmosDB by genericId
//        ///
//        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
//        ///
//        /// Throws an exception if not found
//        /// </summary>
//        /// <param name="genericId">LoadClients ID</param>
//        /// <returns>LoadClients object</returns>
//        public async Task<LoadClients> GetGenericAsync(string genericId)
//        {
//            if (string.IsNullOrWhiteSpace(genericId))
//            {
//                throw new ArgumentNullException(nameof(genericId));
//            }

//            string key = $"/api/generic/{genericId.ToLowerInvariant().Trim()}";

//            if (App.Config.Cache && cache.Contains(key) && cache.Get(key) is LoadClients gc)
//            {
//                return gc;
//            }

//            // get the partition key for the generic ID
//            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
//            // ComputePartitionKey will throw an ArgumentException if the genericId isn't valid
//            // get a generic by ID

//            LoadClients g = await cosmosDetails.Container.ReadItemAsync<LoadClients>(genericId, new PartitionKey(LoadClients.ComputePartitionKey(genericId))).ConfigureAwait(false);

//            if (App.Config.Cache)
//            {
//                cache.Add(new CacheItem(key, g), cachePolicy);
//            }

//            return g;
//        }

//        public async Task<IEnumerable<LoadClients>> GetGenericsAsync(GenericQueryParameters genericQueryParameters)
//        {
//            if (genericQueryParameters == null)
//            {
//                throw new ArgumentNullException(nameof(genericQueryParameters));
//            }

//            string key = genericQueryParameters.GetKey();

//            if (App.Config.Cache && cache.Contains(key) && cache.Get(key) is List<LoadClients> g)
//            {
//                return g;
//            }

//            string sql = App.Config.CacheDal.GetGenericIds(genericQueryParameters);

//            List<LoadClients> generic = new List<LoadClients>();

//            // retrieve the items
//            if (!string.IsNullOrWhiteSpace(sql))
//            {
//                generic = (List<LoadClients>)await InternalCosmosDBSqlQuery<LoadClients>(sql).ConfigureAwait(false);
//            }

//            if (App.Config.Cache)
//            {
//                // add to cache
//                cache.Add(new CacheItem(key, generic), cachePolicy);
//            }

//            return generic;
//        }

//        /// <summary>
//        /// upsert a generic
//        /// </summary>
//        /// <param name="generic">LoadClients to upsert</param>
//        /// <returns>LoadClients</returns>
//        public async Task<LoadClients> UpsertGenericAsync(LoadClients generic)
//        {
//            ItemResponse<LoadClients> response = await cosmosDetails.Container.UpsertItemAsync(generic, new PartitionKey(generic.PartitionKey));

//            return response.Resource;
//        }

//        /// <summary>
//        /// Delete a generic by Id
//        /// </summary>
//        /// <param name="genericId">LoadClients ID</param>
//        /// <returns>void</returns>
//        public async Task DeleteGenericAsync(string genericId)
//        {
//            try
//            {
//                await cosmosDetails.Container.DeleteItemAsync<LoadClients>(genericId, new PartitionKey(LoadClients.ComputePartitionKey(genericId)));
//            }
//            catch (CosmosException cex)
//            {
//                // ignore 404 errors
//                if (cex.StatusCode != System.Net.HttpStatusCode.NotFound)
//                {
//                    Console.WriteLine(cex.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }
//    }
//}
