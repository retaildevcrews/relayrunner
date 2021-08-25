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
    /// Handle all of the /api/loadClients requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoadClientsController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(LoadClientsController).FullName,
            ErrorMessage = "LoadClientsControllerException",
            NotFoundError = "LoadClients Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadClientsController"/> class.
        /// </summary>
        public LoadClientsController()
        {
            dal = App.Config.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of LoadClient objects
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetLoadClientsAsync()
        {
            IActionResult res;

            res = await ResultHandler.Handle(dal.GetLoadClientsAsync(), Logger).ConfigureAwait(false);

            return res;
        }

         /// <summary>
         /// Returns a single JSON LoadClient by Parameter, loadClientId
         /// </summary>
         /// <param name="loadClientId">loadClientId</param>
         /// <returns>IActionResult</returns>
        [HttpGet("{loadClientId}")]
        public async Task<IActionResult> GetLoadClientByIdAsync([FromRoute] string loadClientId)
         {
             if (string.IsNullOrWhiteSpace(loadClientId))
             {
                 throw new ArgumentNullException(nameof(loadClientId));
             }

             List<Middleware.Validation.ValidationError> list = LoadClientParameters.ValidateLoadClientId(loadClientId);

             if (list.Count > 0)
             {
                 Logger.LogWarning(nameof(GetLoadClientsAsync), "Invalid Load Client Id", NgsaLog.LogEvent400, HttpContext);

                 return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
             }

             IActionResult res;

             res = await ResultHandler.Handle(dal.GetLoadClientByIdAsync(loadClientId), Logger).ConfigureAwait(false);

             return res;
         }
     }
 }
