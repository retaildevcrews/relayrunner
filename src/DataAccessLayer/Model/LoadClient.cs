// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text.Json;
using Lucene.Net.Documents;
using static Lucene.Net.Documents.Field;

namespace Database.Model
{
    public class LoadClient : ICloneable
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string Version { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public string Scheduler { get; set; }
        public string Metrics { get; set; }
        public string Status { get; set; }
        public string DateCreated { get; init; }


        /// <summary>
        /// Compute the partition key based on the genericId
        ///
        /// For this sample, the partitionkey is the id mod 10
        ///
        /// In a full implementation, you would update the logic to determine the partition key
        /// </summary>
        /// <param name="id">document id</param>
        /// <returns>the partition key</returns>
        public static string ComputePartitionKey(string id)
        {
            // TODO: Decide on partition key structure
            return id;
        }

        /// <summary>
        /// IClonable::Clone
        /// </summary>
        /// <returns>LoadClient as object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Convert LoadClients to a Lucene Document for indexing
        /// </summary>
        /// <returns>Lucene Document</returns>
        public Document ToDocument()
        {
            Document doc = new Document
            {
                new StringField("name", Name, Store.YES),
                new StringField("id", Id, Store.YES),
                new Int32Field("partitionKey", int.Parse(PartitionKey), Store.YES),
                new StringField("version", Version, Store.YES),
                new StringField("region", Region, Store.YES),
                new StringField("zone", Zone, Store.YES),
                new StringField("scheduler", Scheduler, Store.YES),
                new StringField("metrics", Metrics, Store.YES),
                new TextField("status", Status, Store.YES),
                new StringField("dateCreated", DateCreated, Store.YES),
            };

            doc.Add(new StoredField("json", JsonSerializer.SerializeToUtf8Bytes<LoadClient>(this)));
            return doc;
        }
    }
}
