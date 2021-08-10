// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using RelayRunner.Middleware.Validation;

namespace RelayRunner.Middleware
{
    /// <summary>
    /// Query string parameters for LoadClient controller
    /// </summary>
    public sealed class LoadClientQueryParameters
    {
        public string Q { get; set; }

        /// <summary>
        /// Validate Id
        /// </summary>
        /// <param name="id">id to validate</param>
        /// <returns>empty list on valid</returns>
        public static List<ValidationError> ValidateId(string id)
        {
            // TODO: Need to decide on Id format

            List<ValidationError> errors = new List<ValidationError>();
            return errors;
        }

        /// <summary>
        /// Validate this object
        /// </summary>
        /// <returns>list of validation errors or empty list</returns>
        public List<ValidationError> Validate()
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (!string.IsNullOrWhiteSpace(Q) && Q.Length < 2)
            {
                errors.Add(new ValidationError { Target = "q", Message = ValidationError.GetErrorMessage("Q") });
            }

            return errors;
        }

        /// <summary>
        /// Get the cache key for this request
        /// </summary>
        /// <returns>cache key</returns>
        public string GetKey()
        {
            return $"/api/loadClient/{(string.IsNullOrWhiteSpace(Q) ? string.Empty : Q.ToUpperInvariant().Trim())}";
        }
    }
}
