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
        public int Id { get; set; }
        public int Version { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public string Scheduler { get; set; }
        public string Metrics { get; set; }
        public string Status { get; set; }
        public DateTimeOffset DateCreated { get; init; }


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
            // validate id
            if (!string.IsNullOrWhiteSpace(id) &&
                (id.StartsWith("tt", StringComparison.OrdinalIgnoreCase) ||
                id.StartsWith("zz", StringComparison.OrdinalIgnoreCase)) &&
                int.TryParse(id[2..], out int idInt))
            {
                return (idInt % 10).ToString(CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Invalid Partition Key");
        }

        /// <summary>
        /// Sort LoadClients by Name
        /// </summary>
        /// <param name="x">first comparison</param>
        /// <param name="y">second comparison</param>
        /// <returns>int</returns>
        public static int PropertyCompare(LoadClient x, LoadClient y)
        {
            int result;

            result = string.Compare(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase);

            // Do I even need this if statement or what other property should we compare based on.
            // Should we change ID and Version to strings?
            // if (result == 0)
            // {
            //     return string.Compare(y.Id, y.Id, StringComparison.OrdinalIgnoreCase);
            // }

            return result;
        }

        // Do we need to keep below section and should I change properties?

        /// <summary>
        /// Duplicate this LoadClient for upsert testing
        /// </summary>
        /// <returns>LoadClient</returns>
        public Generic DuplicateForUpsert()
        {
            Generic g = (Generic)MemberwiseClone();

            g.GenericId = g.GenericId.Replace("tt", "zz");
            g.Id = g.GenericId;
            g.Type = "Generic-Dupe";

            return g;
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
        /// Convert LoadClient to a Lucene Document for indexing
        /// </summary>
        /// <returns>Lucene Document</returns>
        public Document ToDocument()
        {
            Document doc = new Document
            {
                new StringField("Name", Name, Store.YES),
                new StringField("Region", Region, Store.YES),
                new StringField("Zone", Zone, Store.YES),
                new StringField("Scheduler", Scheduler, Store.YES),
                new StringField("Metrics", Metrics, Store.YES),
                new StringField("Status", Status, Store.YES),
                // new Int32Field("partitionKey", int.Parse(PartitionKey), Store.YES),
                // new StringField("type", Type, Store.YES),
                // new TextField("property", Property, Store.YES),
                // new StringField("propertySort", Property.ToLowerInvariant(), Store.YES),
            };

            doc.Add(new StoredField("json", JsonSerializer.SerializeToUtf8Bytes<LoadClient>(this)));
            return doc;
        }
    }
}
