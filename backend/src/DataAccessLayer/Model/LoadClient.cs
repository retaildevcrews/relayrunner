// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace RelayRunner.Model
{
    public class LoadClient
    {
        public string PartitionKey { get; set; }
        public string EntityType { get; set; }
        public string Version { get; set; }
        public string Id { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public bool Prometheus { get; set; }
        public string StartupArgs { get; set; }
        public DateTime StartTime { get; set; }


        /// <summary>
        /// Compute the partition key based on the EntityType
        /// </summary>
        /// <param name="entityType">EntityType</param>
        /// <returns>the partition key</returns>
        public static string ComputePartitionKey(string entityType)
        {
            if (!string.IsNullOrWhiteSpace(entityType))
            {
                return entityType;
            }

            throw new ArgumentException("Invalid Partition Key");
        }
    }
}
