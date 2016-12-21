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
using System.Collections.Generic;

using Nancy;
using Nancy.Security;
using Nancy.Extensions;
using Nancy.Authentication.Stateless;

using Newtonsoft.Json;

using Limitless.Managers;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;

namespace Limitless
{
    /// <summary>
    /// TODO: Move this
    /// </summary>
    public class JwtToken
    {
        public string sub;
        public long exp;
    }

    /// <summary>
    /// TODO: Move this
    /// </summary>
    public class TestUser : IUserIdentity
    {
        public IEnumerable<string> Claims
        {
            get
            {
                return new string[] { "one" };
            }
        }

        public string UserName
        {
            get
            {
                return "TestUSer";
            }
        }
    }

    /// <summary>
    /// The core API. Composed of the base and all the
    /// extended API modules.
    /// </summary>
    public class ComposedAPI : NancyModule
    {
        /// <summary>
        /// Standard Constructor.
        /// </summary>
        public ComposedAPI(RouteManager routeManager)
        {
            // Setup the JWT authentication for routes that require it.
            var configuration = new StatelessAuthenticationConfiguration(ctx =>
            {
                var jwtToken = ctx.Request.Headers.Authorization;
                try
                {
                    var payload = Jose.JWT.Decode<JwtToken>(jwtToken, "mysecretkey");
                    var tokenExpires = DateTime.FromBinary(payload.exp);
                    if (tokenExpires > DateTime.UtcNow)
                    {
                        return new TestUser();
                    }
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
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
                if (route.RequiresAuthentication)
                {
                    this.RequiresAuthentication();
                    // TODO: Find a way to get this.Context.CurrentUser to the module route
                }
                dynamic postData = JsonConvert.DeserializeObject(Request.Body.AsString());
                return route.Handler(parameters, postData);
            };
        }
    }
}
