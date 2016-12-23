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

using Nancy;
using Nancy.Security;
using Nancy.Extensions;
using Nancy.Authentication.Stateless;

using Newtonsoft.Json;

using Limitless.Builtin;
using Limitless.Managers;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace Limitless
{
    /// <summary>
    /// The core API. Composed of the base and all the
    /// extended API modules.
    /// </summary>
    public class ComposedAPI : NancyModule
    {
        /// <summary>
        /// Standard Constructor with injected parameters.
        /// </summary>
        public ComposedAPI(RouteManager routeManager, IIdentityProvider identityProvider)
        {
            // Setup the JWT authentication for routes that require it.
            var configuration = new StatelessAuthenticationConfiguration(ctx =>
            {
                var jwtToken = ctx.Request.Headers.Authorization;
                BaseUser baseUser = identityProvider.ValidateToken(jwtToken);
                return InternalUserIdentity.Wrap(baseUser);
            });
            StatelessAuthentication.Enable(this, configuration);

            foreach (APIRoute route in routeManager.GetRoutes())
            {
                switch (route.Method)
                {
                    case HttpMethod.Get:
                        Get[route.Path] = BuildComposedFunction(route);
                        break;
                    case HttpMethod.Post:
                        Post[route.Path] = BuildComposedFunction(route);
                        break;
                    case HttpMethod.Put:
                        Put[route.Path] = BuildComposedFunction(route);
                        break;
                    case HttpMethod.Delete:
                        Delete[route.Path] = BuildComposedFunction(route);
                        break;
                }
            }
        }

        /// <summary>
        /// Constructs the handler function including authentication.
        /// </summary>
        /// <param name="route">The route to build</param>
        /// <returns>The constructed function</returns>
        private Func<dynamic, dynamic> BuildComposedFunction(APIRoute route)
        {
            return (dynamic parameters) =>
            {
                InternalUserIdentity internalUser = null;
                if (route.RequiresAuthentication)
                {
                    this.RequiresAuthentication();
                    internalUser = (InternalUserIdentity)Context.CurrentUser;
                }
                dynamic postData = JsonConvert.DeserializeObject(Request.Body.AsString());
                return route.Handler(parameters, postData, internalUser);
            };
        }
    }
}
