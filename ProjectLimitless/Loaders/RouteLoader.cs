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

using Limitless.Runtime.Types;

namespace Limitless.Loaders
{
    /// <summary>
    /// Provides a custom bootstrapper for Nancy.
    /// </summary>
    public class RouteLoader
    {
        /// <summary>
        /// The collection of routes to load.
        /// </summary>
        public List<APIRoute> Routes { get; }
        /// <summary>
        /// Holds the instance.
        /// </summary>
        private static RouteLoader instance = null;
        /// <summary>
        /// Lock for thread safety.
        /// </summary>
        private static readonly object padlock = new object();
        
        /// <summary>
        /// Gets the Singleton instance of the loader.
        /// </summary>
        public static RouteLoader Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RouteLoader();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Private constructor for Singleton pattern;
        /// </summary>
        RouteLoader()
        {
            Routes = new List<APIRoute>();
        }
    }
}
