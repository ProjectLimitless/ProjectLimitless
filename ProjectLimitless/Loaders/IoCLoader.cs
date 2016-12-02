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

namespace Limitless.Loaders
{
    /// <summary>
    /// Provides a custom bootstrapper for Nancy.
    /// </summary>
    public class IoCLoader : DefaultNancyBootstrapper
    {
        /// <summary>
        /// Overrides the default container init and provides a ComposedAPI instance.
        /// </summary>
        /// <param name="container">The TinyIoC container</param>
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<RouteLoader>(RouteLoader.Instance);
        }
    }
}
