// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Ngsa.Application.DataAccessLayer;
using Ngsa.Middleware;

namespace Ngsa.Application.Controllers
{
    /// <summary>
    /// Handle all of the /api/generic requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoadClientsController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(LoadClientsController).FullName,
            ErrorMessage = "LoadClientControllerException",
            NotFoundError = "LoadClient Not Found",
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
        /// <param name="loadClientQueryParameters">query parameters</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetLoadClientsAsync([FromQuery] GenericQueryParameters loadClientQueryParameters)
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
    }
}

//         /// <summary>
//         /// Returns a single JSON Generic by genericIdParameter
//         /// </summary>
//         /// <param name="genericId">Generic ID</param>
//         /// <returns>IActionResult</returns>
//         [HttpGet("{genericId}")]
//         public async Task<IActionResult> GetGenericByIdAsync([FromRoute] string genericId)
//         {
//             if (string.IsNullOrWhiteSpace(genericId))
//             {
//                 throw new ArgumentNullException(nameof(genericId));
//             }

//             List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

//             if (list.Count > 0)
//             {
//                 Logger.LogWarning(nameof(GetGenericsAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

//                 return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
//             }

//             IActionResult res;

//             if (App.Config.AppType == AppType.WebAPI)
//             {
//                 res = await DataService.Read<Generic>(Request).ConfigureAwait(false);
//             }
//             else
//             {
//                 res = await ResultHandler.Handle(dal.GetGenericAsync(genericId), Logger).ConfigureAwait(false);
//             }

//             return res;
//         }

//         [HttpPut("{genericId}")]
//         public async Task<IActionResult> UpsertGenericAsync([FromRoute] string genericId)
//         {
//             try
//             {
//                 List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

//                 if (list.Count > 0 || !genericId.StartsWith("zz"))
//                 {
//                     Logger.LogWarning(nameof(UpsertGenericAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

//                     return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
//                 }

//                 // duplicate the generic for upsert
//                 Generic gOrig = App.Config.CacheDal.GetGeneric(genericId.Replace("zz", "tt"));
//                 Generic g = gOrig.DuplicateForUpsert();

//                 IActionResult res;

//                 if (App.Config.AppType == AppType.WebAPI)
//                 {
//                     res = await DataService.Post(Request, g).ConfigureAwait(false);
//                 }
//                 else
//                 {
//                     await App.Config.CacheDal.UpsertGenericAsync(g);

//                     // upsert into Cosmos
//                     if (!App.Config.InMemory)
//                     {
//                         try
//                         {
//                             await App.Config.CosmosDal.UpsertGenericAsync(g).ConfigureAwait(false);
//                         }
//                         catch (CosmosException ce)
//                         {
//                             Logger.LogError("UpsertGenericAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);

//                             return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
//                         }
//                         catch (Exception ex)
//                         {
//                             // log and return 500
//                             Logger.LogError("UpsertGenericAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
//                             return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
//                         }
//                     }

//                     res = Ok(g);
//                 }

//                 return res;
//             }
//             catch
//             {
//                 return NotFound($"Generic ID Not Found: {genericId}");
//             }
//         }

//         /// <summary>
//         /// Delete a generic by genericId
//         /// </summary>
//         /// <param name="genericId">ID to delete</param>
//         /// <returns>IActionResult</returns>
//         [HttpDelete("{genericId}")]
//         public async Task<IActionResult> DeleteGenericAsync([FromRoute] string genericId)
//         {
//             List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

//             if (list.Count > 0 || !genericId.StartsWith("zz"))
//             {
//                 Logger.LogWarning(nameof(UpsertGenericAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

//                 return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
//             }

//             IActionResult res;

//             if (App.Config.AppType == AppType.WebAPI)
//             {
//                 res = await DataService.Delete(Request).ConfigureAwait(false);
//             }
//             else
//             {
//                 await App.Config.CacheDal.DeleteGenericAsync(genericId);
//                 res = NoContent();

//                 if (!App.Config.InMemory)
//                 {
//                     try
//                     {
//                         // Delete from Cosmos
//                         await App.Config.CosmosDal.DeleteGenericAsync(genericId).ConfigureAwait(false);
//                     }
//                     catch (CosmosException ce)
//                     {
//                         // log and return Cosmos status code
//                         if (ce.StatusCode != HttpStatusCode.NotFound)
//                         {
//                             Logger.LogError("DeleteGenericAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
//                             return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         // log and return 500
//                         Logger.LogError("DeleteGenericAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
//                         return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
//                     }
//                 }
//             }

//             return res;
//         }
//     }
// }