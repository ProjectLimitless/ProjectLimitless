﻿/** 
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
using System.Dynamic;

using Newtonsoft.Json;

using Nancy;
using Nancy.Security;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Nancy.Authentication.Stateless;

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
                    case HttpMethod.Options:
                        Options[route.Path] = BuildComposedFunction(route);
                        break;
                }
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
                dynamic postData;

                // TODO: Add all the request headers to parameters with a generic type (for external use)
                if (Request.Headers.ContentType == MimeType.Json)
                {
                    postData = JsonConvert.DeserializeObject(Request.Body.AsString());
                }
                else postData = Request.Body.AsString();
                // Add the content type to the parameters passed to handlers
                parameters.contentType = Request.Headers.ContentType;
                
                var negotiator = Negotiate.WithStatusCode(200);

                try
                {
                    var handlerResponse = route.Handler(parameters, postData, internalUser);
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
    }
}
