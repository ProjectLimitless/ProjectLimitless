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

using Limitless.Config;
using Limitless.Managers;
using Limitless.Runtime.Interfaces;

namespace Limitless.Containers
{
    /// <summary>
    /// Holds and gives access to core parts via a Singleton
    /// to allow usage of already constructed objects in injected modules.
    /// </summary>
    internal class CoreContainer
    {
        /// <summary>
        /// Manages API routes.
        /// </summary>
        public RouteManager RouteManager { get; internal set; }
        /// <summary>
        /// Manages loaded modules.
        /// </summary>
        public ModuleManager ModuleManager { get; internal set; }
        /// <summary>
        /// Manages inputs and outputs.
        /// </summary>
        public IOManager IOManager { get; internal set; }
        /// <summary>
        /// Provides identity management.
        /// </summary>
        public IIdentityProvider IdentityProvider { get; internal set; }
        /// <summary>
        /// The Limitless configuration.
        /// </summary>
        public LimitlessSettings Settings { get; internal set; }

        /// <summary>
        /// Holds the instance.
        /// </summary>
        private static CoreContainer instance = null;
        /// <summary>
        /// Lock for singleton.
        /// </summary>
        private static readonly object padlock = new object();
        
        /// <summary>
        /// Gets the Singleton instance of the manager.
        /// </summary>
        public static CoreContainer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CoreContainer();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Private constructor for Singleton pattern;
        /// </summary>
        private CoreContainer()
        {
            RouteManager = new RouteManager();
            Settings = new LimitlessSettings();
        }
    }
}
