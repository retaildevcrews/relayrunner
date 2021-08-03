// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Model;
using Ngsa.Middleware;

namespace Ngsa.Application.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<Generic> GetGenericAsync(string genericId);
        Task<IEnumerable<Generic>> GetGenericsAsync(GenericQueryParameters genericQueryParameters);
        Task DeleteGenericAsync(string genericId);
        Task<Generic> UpsertGenericAsync(Generic generic);
    }
}
