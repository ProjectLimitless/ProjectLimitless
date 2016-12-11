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
    /// Manages API routes. Defined as a Singleton to allow Nancy Bootstrapper
    /// access to already defined routes.
    /// </summary>
    public class RouteManager
    {
        /// <summary>
        /// The collection of routes to load.
        /// </summary>
        public List<APIRoute> Routes { get; }
        /// <summary>
        /// The collection of content routes to load.
        /// </summary>
        public List<string> ContentRoutes { get; }
        /// <summary>
        /// Holds the instance.
        /// </summary>
        private static RouteManager instance = null;
        /// <summary>
        /// Lock for singleton.
        /// </summary>
        private static readonly object padlock = new object();
        
        /// <summary>
        /// Gets the Singleton instance of the manager.
        /// </summary>
        public static RouteManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RouteManager();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Private constructor for Singleton pattern;
        /// </summary>
        RouteManager()
        {
            Routes = new List<APIRoute>();
            ContentRoutes = new List<string>();
        }
    }
}
