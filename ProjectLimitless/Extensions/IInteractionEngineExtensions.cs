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

using Newtonsoft.Json.Linq;

using Limitless.Builtin;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Interactions;

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
            route.Handler = (APIRequest request) =>
            {
                var response = new APIResponse(interactionEngine.ListSkills());

                // TODO: the analysis module hook needs to be implemented in a better way
                if (handler != null)
                {
                    return handler(response, new object[] { request });
                }
                return response;
            };
            routes.Add(route);

            route = new APIRoute();
            route.Path = "/skills";
            route.Description = "Register a new skill";
            route.Method = HttpMethod.Post;
            route.RequiresAuthentication = true;
            route.Handler = (APIRequest request) =>
            {
                Skill skill = ((JObject)request.Data).ToObject<Skill>();
                switch (skill.Binding)
                {
                    /*
                    // TODO: Binary executors can't be loaded over the network using the API
                    // since they need a method handler
                    case SkillExecutorBinding.Builtin:
                    case SkillExecutorBinding.Module:
                        skill.Executor = ((JObject)skill.Executor).ToObject<BinaryExecutor>();
                        break;
                    */
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

                // TODO: the analysis module hook needs to be implemented in a better way
                if (handler != null)
                {
                    return handler(response, new object[] { request });
                }
                return response;
            };
            routes.Add(route);

            route = new APIRoute();
            route.Path = "/skills/{skillUUID}";
            route.Description = "Deregister a new skill";
            route.Method = HttpMethod.Delete;
            route.RequiresAuthentication = true;
            route.Handler = (APIRequest request) =>
            {
                APIResponse response = new APIResponse();
                if (interactionEngine.DeregisterSkill(request.Parameters.skillUUID))
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusMessage = $"The skill '{request.Parameters.skillUUID}' was not found or has already been deregistered";
                }

                // TODO: the analysis module hook needs to be implemented in a better way
                if (handler != null)
                {
                    return handler(response, new object[] { request });
                }
                return response;
            };
            routes.Add(route);
            
            return routes;
        }
    }
}