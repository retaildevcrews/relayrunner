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
    /// Handle all of the /api/generic requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoadClientController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(LoadClientController).FullName,
            ErrorMessage = "LoadClientControllerException",
            NotFoundError = "LoadClient Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadClientController"/> class.
        /// </summary>
        public LoadClientController()
        {
            dal = App.Config.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of LoadClient objects
        /// </summary>
        /// <param name="loadClientQueryParameters">query parameters</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetLoadClientsAsync([FromQuery] LoadClientQueryParameters loadClientQueryParameters)
        {
            if (loadClientQueryParameters == null)
            {
                throw new ArgumentNullException(nameof(loadClientQueryParameters));
            }

            List<Middleware.Validation.ValidationError> list = loadClientQueryParameters.Validate();

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetLoadClientsAsync), NgsaLog.MessageInvalidQueryString, NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<LoadClient>>(Request).ConfigureAwait(false);
            }
            else
            {
                // get the result
                res = await ResultHandler.Handle(dal.GetLoadClientsAsync(loadClientQueryParameters), Logger).ConfigureAwait(false);
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

        // TODO: Configure COSMOS and id format

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpsertLoadClientAsync([FromRoute] string id)
        // {
        //     try
        //     {
        //         List<Middleware.Validation.ValidationError> list = LoadClientQueryParameters.ValidateId(id);

        //        // TODO: update based on id format

        //         if (list.Count > 0 || !id.StartsWith("zz"))
        //         {
        //             Logger.LogWarning(nameof(UpsertLoadClientAsync), "Invalid Load Client Id", NgsaLog.LogEvent400, HttpContext);

        //             return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
        //         }

        //         // duplicate the load client for upsert

        //         // TODO: based on id format tbd

        //         //LoadClient gOrig = App.Config.CacheDal.GetLoadClients(id.Replace("zz", "tt"));
        //         //LoadClient g = gOrig.DuplicateForUpsert();

        //         IActionResult res;

        //         if (App.Config.AppType == AppType.WebAPI)
        //         {
        //             res = await DataService.Post(Request, g).ConfigureAwait(false);
        //         }
        //         else
        //         {
        //             await App.Config.CacheDal.UpsertLoadClientsAsync(g);

        //             // upsert into Cosmos
        //             if (!App.Config.InMemory)
        //             {
        //                 try
        //                 {
        //                     await App.Config.CosmosDal.UpsertLoadClientAsync(g).ConfigureAwait(false);
        //                 }
        //                 catch (CosmosException ce)
        //                 {
        //                     Logger.LogError("UpsertLoadClientsAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);

        //                     return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
        //                 }
        //                 catch (Exception ex)
        //                 {
        //                     // log and return 500
        //                     Logger.LogError("UpsertLoadClientsAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
        //                     return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
        //                 }
        //             }

        //             res = Ok(g);
        //         }

        //         return res;
        //     }
        //     catch
        //     {
        //         return NotFound($"LoadClient ID Not Found: {id}");
        //     }
        // }

        // Configure Cosmos

         /// <summary>
         /// Delete a LoadClient by id
         /// </summary>
         /// <param name="id">ID to delete</param>
         /// <returns>IActionResult</returns>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteLoadClientAsync([FromRoute] string id)
        // {
        //     List<Middleware.Validation.ValidationError> list = LoadClientQueryParameters.ValidateId(id);

        //    // TODO: update once decide id format
        //     if (list.Count > 0 || !id.StartsWith("zz"))
        //     {
        //         Logger.LogWarning(nameof(UpsertLoadClientAsync), "Invalid LoadClient Id", NgsaLog.LogEvent400, HttpContext);

        //         return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
        //     }

        //     IActionResult res;

        //     if (App.Config.AppType == AppType.WebAPI)
        //     {
        //         res = await DataService.Delete(Request).ConfigureAwait(false);
        //     }
        //     else
        //     {
        //         await App.Config.CacheDal.DeleteLoadClientAsync(id);
        //         res = NoContent();

        //         if (!App.Config.InMemory)
        //         {
        //             try
        //             {
        //                 // Delete from Cosmos
        //                 await App.Config.CosmosDal.DeleteLoadClientAsync(id).ConfigureAwait(false);
        //             }
        //             catch (CosmosException ce)
        //             {
        //                 // log and return Cosmos status code
        //                 if (ce.StatusCode != HttpStatusCode.NotFound)
        //                 {
        //                     Logger.LogError("DeleteLoadClientAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
        //                     return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 // log and return 500
        //                 Logger.LogError("DeleteLoadClientAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
        //                 return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
        //             }
        //         }
        //     }

        //     return res;
        // }
     }
 }
