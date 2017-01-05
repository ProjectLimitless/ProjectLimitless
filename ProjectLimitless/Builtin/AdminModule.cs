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
using System.Reflection;
using System.Collections.Generic;

using Limitless.Containers;
using Limitless.Runtime.Enums;
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
        /// <param name="parameters">API route parameters</param>
        /// <param name="user">The authenticated user</param>
        /// <returns>The list of routes and their descriptions</returns>
        [APIRoute(
            Path = "/admin/routes", 
            Method = HttpMethod.Get, 
            Description = "Returns a list of all the available routes of the API",
            RequiresAuthentication = true
        )]
        public dynamic RoutesList(dynamic parameters, dynamic user)
        {
            List<dynamic> routes = new List<dynamic>();
            foreach (APIRoute route in CoreContainer.Instance.RouteManager.GetRoutes())
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

        /// <summary>
        /// Returns a list of loaded modules.
        /// </summary>
        /// <param name="parameters">API route parameters</param>
        /// <param name="user">The authenticated user</param>
        /// <returns>The list of loaded modules and their descriptions</returns>
        [APIRoute(
            Path = "/admin/modules",
            Method = HttpMethod.Get,
            Description = "Returns a list of all the loaded modules for the installation",
            RequiresAuthentication = true
        )]
        public dynamic ModulesList(dynamic parameters, dynamic user)
        {
            List<dynamic> modules = new List<dynamic>();
            foreach (KeyValuePair<Type, List<IModule>> kvp in CoreContainer.Instance.ModuleManager.Modules)
            {
                foreach (IModule module in kvp.Value)
                {
                    dynamic moduleInfo = new ExpandoObject();
                    
                    moduleInfo.Type = kvp.Key.Name;
                    moduleInfo.Title = module.GetTitle();
                    moduleInfo.Author = module.GetAuthor();
                    moduleInfo.Version = module.GetVersion();
                    moduleInfo.Description = module.GetDescription();
                    modules.Add(moduleInfo);
                }
            }
            return modules;
        }

        /// <summary>
        /// Returns a list of users.
        /// </summary>
        /// <param name="parameters">API route parameters</param>
        /// <param name="user">The authenticated user</param>
        /// <returns>The list of users</returns>
        [APIRoute(
            Path = "/admin/users",
            Method = HttpMethod.Get,
            Description = "Returns a list of users for the installation",
            RequiresAuthentication = true
        )]
        public dynamic UsersList(dynamic parameters, dynamic user)
        {
            return CoreContainer.Instance.IdentityProvider.List();
        }

        //TODO: Load as proper builtin modulel
        public void Configure(dynamic settings)
        {
            throw new NotImplementedException();
        }

        public Type GetConfigurationType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetTitle"/>
        /// </summary>
        public string GetTitle()
        {
            var assembly = typeof(AdminModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attribute != null)
            {
                return attribute.Title;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            var assembly = typeof(AdminModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (attribute != null)
            {
                return attribute.Company;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetVersion"/>
        /// </summary>
        public string GetVersion()
        {
            var assembly = typeof(AdminModule).Assembly;
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetDescription"/>
        /// </summary>
        public string GetDescription()
        {
            var assembly = typeof(AdminModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
            return "Unknown";
        }
    }
}
