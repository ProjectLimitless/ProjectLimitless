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
        private ILogger _log;
        /// <summary>
        /// The loaded settings.
        /// </summary>
        private LimitlessSettings _settings;

        /// <summary>
        /// Provides administration functions.
        /// TODO: Mode to separate module?
        /// </summary>
        private AdminModule _adminModule;

        // TODO: Load as proper module?
        private AnalysisModule _analysis;

        /// <summary>
        /// Constructor taking the configuration to be used.
        /// </summary>
        /// <param name="settings">The configuration to be used</param>
        public Limitless(LimitlessSettings settings, ILogger log)
        {
            _settings = settings;
            _log = log;
            _analysis = new AnalysisModule(_log);

            // TODO: Maybe make IOManager just an instance variable?
            CoreContainer.Instance.ModuleManager = new ModuleManager(_settings.FullConfiguration, _log);
            
            CoreContainer.Instance.IOManager = new IOManager(_log);

            // TODO: Rethink inputs
            // Might be audio, text, video, image, gesture, etc
            // Will be from a client app, probably not a user directly
            // Audio might output text but can output intent as well
            // Intent needs to be executed and response needs to be
            // negotiated based on the input
            // maybe a ll-accept header / ll-content-type?

            // Inputs can be sent as raw mime byte data or
            // as part of JSON as base64 encoded fields. 
            // Here I sent up the processing of the input
            // API route that handles the data sent by the client
            // application
            
            // Define the input route
            var inputRoute = new APIRoute();
            inputRoute.Path = "/input";
            inputRoute.Description = "Process the input";
            inputRoute.Method = HttpMethod.Post;
            inputRoute.Handler = CoreContainer.Instance.IOManager.Handle;
            inputRoute.RequiresAuthentication = true;
            CoreContainer.Instance.RouteManager.AddRoute(inputRoute);
            
            Configure();
        }

        /// <summary>
        /// Configure Limitless for startup.
        /// </summary>
        private void Configure()
        {
            _log.Debug("Configuring Project Limitless...");
            _log.Info($"Settings| Default system name set as {_settings.Core.Name}");
            _log.Info($"Settings| {_settings.Core.EnabledModules.Length} module(s) will be loaded");

            foreach (string moduleName in _settings.Core.EnabledModules)
            {
                // Load each module specified in the config. First try to load
                // the module as DLL, if it can't be found, load it as a 
                // builtin module that is part of this assembly.
                IModule module = null;
                try
                {
                    module = CoreContainer.Instance.ModuleManager.Load(moduleName);
                }
                catch (DllNotFoundException ex)
                {
                    _log.Warning($"Unable to load module '{ex.Message}', attempting to load as builtin module");
                    // Create a type from the builtin module name
                    Type builtinType = Type.GetType(moduleName, true, false);
                    module = CoreContainer.Instance.ModuleManager.LoadBuiltin(moduleName, builtinType);
                }

                if (module == null)
                {
                    _log.Error($"Unable to load module '{moduleName}'");
                    continue;
                }

                _log.Info($"Loaded module '{moduleName}'");
                Integrate(moduleName, module);
            }

            //TODO: Setup the admin API - move to own module
            _adminModule = new AdminModule(_log);
            var routes = ((IModule)_adminModule).GetAPIRoutes();
            if (CoreContainer.Instance.RouteManager.AddRoutes(routes))
            {
                _log.Info($"Added {routes.Count} new API routes for module 'AdminModule'");
            }
            else
            {
                _log.Warning($"Unable to add all API routes for module 'AdminModule'. Possible duplicate route and method.");
            }
            
            if (_settings.Core.API.Nancy.DashboardEnabled)
            {
                _log.Warning($"The Nancy dashboard is enabled at '/{_settings.Core.API.Nancy.DashboardPath}'. It should only be enabled for debugging of the API.");
            }

            // TODO: Remove this? Only debug output?
            foreach (var route in CoreContainer.Instance.RouteManager.GetRoutes())
            {
                _log.Debug($"Added route '{route.Path}'");
            }

            //TODO: Setup the diagnostics

            CoreContainer.Instance.Settings = _settings;
        }

        /// <summary>
        /// Run executes the main loop.
        /// </summary>
        public void Run()
        {
            var config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = true;

            var bindingAddresses = new List<Uri>();
            // If the host is set to 0.0.0.0, we bind to all the available IP addresses
            if (_settings.Core.API.Host == "0.0.0.0")
            {
                bindingAddresses = GetAvailableIPs(_settings.Core.API.Port);
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
        /// Integrates the given module with the core by replacing other modules,
        /// configuring routes and setting up wrappers.
        /// </summary>
        /// <param name="moduleName">The name of the loaded module as specified in the config</param>
        /// <param name="module">The loaded module</param>
        /// <exception cref="NotSupportedException">When duplicate content paths are found</exception>
        private void Integrate(string moduleName, IModule module)
        {
            _log.Info($"Integrating module '{moduleName}'");
            
            if (module is ILogger)
            {
                _log = (ILogger)module;
                _log.Info($"Loaded module '{moduleName}' implements ILogger, replaced Bootstrap logger");

                // TODO: Find a better way to reload modules in other modules
                /// See Issue #3 
                /// https://github.com/ProjectLimitless/ProjectLimitless/issues/3
                _analysis.SetLog(_log);
                CoreContainer.Instance.IOManager.SetLog(_log);
            }

            if (module is IUIModule)
            {
                // Multiple UI modules are allowed, we can add all their paths
                var ui = module as IUIModule;
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

            if (module is IIdentityProvider)
            {
                var identityProvider = module as IIdentityProvider;
                // For the IIdentityProvider interface we need to add routes to the API
                var userRoute = new APIRoute();
                userRoute.Path = "/login";
                userRoute.Description = "Log a user in";
                userRoute.Method = HttpMethod.Post;
                // TODO: Relook where this handler should be defined, possibly IIdentityProvider.LoginHandler?
                userRoute.Handler = (dynamic parameters, dynamic postData, dynamic user) =>
                {
                    // RequiredFields property is only implemented on the APIRouteAttribute
                    // so I have to manually do the checks for required interface routes
                    if (postData.username == null || postData.password == null)
                    {
                        throw new MissingFieldException("Username and password must not be null");
                    }

                    // Try..Catch as I'm calling user code here
                    var apiResponse = new APIResponse();
                    try
                    {
                        var loginResult = identityProvider.Login((string)postData.username, (string)postData.password);
                        
                        if (loginResult.IsAuthenticated == false)
                        {
                            apiResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                            apiResponse.StatusMessage = "Authentication required";
                            apiResponse.Data = loginResult.ErrorResponse;
                        }
                        else
                        {
                            apiResponse.Data = loginResult.User;
                        }
                    }
                    catch (Exception)
                    {
                        // Rethrow as we don't want to handle it
                        throw;
                    }
                    
                    return apiResponse;
                };
                CoreContainer.Instance.RouteManager.AddRoute(userRoute);
                CoreContainer.Instance.IdentityProvider = identityProvider;
            }

            // Add this provider to the input pipeline
            if (module is IInputProvider)
            {
                var inputProvider = module as IInputProvider;
                try
                {
                    CoreContainer.Instance.IOManager.RegisterProvider(inputProvider);
                }
                catch (NotSupportedException ex)
                {
                    _log.Error($"Unable to load input provider '{inputProvider.GetType().Name}': {ex.Message}");
                }
            }
            
            if (module is IInteractionEngine)
            {
                var engine = module as IInteractionEngine;
                // TODO: Better replacement. See Issue #3
                // https://github.com/ProjectLimitless/ProjectLimitless/issues/3
                CoreContainer.Instance.IOManager.SetEngine(engine);
            }


            // TODO: Add decorating to interfaces with required paths
            // TODO: Testing hook
            var moduleRoutes = module.GetAPIRoutes(_analysis.Record);
            if (CoreContainer.Instance.RouteManager.AddRoutes(moduleRoutes))
            {
                _log.Info($"Added {moduleRoutes.Count} new API routes for module '{moduleName}'");
            }
            else
            {
                _log.Warning($"Unable to add all API routes for module '{moduleName}'. Possible duplicate route and method.");
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
        private List<Uri> GetAvailableIPs(int port)
        {
            var availableIPs = new List<Uri>();
            string hostName = Dns.GetHostName();

            // Host name URI
            string hostNameUri = string.Format("http://{0}:{1}", Dns.GetHostName(), port);
            availableIPs.Add(new Uri(hostNameUri));

            // Host address URI(s)
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ipAddress in hostEntry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)  // IPv4 addresses only
                {
                    var addrBytes = ipAddress.GetAddressBytes();
                    string hostAddressUri = string.Format("http://{0}.{1}.{2}.{3}:{4}",
                    addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3], port);
                    availableIPs.Add(new Uri(hostAddressUri));
                }
            }

            // Localhost URI
            availableIPs.Add(new Uri(string.Format("http://localhost:{0}", port)));
            return availableIPs;
        }
    }
}
