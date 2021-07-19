// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Ngsa.Middleware.Validation
{
    /// <summary>
    /// Validation Error Class
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets error Code
        ///     default is InvalidValue per spec
        /// </summary>
        public string Code { get; set; } = "InvalidValue";

        /// <summary>
        /// Gets or sets error Target
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets error Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Get standard error message
        ///     changing these will require changes to the json validation tests
        /// </summary>
        /// <param name="fieldName">field name</param>
        /// <returns>string</returns>
        public static string GetErrorMessage(string fieldName)
        {
            return fieldName.ToUpperInvariant() switch
            {
                _ => $"Unknown parameter: {fieldName}",
            };
        }

        /// <summary>
        /// Get the doc link based on request URL
        /// </summary>
        /// <param name="path">full request path</param>
        /// <returns>link to doc</returns>
        public static string GetErrorLink(string path)
        {
            string s = "https://github.com/retaildevcrews/ngsa/blob/main/docs/ParameterValidation.md";

            path = path.ToLowerInvariant();

            return s;
        }

        public static string GetCategory(HttpContext context, out string subCategory, out string mode)
        {
            string category;

            string path = RequestLogger.GetPathAndQuerystring(context.Request).ToLowerInvariant();

            
            if (path.StartsWith("/healthz"))
            {
                category = "Healthz";
                subCategory = "Healthz";
                mode = "Healthz";
            }
            else if (path.StartsWith("/metrics"))
            {
                category = "Metrics";
                subCategory = "Metrics";
                mode = "Metrics";
            }
            else
            {
                category = "Static";
                subCategory = "Static";
                mode = "Static";
            }

            return category;
        }
    }
}
