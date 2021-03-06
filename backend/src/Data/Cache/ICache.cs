// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Documents;
using RelayRunner.Models;

namespace RelayRunner.Application.Data
{
    /// <summary>
    /// Data Access Layer for Cache Interface
    /// </summary>
    public interface ICache
    {
        IEnumerable<Client> GetClients();
        Client GetClientByClientStatusId(string clientStatusId);
        void ProcessClientStatusChange(Document doc);
    }
}
