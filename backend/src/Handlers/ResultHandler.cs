// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RelayRunner.Middleware.Validation;

namespace RelayRunner.Middleware
{
    /// <summary>
    /// Handles query requests from the controllers
    /// </summary>
    public static class ResultHandler
    {
        /// <summary>
        /// ContentResult factory
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="statusCode">int</param>
        /// <returns>JsonResult</returns>
        public static JsonResult CreateResult(string message, HttpStatusCode statusCode)
        {
            JsonResult res = new (new ErrorResult { Error = statusCode, Message = message })
            {
                StatusCode = (int)statusCode,
            };

            return res;
        }

        /// <summary>
        /// ContentResult factory
        /// </summary>
        /// <param name="errorList">list of validation errors</param>
        /// <param name="path">string</param>
        /// <returns>JsonResult</returns>
        public static JsonResult CreateResult(List<ValidationError> errorList, string path)
        {
            Dictionary<string, object> data = new ()
            {
                { "type", ValidationError.GetErrorLink(path) },
                { "title", "Parameter validation error" },
                { "detail", "One or more invalid parameters were specified." },
                { "status", (int)HttpStatusCode.BadRequest },
                { "instance", path },
                { "validationErrors", errorList },
            };

            JsonResult res = new (data)
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ContentType = "application/problem+json",
            };

            return res;
        }
    }
}
