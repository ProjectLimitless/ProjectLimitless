

using Limitless.Runtime.Attributes;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Types;
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
        /// <param name="handler">A handler to wrap around the module invoke</param>
        /// <returns>The list of API routes</returns>
        public static List<APIRoute> GetAPIRoutes(this IModule module, Func<dynamic, dynamic> handler = null)
        {
            List<APIRoute> apiRoutes = new List<APIRoute>();

            // Check for APIRouteAttributes and add the routes that extend the API
            MethodInfo[] methods = module.GetType().GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(APIRouteAttribute), false).Length > 0)
                    .ToArray();

            foreach (MethodInfo methodInfo in methods)
            {
                APIRouteAttribute attributes = Attribute.GetCustomAttribute(methodInfo, typeof(APIRouteAttribute)) as APIRouteAttribute;
                APIRoute route = new APIRoute();
                route.Path = attributes.Path;
                route.Method = attributes.Method;
                route.Description = attributes.Description;
                route.RequiresAuthentication = attributes.RequiresAuthentication;
                route.Handler = (dynamic parameters, dynamic postData) =>
                {
                    dynamic result;
                    if (handler != null)
                    {
                        // TODO: Ensure we can chain multiple methods
                        result = handler(methodInfo.Invoke(module, new object[] { parameters, postData }));
                    }
                    else
                    {
                        result = (dynamic)methodInfo.Invoke(module, new object[] { parameters, postData });
                    }
                    return result;
                };
                apiRoutes.Add(route);
            }

            return apiRoutes;
        }
    }
}
