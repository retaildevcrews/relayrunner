// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Ngsa.Middleware.Validation;

namespace Ngsa.Middleware
{
    /// <summary>
    /// Query string parameters for Generic controller
    /// </summary>
    public sealed class GenericQueryParameters
    {
        public string Q { get; set; }

        /// <summary>
        /// Validate genericId
        /// </summary>
        /// <param name="genericId">id to validate</param>
        /// <returns>empty list on valid</returns>
        public static List<ValidationError> ValidateGenericId(string genericId)
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (!string.IsNullOrWhiteSpace(genericId) && (
                genericId != genericId.ToLowerInvariant().Trim() ||
                (!genericId.StartsWith("tt") && !genericId.StartsWith("zz")) ||
                !int.TryParse(genericId[2..], out int v) ||
                v <= 0))
            {
                errors.Add(new ValidationError { Target = "genericId", Message = ValidationError.GetErrorMessage("genericId") });
            }

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
            return $"/api/generic/{(string.IsNullOrWhiteSpace(Q) ? string.Empty : Q.ToUpperInvariant().Trim())}";
        }
    }
}
