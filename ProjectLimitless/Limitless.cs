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
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Interactions;
using Limitless.Runtime.Enums;

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
        private readonly LimitlessSettings _settings;

        /// <summary>
        /// Provides administration functions.
        /// TODO: Mode to separate module?
        /// </summary>
        private AdminModule _adminModule;

        // TODO: Load as proper module?
        private AnalysisModule _analysis;

        /// <summary>
        /// Creates a new instance of <see cref="Limitless"/> with
        /// the loaded <see cref="LimitlessSettings"/> and <see cref="ILogger"/>.
        /// </summary>
        /// <param name="settings">The <see cref="LimitlessSettings"/> to be used</param>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        public Limitless(LimitlessSettings settings, ILogger log)
        {
            
            Skill skill = new Skill();
            skill.Name = "Coffee Brewer";
            skill.Author = "Sample Skill Maker";
            skill.ShortDescription = "A skill to make coffee using a standard kettle";
            skill.Intent = new Intent();
            skill.Intent.Actions.Add("brew");
            skill.Intent.Actions.Add("make");
            skill.Intent.Targets.Add("coffee");
            skill.Intent.Targets.Add("cuppa");
            skill.Binding = SkillExecutorBinding.Network;
            var executor = new NetworkExecutor();
            executor.Url = "https://www.google.com";
            executor.ValidateCertificate = false;
            skill.Executor = executor;
            skill.Locations.Add("kitchen");
            skill.Locations.Add("downstairs");
            skill.Help.Phrase = "make coffee";
            skill.Help.ExamplePhrase = "Make me a cup of coffee";
			skill.Parameters.Add(new SkillParameter("sugar", SkillParameterType.Integer, true));

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(skill));

            Console.ReadLine();


            _settings = settings;
            _log = log;
            // TODO: the analysis module hook needs to be implemented in a better way
            _analysis = new AnalysisModule(_log);

            CoreContainer.Instance.ModuleManager = new ModuleManager(_settings.FullConfiguration, _log);
            
            // TODO: Maybe make IOManager just an instance variable?
            CoreContainer.Instance.IOManager = new IOManager(_log);
            CoreContainer.Instance.RouteManager.AddRoutes(CoreContainer.Instance.IOManager.GetRequiredRoutes());

#if DEBUG
            _log.Debug($"Added {CoreContainer.Instance.IOManager.GetRequiredRoutes().Count} required API routes for 'IOManager'");
#endif
            
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
                    var builtinType = Type.GetType(moduleName, true, false);
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
            var routes = _adminModule.GetAPIRoutes();
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

            // When debug output is enabled, print the loaded routes
            foreach (var route in CoreContainer.Instance.RouteManager.GetRoutes())
            {
                _log.Debug($"Added route '{route.Method.ToString().ToUpper()} {route.Path}'");
            }

            //TODO: Setup the diagnostics

            CoreContainer.Instance.Settings = _settings;
        }

        /// <summary>
        /// Run executes the main loop.
        /// </summary>
        public void Run()
        {
            var config = new HostConfiguration
            {
                UrlReservations =
                {
                    CreateAutomatically = true
                }
            };

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

                CoreContainer.Instance.RouteManager.AddRoutes(identityProvider.GetRequiredAPIRoutes(_analysis.Record));
                CoreContainer.Instance.IdentityProvider = identityProvider;
#if DEBUG
                _log.Debug($"Added {identityProvider.GetRequiredAPIRoutes(_analysis.Record).Count} required API routes for 'IdentityProvider'");
#endif

            }

            // Add this provider to the input pipeline
            if (module is IIOProcessor)
            {
                var inputProvider = module as IIOProcessor;
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
                var interactionEngine = module as IInteractionEngine;
                // TODO: Better replacement. See Issue #3
                // https://github.com/ProjectLimitless/ProjectLimitless/issues/3
                CoreContainer.Instance.IOManager.SetEngine(interactionEngine);
                CoreContainer.Instance.RouteManager.AddRoutes(interactionEngine.GetRequiredAPIRoutes(_analysis.Record));

#if DEBUG
                _log.Debug($"Added {interactionEngine.GetRequiredAPIRoutes(_analysis.Record).Count} required API routes for 'InteractionEngine'");
#endif
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
            string hostNameUri = $"http://{Dns.GetHostName()}:{port}";
            availableIPs.Add(new Uri(hostNameUri));

            // Host address URI(s)
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ipAddress in hostEntry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)  // IPv4 addresses only
                {
                    var addrBytes = ipAddress.GetAddressBytes();
                    string hostAddressUri = $"http://{addrBytes[0]}.{addrBytes[1]}.{addrBytes[2]}.{addrBytes[3]}:{port}";
                    availableIPs.Add(new Uri(hostAddressUri));
                }
            }

            // Localhost URI
            availableIPs.Add(new Uri($"http://localhost:{port}"));
            return availableIPs;
        }
    }
}
