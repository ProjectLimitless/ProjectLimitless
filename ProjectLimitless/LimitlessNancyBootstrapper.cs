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

using Nancy;
using Nancy.TinyIoc;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Bootstrapper;

using Limitless.Loaders;
using Limitless.Managers;
using Limitless.Containers;
using Limitless.Runtime.Interfaces;

namespace Limitless
{
    /// <summary>
    /// Provides a custom Nancy bootstrapper for Project Limitless.
    /// </summary>
    public class LimitlessNancyBootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// Overrides the default application startup to add custom error
        /// handlers for Nancy.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pipelines"></param>
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            CustomErrorsLoader.AddCode(404, "The resource could not be found");
            CustomErrorsLoader.AddCode(500, "An application error occurred");

            if (CoreContainer.Instance.Settings.Core.API.Nancy.DashboardEnabled == false)
            {
                DiagnosticsHook.Disable(pipelines);
                // Workaround according to https://github.com/NancyFx/Nancy/issues/1518
                container.Register<IDiagnostics, DisabledDiagnostics>();
            }

            StaticConfiguration.EnableRequestTracing = CoreContainer.Instance.Settings.Core.API.Nancy.EnableRequestTracing;
        }

        /// <summary>
        /// Overrides the default container init and provides a ComposedAPI instance.
        /// </summary>
        /// <param name="container">The TinyIoC container</param>
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<RouteManager>(CoreContainer.Instance.RouteManager);
            container.Register<IIdentityProvider>(CoreContainer.Instance.IdentityProvider);
        }
        
        /// <summary>
        /// Overrides the default convention setup for static routes.
        /// </summary>
        /// <param name="conventions">The default Nancy conventions</param>
        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);
            foreach (string contentRoute in CoreContainer.Instance.RouteManager.GetContentRoutes())
            {
                conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory(contentRoute, contentRoute));
            }
        }

        /// <summary>
        /// Overrides the diagnostics configuration to provide a password for access.
        /// </summary>
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new DiagnosticsConfiguration
                {
                    Password = CoreContainer.Instance.Settings.Core.API.Nancy.DashboardPassword,
                    Path = CoreContainer.Instance.Settings.Core.API.Nancy.DashboardPath
                };
            }
        }
    }
}
