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
using System.Dynamic;
using System.Collections.Generic;

using Newtonsoft.Json;

using Nancy;
using Nancy.Security;
using Nancy.Extensions;
using Nancy.Authentication.Stateless;

using Limitless.Builtin;
using Limitless.Managers;
using Limitless.Containers;
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
                // Try..Catch as I am calling user code here
                try
                { 
                    var jwtToken = ctx.Request.Headers.Authorization;
                    if (string.IsNullOrEmpty(jwtToken))
                    {
                        return null;
                    }

                    var loginResult = identityProvider.TokenLogin(jwtToken);
                    if (loginResult.IsAuthenticated)
                    {
                        return InternalUserIdentity.Wrap(loginResult.User);
                    }
                    ctx.Response = new Nancy.Response();
                    ctx.Response.StatusCode = HttpStatusCode.Unauthorized;
                    ctx.Response.ReasonPhrase = loginResult.ErrorResponse;
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            });
            StatelessAuthentication.Enable(this, configuration);

            foreach (var route in routeManager.GetRoutes())
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
                    case HttpMethod.Head:
                        Head[route.Path] = BuildComposedFunction(route);
                        break;
                }
                Options[route.Path] = BuildRouteOptions(route);
            }

            /*
            TODO: Testing routes for error pages
            Get["/"] = _ =>
            {
                return "Hello";
            };
            Get["/ex"] = _ =>
            {
                throw new NullReferenceException("Error is null");
            };*/
            
            /*
            // TODO: This should be hooked in the analysis
            this.Before += (NancyContext ctx) =>
            {
                Nancy.Routing.Route r = (Nancy.Routing.Route)ctx.ResolvedRoute;
                Console.WriteLine(r.Action);
                Console.WriteLine("This is before API");
                return null;
            };

            this.Before += (NancyContext ctx) =>
            {
                Console.WriteLine("This is after API");
                return null;
            };
            */
        }

        /// <summary>
        /// Constructs the handler function for the route options.
        /// </summary>
        /// <param name="route">The route to build the OPTIONS for</param>
        /// <returns>the OPTIONS response</returns>
        private Func<dynamic, dynamic> BuildRouteOptions(APIRoute route)
        {
            return (dynamic parameters) =>
            {
                var negotiator = Negotiate.WithStatusCode(200);
                negotiator.WithHeader("Access-Control-Allow-Origin", CoreContainer.Instance.Settings.Core.API.CORS.AllowedOrigin);

                // This fetches all the HTTP methods available to the routes 
                // that match route.Path and adds it to the response headers
                var methods = string.Join(",", CoreContainer.Instance.RouteManager.GetRoutes()
                                    .Where(x => x.Path == route.Path)
                                    .Select(x => Enum.GetName(typeof(HttpMethod), x.Method).ToUpper()));
                negotiator.WithHeader("Access", methods);
                negotiator.WithHeader("Access-Control-Allow-Methods", methods);

                // Let's allow all the request headers for now
                var headers = new List<string>(Request.Headers.Keys);
                if (Request.Headers.Keys.Contains("Access-Control-Request-Headers"))
                {
                    var acrh = Request.Headers["Access-Control-Request-Headers"].First();
                    headers.AddRange(acrh.Split(','));
                }
                // and add some ones we know we need
                headers.Add("Accept-Language");
                headers.Add("Request-Language");
                headers.Add("Content-Type");
                negotiator.WithHeader("Access-Control-Allow-Headers", string.Join(",", headers));

                // Always allow credentials
                negotiator.WithHeader("Access-Control-Allow-Credentials", "true");

                // If the route requires auth, then OPTIONS 
                // should require it to
                if (route.RequiresAuthentication)
                {
                    this.RequiresAuthentication();
                }
                return negotiator;
            };
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
                var request = new APIRequest(); 
                if (route.RequiresAuthentication)
                {
                    this.RequiresAuthentication();
                    request.AuthenticatedUser = ((InternalUserIdentity)Context.CurrentUser).Meta;
                }

                // Parse the post data, if it's JSON, deserialize into a dynamic
                dynamic postData;
                if (Request.Headers.ContentType == MimeType.Json)
                {
                    postData = JsonConvert.DeserializeObject(Request.Body.AsString());
                }
                else postData = Request.Body.AsString();

                // Build the request for API use
                request.Data = postData;
                request.Parameters = parameters;
                request.Headers = CloneHeaders(Request.Headers);
                
                var negotiator = Negotiate.WithStatusCode(200);
                try
                {
                    var handlerResponse = route.Handler(request);
                    if (handlerResponse is APIResponse)
                    {
                        var apiResponse = handlerResponse as APIResponse;
                        negotiator.WithStatusCode(apiResponse.StatusCode);

                        if (apiResponse.Data != null)
                        {
                            negotiator.WithModel((object)apiResponse.Data);
                        }
                        if (apiResponse.StatusMessage != null)
                        {
                            negotiator.WithReasonPhrase(apiResponse.StatusMessage);
                        }
                        if (apiResponse.Headers.Count > 0)
                        {
                            negotiator.WithHeaders(apiResponse.Headers.ToArray());
                        }       
                    }
                    else
                    {
                        if (handlerResponse != null)
                        {
                            negotiator.WithModel((object)handlerResponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    dynamic exceptionResponse = new ExpandoObject();
                    exceptionResponse.Type = ex.GetType();
                    exceptionResponse.Message = ex.Message;
                    exceptionResponse.StackTrace = ex.StackTrace;
                    exceptionResponse.Target = $"{ex.Source}.{ex.TargetSite.Name}";
                    if (ex.InnerException != null)
                    {
                        exceptionResponse.InnerException = ex.InnerException.Message;
                    }
                    negotiator
                        .WithModel((object)exceptionResponse)
                        .WithStatusCode(500);
                }

                return negotiator;
            };
        }

        /// <summary>
        /// Copies Nancy headers into Runtime headers to
        /// avoid creating dependencies for modules. 
        /// </summary>
        /// <param name="headers">The original NancyFx headers</param>
        /// <returns>The Runtime headers</returns>
        private NancyRequestHeaders CloneHeaders(RequestHeaders headers)
        {
            Dictionary<string, IEnumerable<string>> newHeaders = new Dictionary<string, IEnumerable<string>>();
            foreach (string header in headers.Keys)
            {
                newHeaders.Add(header, headers[header]);
            }
            return new NancyRequestHeaders(newHeaders);
        }
    }
}
