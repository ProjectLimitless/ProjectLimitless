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

using System.Collections.Generic;

using Limitless.Runtime.Types;

namespace Limitless.Managers
{
    /// <summary>
    /// Manages API routes.
    /// </summary>
    public class RouteManager
    {
        /// <summary>
        /// The collection of routes to load.
        /// 
        /// Private to add checks to adding routes.
        /// </summary>
        private List<APIRoute> _routes;
        /// <summary>
        /// The collection of content routes to load.
        ///
        /// Private to add checks to adding content routes.
        /// </summary>
        private List<string> _contentRoutes;
       
        /// <summary>
        /// Standard constructor
        /// </summary>
        public RouteManager()
        {
            _routes = new List<APIRoute>();
            _contentRoutes = new List<string>();
        }

        /// <summary>
        /// Gets the list of registered API routes.
        /// </summary>
        /// <returns>The list of routes</returns>
        public List<APIRoute> GetRoutes()
        {
            return _routes;
        }

        /// <summary>
        /// Adds a route to the API. Prepends '/api' to the path.
        /// </summary>
        /// <param name="route">The route to add</param>
        /// <returns>true if added, false otherwise</returns>
        public bool AddRoute(APIRoute route)
        {
            // Prepending '/api' to the path to ensure API routes
            // don't clash with static routes
            route.Path = $"/api{route.Path}";
            foreach (APIRoute loadedRoute in _routes)
            {
                if ((loadedRoute.Path == route.Path) && (loadedRoute.Method == route.Method))
                {
                    return false;
                }
            }
            _routes.Add(route);
            return true;
        }

        /// <summary>
        /// Adds multiple routes to the API. Stops at the 
        /// first error when loading a route. Prepends '/api' to each path.
        /// </summary>
        /// <param name="routes">The list of routes to add</param>
        /// <returns>true if everything was added, false otherwise</returns>
        public bool AddRoutes(List<APIRoute> routes)
        {
            foreach (APIRoute route in routes)
            {
                if (AddRoute(route) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the list of registred content routes.
        /// </summary>
        /// <returns>The list of routes</returns>
        public List<string> GetContentRoutes()
        {
            return _contentRoutes;
        }

        /// <summary>
        /// Adds a content path to the API.
        /// </summary>
        /// <param name="path">The path to add</param>
        /// <returns>true if added, false otherwise</returns>
        public bool AddContentRoute(string path)
        {
            if (_contentRoutes.Contains(path))
            {
                return false;
            }
            _contentRoutes.Add(path);
            return true;
        }
    }
}
