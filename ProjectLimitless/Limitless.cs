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

using Nancy.Hosting.Self;

using Limitless.Config;
using Limitless.Builtin;
using Limitless.Managers;
using Limitless.Extensions;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

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
        public ILogger _log;
        /// <summary>
        /// Manager of the modules.
        /// </summary>
        private ModuleManager _moduleManager;
        /// <summary>
        /// Provides administration functions.
        /// TODO: Mode to separate module?
        /// </summary>
        private AdminModule _adminModule;

        /// <summary>
        /// Constructor taking the configuration to be used.
        /// </summary>
        /// <param name="settings">The configuration to be used</param>
        public Limitless(LimitlessSettings settings, ILogger log)
        {
            _log = log;
            log.Debug("Configuring Project Limitless...");
            log.Info($"Settings| Default system name set as {settings.Core.Name}");
            log.Info($"Settings| {settings.Core.EnabledModules.Length} module(s) will be loaded");

            _moduleManager = new ModuleManager(settings.FullConfiguration, _log);
            foreach (string moduleName in settings.Core.EnabledModules)
            {
                IModule module = _moduleManager.Load(moduleName);
                if (module == null)
                {
                    _log.Error($"Unable to load module '{moduleName}'");
                    continue;
                }

                _log.Info($"Loaded module '{moduleName}'. Checking interface types...");

                if (module is ILogger)
                {
                    _log = (ILogger)module;
                    _log.Info($"Loaded module '{moduleName}' implements ILogger, replaced Bootstrap logger");
                }
                else if (module is IUIModule)
                {
                    // Multiple UI modules is allowed, we can add all their paths
                    IUIModule ui = module as IUIModule;
                    string contentPath = ui.GetContentPath();
                    if (RouteManager.Instance.AddContentRoute(contentPath))
                    {
                        _log.Debug($"Added content path '{contentPath}' for module '{moduleName}'");
                    }
                    else
                    {
                        _log.Critical($"Previously loaded IUIModule already uses the content path '{contentPath}' specified in '{moduleName}'");
                        throw new NotSupportedException($"Previously loaded IUIModule already uses the content path '{contentPath}' specified in '{moduleName}'");
                    }
                }
                // TODO: Add decorating to interfaces with required paths

                List<APIRoute> moduleRoutes = module.GetAPIRoutes();
                if (RouteManager.Instance.AddRoutes(moduleRoutes))
                {
                    _log.Info($"Added {moduleRoutes.Count} new API routes for module '{moduleName}'");
                }
                else
                {
                    _log.Warning($"Unable to add all API routes for module '{moduleName}'. Possible duplicate route and method.");
                }
            }

            //TODO: Setup the admin API - move to own module
            _adminModule = new AdminModule(_log);
            List<APIRoute> routes = ((IModule)_adminModule).GetAPIRoutes();
            if (RouteManager.Instance.AddRoutes(routes))
            {
                _log.Info($"Added {routes.Count} new API routes for module 'AdminModule'");
            }
            else
            {
                _log.Warning($"Unable to add all API routes for module 'AdminModule'. Possible duplicate route and method.");
            }

            //TODO: Setup the diagnostics
        }

        /// <summary>
        /// Run executes the main loop.
        /// </summary>
        public void Run()
        {
            HostConfiguration config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = true;
            // TODO: Use API host from config
            using (var host = new NancyHost(config, new Uri("http://192.168.1.3:1234")))
            {
                host.Start();
                _log.Info("Running on :1234");
                Console.ReadLine();
            }
        }
    }
}
