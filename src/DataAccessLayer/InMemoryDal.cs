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
using Ngsa.Middleware;

/// <summary>
/// This code is used to support performance testing
///
/// This loads the Database data into memory which removes the roundtrip to Cosmos
/// This provides higher performance and less variability which allows us to establish
/// baseline performance metrics
/// </summary>
namespace Ngsa.Application.DataAccessLayer
{
    public class InMemoryDal : IDAL
    {
        private const LuceneVersion Version = LuceneVersion.LUCENE_48;

        private const string GenericSQL = "select g.id, g.partitionKey, g.genericId, g.type, g.property, g.textSearch from g where g.id in ({0}) order by g.textSearch ASC, g.genericId ASC";

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
            GenericIndex = new Dictionary<string, Generic>();

            // 16 bytes
            benchmarkData = "0123456789ABCDEF";

            // 1 MB
            while (benchmarkData.Length < 1024 * 1024)
            {
                benchmarkData += benchmarkData;
            }

            // load generic into Lucene index
            foreach (Generic generic in LoadGeneric(jsonOptions))
            {
                writer.AddDocument(generic.ToDocument());
            }

            // flush the writes to the index
            writer.Flush(true, true);
        }

        // used for upsert / delete
        public static Dictionary<string, Generic> GenericIndex { get; set; }

        public async Task<string> GetBenchmarkDataAsync(int size)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return benchmarkData[0..size];
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Generic by ID
        /// </summary>
        /// <param name="genericId">ID</param>
        /// <returns>Generic</returns>
        public async Task<Generic> GetGenericAsync(string genericId)
        {
            return await Task<Generic>.Factory.StartNew(() => { return GetGeneric(genericId); }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Generic By ID
        /// </summary>
        /// <param name="genericId">ID</param>
        /// <returns>Generic</returns>
        public Generic GetGeneric(string genericId)
        {
            if (genericId.StartsWith("tt"))
            {
                IndexSearcher searcher = new IndexSearcher(writer.GetReader(true));

                // search by genericId
                TopDocs hits = searcher.Search(new PhraseQuery { new Term("genericId", genericId) }, 1);

                if (hits.TotalHits > 0)
                {
                    // deserialize the json from the index
                    return JsonSerializer.Deserialize<Generic>(searcher.Doc(hits.ScoreDocs[0].Doc).GetBinaryValue("json").Bytes);
                }
                else
                {
                    // handle the upserted generic
                    if (GenericIndex.ContainsKey(genericId))
                    {
                        return GenericIndex[genericId];
                    }
                }
            }

            throw new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 404, string.Empty, 0);
        }

        /// <summary>
        /// Get Cosmos query string based on query parameters
        /// </summary>
        /// <param name="genericQueryParameters">query params</param>
        /// <returns>Cosmos query string</returns>
        public string GetGenericIds(GenericQueryParameters genericQueryParameters)
        {
            List<Generic> cache;
            string ids = string.Empty;

            if (genericQueryParameters == null)
            {
                cache = GetGenerics(string.Empty);
            }
            else
            {
                cache = GetGenerics(genericQueryParameters.Q);
            }

            foreach (Generic g in cache)
            {
                ids += $"'{g.Id}',";
            }

            // nothing found
            if (string.IsNullOrWhiteSpace(ids))
            {
                return string.Empty;
            }

            return GenericSQL.Replace("{0}", ids[0..^1], StringComparison.Ordinal);
        }

        /// <summary>
        /// Get list of Generic based on query parameters
        /// </summary>
        /// <param name="genericQueryParameters">query params</param>
        /// <returns>List of Generic</returns>
        public List<Generic> GetGenerics(GenericQueryParameters genericQueryParameters)
        {
            if (genericQueryParameters == null)
            {
                return GetGenerics(string.Empty);
            }

            return GetGenerics(genericQueryParameters.Q);
        }

        /// <summary>
        /// Get List of Generic by search params
        /// </summary>
        /// <param name="q">match property</param>
        /// <returns>List of Generic</returns>
        public List<Generic> GetGenerics(string q)
        {
            List<Generic> res = new List<Generic>();

            IndexSearcher searcher = new IndexSearcher(writer.GetReader(true));

            // type = Generic
            BooleanQuery bq = new BooleanQuery
            {
                { new PhraseQuery { new Term("type", "Generic") }, Occur.MUST },
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
                res.Add(JsonSerializer.Deserialize<Generic>(searcher.Doc(results.ScoreDocs[i].Doc).GetBinaryValue("json").Bytes));
            }

            return res;
        }

        /// <summary>
        /// Get list of Generic based on query parameters
        /// </summary>
        /// <param name="genericQueryParameters">query params</param>
        /// <returns>List of Generic</returns>
        public Task<IEnumerable<Generic>> GetGenericsAsync(GenericQueryParameters genericQueryParameters)
        {
            return Task<IEnumerable<Generic>>.Factory.StartNew(() =>
            {
                return GetGenerics(genericQueryParameters);
            });
        }

        /// <summary>
        /// Upsert a generic
        ///
        /// Do not store in index or WebV tests will break
        /// </summary>
        /// <param name="generic">Generic to upsert</param>
        /// <returns>Generic</returns>
        public async Task<Generic> UpsertGenericAsync(Generic generic)
        {
            await Task.Run(() =>
            {
                if (GenericIndex.ContainsKey(generic.GenericId))
                {
                    generic = GenericIndex[generic.GenericId];
                }
                else
                {
                    GenericIndex.Add(generic.GenericId, generic);
                }
            }).ConfigureAwait(false);

            return generic;
        }

        /// <summary>
        /// Delete the generic from temporary storage
        /// </summary>
        /// <param name="genericId">Generic ID</param>
        /// <returns>void</returns>
        public async Task DeleteGenericAsync(string genericId)
        {
            await Task.Run(() =>
            {
                if (GenericIndex.ContainsKey(genericId))
                {
                    GenericIndex.Remove(genericId);
                }
            }).ConfigureAwait(false);
        }

        // load Generic List from json file
        private static List<Generic> LoadGeneric(JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<List<Generic>>(File.ReadAllText("src/data/generic.json"), options);
        }
    }
}
