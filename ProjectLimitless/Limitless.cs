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
using System.Linq;
using System.Collections.Generic;

using NLog;
using Nancy.Hosting.Self;

using Limitless.Config;
using Limitless.Loaders;
using Limitless.Runtime;
using Limitless.Runtime.Attributes;
using Nancy;
using Nancy.TinyIoc;

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
        /// Loader of the modules.
        /// </summary>
        private ModuleLoader _moduleLoader;
        
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

            _moduleLoader = new ModuleLoader(settings.FullConfiguration, _log);
            foreach (string moduleName in settings.Core.EnabledModules)
            {
                IModule mod = _moduleLoader.Load(moduleName);
                // Get the methods marked as APIRoutes for extending the API
                var methods = mod.GetType().GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(APIRouteAttribute), false).Length > 0)
                        .ToArray();

                APIRouteAttribute att = (APIRouteAttribute)Attribute.GetCustomAttribute(methods[0], typeof(APIRouteAttribute));

                

                /*if (typeof(IAPIModule).IsAssignableFrom(mod.GetType()))
                {
                    _log.Info("Module '{0}' implements API extensions", "TestModule");
                    List<APIRouteHandler> handlers = ((IAPIModule)mod).GetRoutes();
                    foreach (APIRouteHandler handler in handlers)
                    {
                        _log.Trace("Route available in Module: {0}", handler.Route);
                    }
                }
                else
                {
                    _log.Debug("Module '{0}' does not implement API extensions", "TestModule");
                }*/
            }

            Runtime.Types.APIRouteHandler handler = new Runtime.Types.APIRouteHandler();
            handler.Route = "/where/{country}";
            handler.Handler = (dynamic input) =>
            {
                return $"{input.country} is nowhere to be seen.";
            };
            RouteLoader.Instance.Routes.Add(handler);
            


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
