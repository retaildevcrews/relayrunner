// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using RelayRunner.Middleware.Validation;

namespace RelayRunner.Middleware
{
    /// <summary>
    /// Query string parameters for LoadClients controller
    /// </summary>
    public sealed class LoadClientParameters
    {
        /// <summary>
        /// Validate Id
        /// </summary>
        /// <param name="loadClientId">id to validate</param>
        /// <returns>empty list on valid</returns>
        public static List<ValidationError> ValidateLoadClientId(string loadClientId)
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (!int.TryParse(loadClientId, out int i) || i < 0)
            {
                errors.Add(new ValidationError() { Target = "loadClientId", Message = ValidationError.GetErrorMessage("loadClientId") });
            }

            return errors;
        }
    }
}
