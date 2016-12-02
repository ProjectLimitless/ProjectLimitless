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

using Limitless.Loaders;
using Limitless.Runtime.Types;

namespace Limitless
{
    /// <summary>
    /// The core API. Composed of the base and all the
    /// extended API modules.
    /// </summary>
    public class ComposedAPI : NancyModule
    {
        /// <summary>
        /// Standard Constructor.
        /// </summary>
        public ComposedAPI(RouteLoader loader)
        {
            foreach (APIRouteHandler routeHandler in loader.Routes)
            {
                Get[routeHandler.Route] = routeHandler.Handler;
            }

        }
    }
}
