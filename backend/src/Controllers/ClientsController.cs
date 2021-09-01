// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RelayRunner.Application.DataAccessLayer;
using RelayRunner.Middleware;
using RelayRunner.Model;

namespace RelayRunner.Application.Controllers
{
    /// <summary>
    /// Handle all of the /api/clients requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : Controller
    {
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(ClientsController).FullName,
            ErrorMessage = "ClientsControllerException",
            NotFoundError = "Clients Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        public ClientsController()
        {
            dal = App.Config.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of Client objects
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetClientsAsync()
        {
            IActionResult res;

            res = await ResultHandler.Handle(dal.GetClientsAsync(), Logger).ConfigureAwait(false);

            return res;
        }

         /// <summary>
         /// Returns a single JSON Client by Parameter, clientStatusId
         /// </summary>
         /// <param name="clientStatusId">clientStatusId</param>
         /// <returns>IActionResult</returns>
        [HttpGet("{clientStatusId}")]
        public async Task<IActionResult> GetClientByClientStatusIdAsync([FromRoute] string clientStatusId)
         {
             if (string.IsNullOrWhiteSpace(clientStatusId))
             {
                 throw new ArgumentNullException(nameof(clientStatusId));
             }

             List<Middleware.Validation.ValidationError> list = ClientParameters.ValidateClientStatusId(clientStatusId);

             if (list.Count > 0)
             {
                 Logger.LogWarning(nameof(GetClientByClientStatusIdAsync), "Invalid Client Status Id", NgsaLog.LogEvent400, HttpContext);

                 return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
             }

             IActionResult res;

             res = await ResultHandler.Handle(dal.GetClientByClientStatusIdAsync(clientStatusId), Logger).ConfigureAwait(false);

             return res;
         }
     }
 }
