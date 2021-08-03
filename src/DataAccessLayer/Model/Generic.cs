// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text.Json;
using Lucene.Net.Documents;
using static Lucene.Net.Documents.Field;

namespace Database.Model
{
    public class Generic : ICloneable
    {
        public string Id { get; set; }
        public string GenericId { get; set; }
        public string PartitionKey { get; set; }
        public string Type { get; set; }
        public string Property { get; set; }
        public string TextSearch { get; set; }

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
        /// Sort Generic by Property
        /// </summary>
        /// <param name="x">first comparison</param>
        /// <param name="y">second comparison</param>
        /// <returns>int</returns>
        public static int PropertyCompare(Generic x, Generic y)
        {
            int result;

            result = string.Compare(x?.Property, y?.Property, StringComparison.OrdinalIgnoreCase);

            if (result == 0)
            {
                return string.Compare(y.Id, y.Id, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Duplicate this generic for upsert testing
        /// </summary>
        /// <returns>Generic</returns>
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
        /// <returns>Generic as object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Convert Generic to a Lucene Document for indexing
        /// </summary>
        /// <returns>Lucene Document</returns>
        public Document ToDocument()
        {
            Document doc = new Document
            {
                new StringField("id", Id, Store.YES),
                new StringField("genericId", GenericId, Store.YES),
                new Int32Field("partitionKey", int.Parse(PartitionKey), Store.YES),
                new StringField("type", Type, Store.YES),
                new TextField("property", Property, Store.YES),
                new StringField("propertySort", Property.ToLowerInvariant(), Store.YES),
            };

            doc.Add(new StoredField("json", JsonSerializer.SerializeToUtf8Bytes<Generic>(this)));
            return doc;
        }
    }
}
