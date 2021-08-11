// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Database.Model
{
    public class LoadClient : ICloneable
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
            return entityType;
        }

        /// <summary>
        /// IClonable::Clone
        /// </summary>
        /// <returns>LoadClient as object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
