﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CorrelationVector;
using RelayRunner.Middleware;

namespace RelayRunner.Application.Controllers
{
    /// <summary>
    /// Handles query requests from the controllers
    /// </summary>
    public static class DataService
    {
        // json serialization options
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // http client used to call data layer
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri(App.Config.DataService),
        };

        /// <summary>
        /// Call the data access layer proxy using a path and query string
        /// </summary>
        /// <typeparam name="T">Result Type</typeparam>
        /// <param name="request">HTTP Request</param>
        /// <returns>IActionResult</returns>
        public static async Task<IActionResult> Read<T>(HttpRequest request)
        {
            if (request == null || !request.Path.HasValue)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string path = RequestLogger.GetPathAndQuerystring(request);

            CorrelationVector cVector = Middleware.CorrelationVectorExtensions.GetCorrelationVectorFromContext(request.HttpContext);

            try
            {
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, path);

                if (cVector != null)
                {
                    req.Headers.Add(CorrelationVector.HeaderName, cVector.Value);
                }

                HttpResponseMessage resp = await Client.SendAsync(req);

                JsonResult json;

                if (resp.IsSuccessStatusCode)
                {
                    T obj = JsonSerializer.Deserialize<T>(await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false), Options);
                    json = new JsonResult(obj, Options);
                }
                else
                {
                    dynamic err = JsonSerializer.Deserialize<dynamic>(await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false), Options);

                    json = new JsonResult(err, Options) { StatusCode = (int)resp.StatusCode };
                }

                return json;
            }
            catch (Exception ex)
            {
                return CreateResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// ContentResult factory
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="statusCode">int</param>
        /// <returns>JsonResult</returns>
        public static JsonResult CreateResult(string message, HttpStatusCode statusCode)
        {
            return new JsonResult(new ErrorResult { Error = statusCode, Message = message })
            {
                StatusCode = (int)statusCode,
            };
        }
    }
}
