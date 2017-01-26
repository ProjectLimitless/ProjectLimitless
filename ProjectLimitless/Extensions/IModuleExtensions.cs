/** 
* This file is part of Project Limitless.
* Copyright © 2016 Donovan Solms.
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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Limitless.Builtin;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Attributes;
using Limitless.Runtime.Interfaces;

namespace Limitless.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IModule"/>.
    /// </summary>
    public static class IModuleExtensions
    {
        /// <summary>
        /// Gets all the methods marked with <see cref="APIRouteAttribute"/> in the given module
        /// and returns the usable API routes for the module.
        /// </summary>
        /// <param name="type">The module to get routes from</param>
        /// <param name="handler">A handler to wrap around the module invoke. Takes the output and API input parameters as input.</param>
        /// <returns>The list of API routes</returns>
        public static List<APIRoute> GetAPIRoutes(this IModule module, Func<dynamic, object[], dynamic> handler = null)
        {
            var apiRoutes = new List<APIRoute>();

            // Check for APIRouteAttributes and add the routes that extend the API
            var methods = module.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(APIRouteAttribute), false).Length > 0)
                    .ToArray();

            foreach (MethodInfo methodInfo in methods)
            {
                var attributes = Attribute.GetCustomAttribute(methodInfo, typeof(APIRouteAttribute)) as APIRouteAttribute;
                var route = new APIRoute();
                route.Path = attributes.Path;
                route.Method = attributes.Method;
                route.Description = attributes.Description;
                route.RequiresAuthentication = attributes.RequiresAuthentication;
                route.Handler = (APIRequest request) =>
                {
                    // For Get and Delete I'll only send the parameters
                    // For Post and Put I'll send parameters and postData to the method
                    // For authenticated routes I'll add the user object to the method
                    var invokeParameters = new List<object>();
                    invokeParameters.Add(request);
                    if (route.Method == HttpMethod.Post || route.Method == HttpMethod.Put)
                    {
                        // Check if postData contains required fields
                        if (attributes.RequiredFields.Length > 0)
                        {
                            // There are required fields, but postData is null
                            if (request.Data == null)
                            {
                                throw new NullReferenceException("POST data is required for this route");
                            }
                            if (request.Headers.ContentType == MimeType.Json)
                            {
                                IDictionary<string, JToken> lookup = request.Data;
                                foreach (string requiredField in attributes.RequiredFields)
                                {
                                    if (lookup.ContainsKey(requiredField) == false)
                                    {
                                        throw new MissingFieldException($"Required data field '{requiredField}' was not found in the POST data");
                                    }
                                }
                            }
                        }
                    }
                    
                    dynamic result;
                    // TODO: the analysis module hook needs to be implemented in a better way
                    if (handler != null)
                    {
                        // TODO: Ensure we can chain multiple methods
                        result = handler(methodInfo.Invoke(module, invokeParameters.ToArray()), invokeParameters.ToArray());
                    }
                    else
                    {
                        result = (dynamic)methodInfo.Invoke(module, invokeParameters.ToArray());
                    }
                    return result;
                };
                apiRoutes.Add(route);
            }
            return apiRoutes;
        }
    }
}