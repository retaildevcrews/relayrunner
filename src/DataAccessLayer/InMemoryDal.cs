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

            // 16 bytes
            benchmarkData = "0123456789ABCDEF";

            // 1 MB
            while (benchmarkData.Length < 1024 * 1024)
            {
                benchmarkData += benchmarkData;
            }

            // flush the writes to the index
            writer.Flush(true, true);
        }

        public async Task<string> GetBenchmarkDataAsync(int size)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return benchmarkData[0..size];
            }).ConfigureAwait(false);
        }
    }
}
