// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Database.Model;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
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
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;

        private const string LoadClientSQL = "select g.name, g.id, g.partitionKey, g.region, g.zone, g.scheduler, g.metrics, g.status, g.dateCreated from g";

        // benchmark results buffer
        private readonly string benchmarkData;

        // Lucene in-memory index
        private readonly IndexWriter writer = new IndexWriter(new RAMDirectory(), new IndexWriterConfig(Version, new StandardAnalyzer(Version)));

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDal"/> class.
        /// </summary>
        public InMemoryDal()
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            // temporary storage for upsert / delete
            LoadClientIndex = new Dictionary<string, LoadClient>();

            // 16 bytes
            benchmarkData = "0123456789ABCDEF";

            // 1 MB
            while (benchmarkData.Length < 1024 * 1024)
            {
                benchmarkData += benchmarkData;
            }

            // TODO: Do we need this?

            //// load load client into Lucene index
            //foreach (LoadClient loadClient in LoadGeneric(jsonOptions))
            //{
            //    writer.AddDocument(loadClient.ToDocument());
            //}

            //// flush the writes to the index
            //writer.Flush(true, true);
        }

        // used for upsert / delete
        public static Dictionary<string, LoadClient> LoadClientIndex { get; set; }

        public async Task<string> GetBenchmarkDataAsync(int size)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return benchmarkData[0..size];
            }).ConfigureAwait(false);
        }

        // TODO

        /// <summary>
        /// Get Load Client by ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClient</returns>
        //public async Task<LoadClient> GetLoadClientAsync(string id)
        //{
        //    return await Task<LoadClient>.Factory.StartNew(() => { return GetLoadClients(id); }).ConfigureAwait(false);
        //}

        /// <summary>
        /// Get LoadClient By ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>LoadClient</returns>
        public LoadClient GetGeneric(string id)
        {
            // TODO: update when decide id format
            if (id.StartsWith("tt"))
            {
                IndexSearcher searcher = new IndexSearcher(writer.GetReader(true));

                // search by id
                TopDocs hits = searcher.Search(new PhraseQuery { new Term("id", id) }, 1);

                if (hits.TotalHits > 0)
                {
                    // deserialize the json from the index
                    return JsonSerializer.Deserialize<LoadClient>(searcher.Doc(hits.ScoreDocs[0].Doc).GetBinaryValue("json").Bytes);
                }
                else
                {
                    // handle the upserted loadClient
                    if (LoadClientIndex.ContainsKey(id))
                    {
                        return LoadClientIndex[id];
                    }
                }
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

            // TODO: requires internal object id

            //foreach (LoadClient g in cache)
            //{
            //    ids += $"'{g.id}',";
            //}

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

            IndexSearcher searcher = new IndexSearcher(writer.GetReader(true));

            // type = LoadClient
            BooleanQuery bq = new BooleanQuery
            {
                { new PhraseQuery { new Term("type", "LoadClient") }, Occur.MUST },
            };

            // propertySort == property.ToLower()
            // TODO: Replace 100 numHits limit
            TopFieldCollector collector = TopFieldCollector.Create(new Sort(new SortField("propertySort", SortFieldType.STRING)), 100, false, false, false, false);

            // add the search term
            if (!string.IsNullOrWhiteSpace(q))
            {
                bq.Add(new WildcardQuery(new Term("property", $"*{q.ToLowerInvariant()}*")), Occur.MUST);
            }

            // run the search
            searcher.Search(bq, collector);

            TopDocs results = collector.GetTopDocs();

            // deserialize json from document
            for (int i = 0; i < results.ScoreDocs.Length; i++)
            {
                res.Add(JsonSerializer.Deserialize<LoadClient>(searcher.Doc(results.ScoreDocs[i].Doc).GetBinaryValue("json").Bytes));
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
            // TODO: needs internal object id

            await Task.Run(() =>
            {
                //    if (LoadClientIndex.ContainsKey(loadClient.id))
                //    {
                //        loadClient = LoadClientIndex[loadClient.id];
                //    }
                //    else
                //    {
                //        LoadClientIndex.Add(loadClient.id, loadClient);
                //    }
            }).ConfigureAwait(false);

            return loadClient;
        }

        /// <summary>
        /// Delete the loadClient from temporary storage
        /// </summary>
        /// <param name="id">LoadClient ID</param>
        /// <returns>void</returns>
        public async Task DeleteLoadClientsAsync(string id)
        {
            await Task.Run(() =>
            {
                if (LoadClientIndex.ContainsKey(id))
                {
                    LoadClientIndex.Remove(id);
                }
            }).ConfigureAwait(false);
        }

        public Task<LoadClient> GetLoadClientAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LoadClient>> GetLoadClientAsync(LoadClientQueryParameters loadClientQueryParameters)
        {
            throw new NotImplementedException();
        }

        public Task DeleteLoadClientAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<LoadClient> UpsertLoadClientAsync(LoadClient loadClient)
        {
            throw new NotImplementedException();
        }

        // load LoadClient List from json file
        private static List<LoadClient> LoadGeneric(JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<List<LoadClient>>(File.ReadAllText("src/data/loadClient.json"), options);
        }
    }
}
