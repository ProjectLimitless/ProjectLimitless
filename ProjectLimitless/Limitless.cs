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

using NLog;
using Nancy.Security;
using Nancy.Hosting.Self;

using Limitless.Config;
using Limitless.Loaders;
using Limitless.Runtime;
using Limitless.Managers;
using Limitless.Runtime.Types;
using Limitless.Runtime.Attributes;

namespace Limitless
{
    /// <summary>
    /// Limitless is the core application.
    /// </summary>
    public class Limitless
    {
        /// <summary>
        /// NLog logger.
        /// </summary>
        public Logger _log;
        /// <summary>
        /// Manager of the modules.
        /// </summary>
        private ModuleManager _moduleManager;
        
        /// <summary>
        /// Constructor taking the configuration to be used.
        /// </summary>
        /// <param name="settings">The configuration to be used</param>
        public Limitless(LimitlessSettings settings, Logger log)
        {
            _log = log;
            log.Debug("Configuring Project Limitless...");
            log.Info($"Settings| Default system name set as {settings.Core.Name}");
            log.Info($"Settings| {settings.Core.EnabledModules.Length} module(s) will be loaded");

            _moduleManager = new ModuleManager(settings.FullConfiguration, _log);
            foreach (string moduleName in settings.Core.EnabledModules)
            {
                IModule module = _moduleManager.Load(moduleName);

                // TODO: move inject here


                // Get the methods marked as APIRoutes for extending the API
                /*MethodInfo[] methods = module.GetType().GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(APIRouteAttribute), false).Length > 0)
                        .ToArray();

                foreach (MethodInfo methodInfo in methods)
                {
                    APIRouteAttribute attributes = (APIRouteAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(APIRouteAttribute));
                    APIRoute extendHandler = new APIRoute();
                    extendHandler.Path = attributes.Path;
                    extendHandler.Method = attributes.Method;
                    extendHandler.RequiresAuthentication = attributes.RequiresAuthentication;
                    extendHandler.Handler = (dynamic input) =>
                    {
                        dynamic result = (dynamic)methodInfo.Invoke(module, new object[] { input });
                        return result;
                    };
                    RouteManager.Instance.Routes.Add(extendHandler);
                    log.Debug($"Added API route '{extendHandler.Path}'");
                }

                // I need the content routes if this is a UI module
                if (typeof(IUIModule).IsAssignableFrom(module.GetType()))
                {
                    IUIModule uiModule = (IUIModule)module;
                    RouteManager.Instance.ContentRoutes.Add(uiModule.GetContentPath());
                    log.Debug($"Added Content route '{uiModule.GetContentPath()}'");
                }
                */
            }
            
        }

        /// <summary>
        /// Run executes the main loop.
        /// </summary>
        public void Run()
        {
            HostConfiguration config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = true;
            using (var host = new NancyHost(config, new Uri("http://localhost:1234")))
            {
                host.Start();
                _log.Info("Running on :1234");
                Console.ReadLine();
            }
        }
    }
}
