// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Model;
using RelayRunner.Middleware;

namespace RelayRunner.Application.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<LoadClient> GetLoadClientAsync(string id);
        Task<IEnumerable<LoadClient>> GetLoadClientsAsync(LoadClientQueryParameters loadClientQueryParameters);
        Task DeleteLoadClientAsync(string id);
        Task<LoadClient> UpsertLoadClientAsync(LoadClient loadClient);
        Task<IEnumerable<LoadClient>> GetLoadClientAsync(LoadClientQueryParameters loadClientQueryParameters);
    }
}