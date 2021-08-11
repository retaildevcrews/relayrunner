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
//        /// Retrieve a single LoadClient from CosmosDB by genericId
//        ///
//        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
//        ///
//        /// Throws an exception if not found
//        /// </summary>
//        /// <param name="genericId">LoadClient ID</param>
//        /// <returns>LoadClient object</returns>
//        public async Task<LoadClient> GetGenericAsync(string genericId)
//        {
//            if (string.IsNullOrWhiteSpace(genericId))
//            {
//                throw new ArgumentNullException(nameof(genericId));
//            }

//            string key = $"/api/generic/{genericId.ToLowerInvariant().Trim()}";

//            if (App.Config.Cache && cache.Contains(key) && cache.Get(key) is LoadClient gc)
//            {
//                return gc;
//            }

//            // get the partition key for the generic ID
//            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
//            // ComputePartitionKey will throw an ArgumentException if the genericId isn't valid
//            // get a generic by ID

//            LoadClient g = await cosmosDetails.Container.ReadItemAsync<LoadClient>(genericId, new PartitionKey(LoadClient.ComputePartitionKey(genericId))).ConfigureAwait(false);

//            if (App.Config.Cache)
//            {
//                cache.Add(new CacheItem(key, g), cachePolicy);
//            }

//            return g;
//        }

//        public async Task<IEnumerable<LoadClient>> GetGenericsAsync(GenericQueryParameters genericQueryParameters)
//        {
//            if (genericQueryParameters == null)
//            {
//                throw new ArgumentNullException(nameof(genericQueryParameters));
//            }

//            string key = genericQueryParameters.GetKey();

//            if (App.Config.Cache && cache.Contains(key) && cache.Get(key) is List<LoadClient> g)
//            {
//                return g;
//            }

//            string sql = App.Config.CacheDal.GetGenericIds(genericQueryParameters);

//            List<LoadClient> generic = new List<LoadClient>();

//            // retrieve the items
//            if (!string.IsNullOrWhiteSpace(sql))
//            {
//                generic = (List<LoadClient>)await InternalCosmosDBSqlQuery<LoadClient>(sql).ConfigureAwait(false);
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
//        /// <param name="generic">LoadClient to upsert</param>
//        /// <returns>LoadClient</returns>
//        public async Task<LoadClient> UpsertGenericAsync(LoadClient generic)
//        {
//            ItemResponse<LoadClient> response = await cosmosDetails.Container.UpsertItemAsync(generic, new PartitionKey(generic.PartitionKey));

//            return response.Resource;
//        }

//        /// <summary>
//        /// Delete a generic by Id
//        /// </summary>
//        /// <param name="genericId">LoadClient ID</param>
//        /// <returns>void</returns>
//        public async Task DeleteGenericAsync(string genericId)
//        {
//            try
//            {
//                await cosmosDetails.Container.DeleteItemAsync<LoadClient>(genericId, new PartitionKey(LoadClient.ComputePartitionKey(genericId)));
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
