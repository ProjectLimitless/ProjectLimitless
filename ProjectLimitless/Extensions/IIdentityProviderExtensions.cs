/** 
* This file is part of Project Limitless.
* Copyright © 2017 Donovan Solms.
* Project Limitless
* https://www.projectlimitless.io
* 
* Project Limitless is free software: you can redistribute it and/or modify
* it under the terms of the Apache License Version 2.0.
* 
* You should have received a copy of the Apache License Version 2.0 with
* Project Limitless. If not, see http://www.apache.org/licenses/LICENSE-2.0.
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Limitless.Builtin;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace Limitless.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IIdentityProvider"/>.
    /// </summary>
    public static class IIdentityProviderExtensions
    {
        /// <summary>
        /// Creates the required routes for all IIdentityProvider implementations.
        /// </summary>
        /// <param name="identityProvider">The provider being extended</param>
        /// <param name="handler">A handler to wrap around the call. Takes the output and API input parameters as input.</param>
        /// <returns>The list of API routes</returns>
        public static List<APIRoute> GetRequiredAPIRoutes(this IIdentityProvider identityProvider, Func<dynamic, object[], dynamic> handler = null)
        {
            var routes = new List<APIRoute>();

            // For the IIdentityProvider interface we need to add routes to the API
            var route = new APIRoute
            {
                Path = "/login",
                Description = "Log a user in",
                Method = HttpMethod.Post,
                Handler = (APIRequest request) =>
                {
                    // RequiredFields property is only implemented on the APIRouteAttribute
                    // so I have to manually do the checks for required interface routes
                    if (request.Data.username == null || request.Data.password == null)
                    {
                        throw new MissingFieldException("Username and password must not be null");
                    }

                    // Try..Catch as I'm calling user code here
                    var apiResponse = new APIResponse();

                    var loginResult = identityProvider.Login((string) request.Data.username, (string) request.Data.password);
                    if (loginResult.IsAuthenticated == false)
                    {
                        apiResponse.StatusCode = (int) HttpStatusCode.Unauthorized;
                        apiResponse.StatusMessage = "Authentication required";
                        apiResponse.Data = loginResult.ErrorResponse;
                    }
                    else
                    {
                        apiResponse.Data = loginResult.User;
                    }
                    
                    if (handler == null)
                        return apiResponse;

                    // TODO: the analysis module hook needs to be implemented in a better way
                    // Hiding the password
                    request.Data.password = "*******";
                    return handler(apiResponse, new object[] {request});
                }
            };
            routes.Add(route);
            
            return routes;
        }
    }
}