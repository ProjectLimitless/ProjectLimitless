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
using System.Dynamic;
using System.Collections.Generic;

using Limitless.Managers;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Attributes;

namespace Limitless.Builtin
{
    internal class AdminModule : IModule
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Standard constructor with log.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public AdminModule(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Returns a list of available API routes.
        /// </summary>
        /// <param name="input">API input, null in this case</param>
        /// <returns>The list of routes and their descriptions</returns>
        [APIRoute(Path = "/admin/routes", Method = HttpMethod.Get, Description = "Shows all the available routes of the API")]
        public dynamic RoutesList(dynamic input)
        {
            List<dynamic> routes = new List<dynamic>();
            foreach (APIRoute route in RouteManager.Instance.GetRoutes())
            {
                dynamic routeInfo = new ExpandoObject();
                routeInfo.Path = route.Path;
                routeInfo.Method = route.Method.ToString().ToUpper();
                routeInfo.Description = route.Description;
                routeInfo.RequiresAuthentication = route.RequiresAuthentication;
                if (routeInfo.Description == "")
                {
                    routeInfo.Description = "No description provided";
                }
                routes.Add(routeInfo);
            }
            return routes;
        }

        //TODO: Move this module to a separate assembly?
        public void Configure(dynamic settings)
        {
            throw new NotImplementedException();
        }

        public Type GetConfigurationType()
        {
            throw new NotImplementedException();
        }
    }
}
