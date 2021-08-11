// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using RelayRunner.Application.DataAccessLayer;
using RelayRunner.Middleware;

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

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<LoadClient>>(Request).ConfigureAwait(false);
            }
            else
            {
                // get the result
                res = await ResultHandler.Handle(dal.GetLoadClientsAsync(), Logger).ConfigureAwait(false);
            }

            return res;
        }

         /// <summary>
         /// Returns a single JSON LoadClient by idParameter
         /// </summary>
         /// <param name="id">ID</param>
         /// <returns>IActionResult</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoadClientByIdAsync([FromRoute] string id)
         {
             if (string.IsNullOrWhiteSpace(id))
             {
                 throw new ArgumentNullException(nameof(id));
             }

             List<Middleware.Validation.ValidationError> list = LoadClientQueryParameters.ValidateId(id);

             if (list.Count > 0)
             {
                 Logger.LogWarning(nameof(GetLoadClientsAsync), "Invalid Load Client Id", NgsaLog.LogEvent400, HttpContext);

                 return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
             }

             IActionResult res;

             if (App.Config.AppType == AppType.WebAPI)
             {
                 res = await DataService.Read<LoadClient>(Request).ConfigureAwait(false);
             }
             else
             {
                 res = await ResultHandler.Handle(dal.GetLoadClientAsync(id), Logger).ConfigureAwait(false);
             }

             return res;
         }
     }
 }
