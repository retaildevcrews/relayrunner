// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.LoadClients;
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
    public class GenericController : Controller
    {
        private static readonly NgsaLog Logger = new NgsaLog
        {
            Name = typeof(GenericController).FullName,
            ErrorMessage = "GenericControllerException",
            NotFoundError = "Generic Not Found",
        };

        private readonly IDAL dal;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericController"/> class.
        /// </summary>
        public GenericController()
        {
            dal = App.Config.CosmosDal;
        }

        /// <summary>
        /// Returns a JSON array of Generic objects
        /// </summary>
        /// <param name="genericQueryParameters">query parameters</param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetGenericsAsync([FromQuery] GenericQueryParameters genericQueryParameters)
        {
            if (genericQueryParameters == null)
            {
                throw new ArgumentNullException(nameof(genericQueryParameters));
            }

            List<Middleware.Validation.ValidationError> list = genericQueryParameters.Validate();

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetGenericsAsync), NgsaLog.MessageInvalidQueryString, NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<List<Generic>>(Request).ConfigureAwait(false);
            }
            else
            {
                // get the result
                res = await ResultHandler.Handle(dal.GetGenericsAsync(genericQueryParameters), Logger).ConfigureAwait(false);
            }

            return res;
        }

        /// <summary>
        /// Returns a single JSON Generic by genericIdParameter
        /// </summary>
        /// <param name="genericId">Generic ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{genericId}")]
        public async Task<IActionResult> GetGenericByIdAsync([FromRoute] string genericId)
        {
            if (string.IsNullOrWhiteSpace(genericId))
            {
                throw new ArgumentNullException(nameof(genericId));
            }

            List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetGenericsAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Read<Generic>(Request).ConfigureAwait(false);
            }
            else
            {
                res = await ResultHandler.Handle(dal.GetGenericAsync(genericId), Logger).ConfigureAwait(false);
            }

            return res;
        }

        [HttpPut("{genericId}")]
        public async Task<IActionResult> UpsertGenericAsync([FromRoute] string genericId)
        {
            try
            {
                List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

                if (list.Count > 0 || !genericId.StartsWith("zz"))
                {
                    Logger.LogWarning(nameof(UpsertGenericAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

                    return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
                }

                // duplicate the generic for upsert
                Generic gOrig = App.Config.CacheDal.GetGeneric(genericId.Replace("zz", "tt"));
                Generic g = gOrig.DuplicateForUpsert();

                IActionResult res;

                if (App.Config.AppType == AppType.WebAPI)
                {
                    res = await DataService.Post(Request, g).ConfigureAwait(false);
                }
                else
                {
                    await App.Config.CacheDal.UpsertGenericAsync(g);

                    // upsert into Cosmos
                    if (!App.Config.InMemory)
                    {
                        try
                        {
                            await App.Config.CosmosDal.UpsertGenericAsync(g).ConfigureAwait(false);
                        }
                        catch (CosmosException ce)
                        {
                            Logger.LogError("UpsertGenericAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);

                            return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
                        }
                        catch (Exception ex)
                        {
                            // log and return 500
                            Logger.LogError("UpsertGenericAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
                            return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
                        }
                    }

                    res = Ok(g);
                }

                return res;
            }
            catch
            {
                return NotFound($"Generic ID Not Found: {genericId}");
            }
        }

        /// <summary>
        /// Delete a generic by genericId
        /// </summary>
        /// <param name="genericId">ID to delete</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{genericId}")]
        public async Task<IActionResult> DeleteGenericAsync([FromRoute] string genericId)
        {
            List<Middleware.Validation.ValidationError> list = GenericQueryParameters.ValidateGenericId(genericId);

            if (list.Count > 0 || !genericId.StartsWith("zz"))
            {
                Logger.LogWarning(nameof(UpsertGenericAsync), "Invalid Generic Id", NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            IActionResult res;

            if (App.Config.AppType == AppType.WebAPI)
            {
                res = await DataService.Delete(Request).ConfigureAwait(false);
            }
            else
            {
                await App.Config.CacheDal.DeleteGenericAsync(genericId);
                res = NoContent();

                if (!App.Config.InMemory)
                {
                    try
                    {
                        // Delete from Cosmos
                        await App.Config.CosmosDal.DeleteGenericAsync(genericId).ConfigureAwait(false);
                    }
                    catch (CosmosException ce)
                    {
                        // log and return Cosmos status code
                        if (ce.StatusCode != HttpStatusCode.NotFound)
                        {
                            Logger.LogError("DeleteGenericAsync", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
                            return ResultHandler.CreateResult(Logger.ErrorMessage, ce.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        // log and return 500
                        Logger.LogError("DeleteGenericAsync", "Exception", NgsaLog.LogEvent500, ex: ex);
                        return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
                    }
                }
            }

            return res;
        }
    }
}