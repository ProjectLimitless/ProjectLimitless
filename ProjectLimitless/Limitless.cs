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
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Nancy.Hosting.Self;

using Limitless.Config;
using Limitless.Builtin;
using Limitless.Managers;
using Limitless.Containers;
using Limitless.Extensions;
using Limitless.Runtime.Enums;
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
        /// The loaded settings.
        /// </summary>
        private LimitlessSettings _settings;

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
            _settings = settings;
            _log = log;
            
            log.Debug("Configuring Project Limitless...");
            log.Info($"Settings| Default system name set as {settings.Core.Name}");
            log.Info($"Settings| {settings.Core.EnabledModules.Length} module(s) will be loaded");

            _moduleManager = new ModuleManager(settings.FullConfiguration, _log);
            foreach (string moduleName in settings.Core.EnabledModules)
            {
                IModule module = null;
                try
                {
                    module = _moduleManager.Load(moduleName);

                }
                catch (DllNotFoundException ex)
                {
                    _log.Warning($"Unable to load module '{ex.Message}', attempting to load as builtin module");
                    // Create a type from the builtin module name
                    Type builtinType = Type.GetType(moduleName, true, false);
                    module = _moduleManager.LoadBuiltin(moduleName, builtinType);
                }

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
                    if (CoreContainer.Instance.RouteManager.AddContentRoute(contentPath))
                    {
                        _log.Debug($"Added content path '{contentPath}' for module '{moduleName}'");
                    }
                    else
                    {
                        _log.Critical($"Previously loaded IUIModule already uses the content path '{contentPath}' specified in '{moduleName}'");
                        throw new NotSupportedException($"Previously loaded IUIModule already uses the content path '{contentPath}' specified in '{moduleName}'");
                    }
                }
                else if (module is IIdentityProvider)
                {
                    IIdentityProvider identityProvider = module as IIdentityProvider;
                    // For the IIdentityProvider interface we need to add routes to the API
                    APIRoute userRoute = new APIRoute();
                    userRoute.Path = "/login";
                    userRoute.Description = "Log a user in";
                    userRoute.Method = HttpMethod.Post;
                    // TODO: Relook where this handler should be defined
                    userRoute.Handler = (dynamic parameters, dynamic postData, dynamic user) =>
                    {
                        BaseUser baseUser = identityProvider.Login((string)postData.username, (string)postData.password);
                        APIResponse apiResponse = new APIResponse();
                        if (baseUser == null)
                        {
                            apiResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                            apiResponse.StatusMessage = "Username of password is incorrect";
                        }
                        else
                        {
                            apiResponse.Data = baseUser;
                        }
                        return apiResponse;
                    };
                    CoreContainer.Instance.RouteManager.AddRoute(userRoute);
                    CoreContainer.Instance.IdentityProvider = identityProvider;
                }

                // TODO: Add decorating to interfaces with required paths

                List<APIRoute> moduleRoutes = module.GetAPIRoutes();
                if (CoreContainer.Instance.RouteManager.AddRoutes(moduleRoutes))
                {
                    // TODO: Remove - only for testing - debug output
                    foreach (APIRoute route in moduleRoutes)
                    {
                        _log.Debug($"Added route '{route.Path}' for module '{moduleName}'");
                    }
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
            if (CoreContainer.Instance.RouteManager.AddRoutes(routes))
            {
                // TODO: Remove - only for testing
                foreach (APIRoute route in routes)
                {
                    _log.Debug($"Added route '{route.Path}' for module 'AdminModule'");
                }
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

            List<Uri> bindingAddresses = new List<Uri>();
            // If the host is set to 0.0.0.0, we bind to all the available IP addresses
            if (_settings.Core.API.Host == "0.0.0.0")
            {
                bindingAddresses = GetUriParams(_settings.Core.API.Port);
            }
            else
            {
                bindingAddresses.Add(new Uri($"http://{_settings.Core.API.Host}:{_settings.Core.API.Port}"));
            }

            using (var host = new NancyHost(config, bindingAddresses.ToArray()))
            {
                host.Start();
                _log.Info($"API is running on '{ String.Join(", ", bindingAddresses) }'");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Returns all the available IP addresses for the host.
        /// 
        /// Slightly modified from the original version available at
        /// https://www.codeproject.com/articles/694907/embed-a-web-server-in-a-windows-service.
        /// </summary>
        /// <param name="port">The port to bind to</param>
        /// <returns>An array of <see cref="Uri"/> with bindable addresses</returns>
        private List<Uri> GetUriParams(int port)
        {
            var uriParams = new List<Uri>();
            string hostName = Dns.GetHostName();

            // Host name URI
            string hostNameUri = string.Format("http://{0}:{1}", Dns.GetHostName(), port);
            uriParams.Add(new Uri(hostNameUri));

            // Host address URI(s)
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ipAddress in hostEntry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)  // IPv4 addresses only
                {
                    var addrBytes = ipAddress.GetAddressBytes();
                    string hostAddressUri = string.Format("http://{0}.{1}.{2}.{3}:{4}",
                    addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3], port);
                    uriParams.Add(new Uri(hostAddressUri));
                }
            }

            // Localhost URI
            uriParams.Add(new Uri(string.Format("http://localhost:{0}", port)));
            return uriParams;
        }
    }
}
