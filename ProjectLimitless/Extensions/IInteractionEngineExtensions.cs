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
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Limitless.Builtin;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Interactions;
using System.Net;

namespace Limitless.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IInteractionEngine"/>.
    /// </summary>
    public static class IInteractionEngineExtensions
    {
        /// <summary>
        /// Creates the required routes for all IInteractionEngine implementations.
        /// </summary>
        /// <param name="type">The module to get routes from</param>
        /// <param name="handler">A handler to wrap around the call. Takes the output and API input parameters as input.</param>
        /// <returns>The list of API routes</returns>
        public static List<APIRoute> GetRequiredAPIRoutes(this IInteractionEngine interactionEngine, Func<dynamic, object[], dynamic> handler = null)
        {
            var routes = new List<APIRoute>();

            // Create the Skills endpoints
            var route = new APIRoute();
            route.Path = "/skills";
            route.Description = "List available skills";
            route.Method = HttpMethod.Get;
            route.RequiresAuthentication = true;
            route.Handler = (dynamic parameters, dynamic postData, dynamic user) =>
            {
                return new APIResponse(interactionEngine.ListSkills());
            };
            routes.Add(route);

            route = new APIRoute();
            route.Path = "/skills";
            route.Description = "Register a new skill";
            route.Method = HttpMethod.Post;
            route.RequiresAuthentication = true;
            route.Handler = (dynamic parameters, dynamic postData, dynamic user) =>
            {
                Skill skill = ((JObject)postData).ToObject<Skill>();
                switch (skill.Binding)
                {
                    case SkillExecutorBinding.Builtin:
                    case SkillExecutorBinding.Module:
                        skill.Executor = ((JObject)skill.Executor).ToObject<BinaryExecutor>();
                        break;
                    case SkillExecutorBinding.Network:
                        skill.Executor = ((JObject)skill.Executor).ToObject<NetworkExecutor>();
                        break;
                    default:
                        throw new NotImplementedException($"The given skill binding '{skill.Binding}' is not implemented");
                }

                APIResponse response = new APIResponse();
                if (interactionEngine.RegisterSkill(skill))
                {
                    response.StatusCode = (int)HttpStatusCode.Created;
                    response.Data = new
                    {
                        UUID = skill.UUID,
                        Registered = true
                    };
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Data = new
                    {
                        Registered = false,
                        Reason = $"The skill '{skill.UUID}' has already been registered"
                    };
                }
                return response;
            };
            routes.Add(route);

            route = new APIRoute();
            route.Path = "/skills/{skillUUID}";
            route.Description = "Deregister a new skill";
            route.Method = HttpMethod.Delete;
            route.RequiresAuthentication = true;
            route.Handler = (dynamic parameters, dynamic postData, dynamic user) =>
            {
                APIResponse response = new APIResponse();
                if (interactionEngine.DeregisterSkill(parameters.skillUUID))
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusMessage = $"The skill '{parameters.skillUUID}' was not found or has already been deregistered";
                }
                return response;
            };
            routes.Add(route);




            // TODO: Figure out how to bind this handler correctly!
            if (handler != null)
            {
                // Attach the specified handler to the routes. I'm doing
                // it here to keep the route.Handler code simple.
                foreach (APIRoute apiRoute in routes)
                {
                    //apiRoute.Handler = handler(apiRoute.Handler);
                }
            }



            return routes;
        }
    }
}