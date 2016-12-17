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

using Nancy.Hosting.Self;

using Limitless.Config;
using Limitless.Managers;
using Limitless.Runtime.Types;
using Limitless.Runtime.Attributes;
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
                    if (RouteManager.Instance.ContentRoutes.Contains(contentPath))
                    {
                        _log.Critical($"Previously loaded IUIModule already uses the content path {contentPath}");
                        throw new NotSupportedException($"Previously loaded IUIModule already uses the content path {contentPath}");
                    }
                    RouteManager.Instance.ContentRoutes.Add(contentPath);
                }
                // TODO: Add decorating to interfaces with required paths


                // Check for APIRouteAttributes and add the routes that extend the API
                MethodInfo[] methods = module.GetType().GetMethods()
                        .Where(m => m.GetCustomAttributes(typeof(APIRouteAttribute), false).Length > 0)
                        .ToArray();

                foreach (MethodInfo methodInfo in methods)
                {
                    APIRouteAttribute attributes = Attribute.GetCustomAttribute(methodInfo, typeof(APIRouteAttribute)) as APIRouteAttribute;
                    APIRoute extendHandler = new APIRoute();
                    extendHandler.Path = attributes.Path;
                    extendHandler.Method = attributes.Method;
                    extendHandler.RequiresAuthentication = attributes.RequiresAuthentication;
                    extendHandler.Handler = (dynamic input) =>
                    {
                        dynamic result = (dynamic)methodInfo.Invoke(module, new object[] { input });
                        return result;
                    };
                    if (RouteManager.Instance.AddRoute(extendHandler))
                    {
                        _log.Debug($"Added API route '{extendHandler.Path}' for module '{moduleName}'");
                    }
                    else
                    {
                        _log.Warning($"API route '{extendHandler.Path}' for module '{moduleName}' could not be added");
                    }
                }
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
