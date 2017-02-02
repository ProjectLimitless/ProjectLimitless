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

namespace Limitless.Config
{
    /// <summary>
    /// Core settings.
    /// </summary>
    public class CoreSettings
    {
        /// <summary>
        /// The default name of the system. Also used as the trigger word.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of modules to load.
        /// </summary>
        public string[] EnabledModules { get; set; }
        /// <summary>
        /// Maximum input extraction attempts.
        /// </summary>
        public uint MaxResolveAttempts { get; set; }
        /// <summary>
        /// API hosting settings.
        /// </summary>
        public CoreAPISettings API { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="CoreSettings"/>.
        /// </summary>
        public CoreSettings()
        {
            API = new CoreAPISettings();
            EnabledModules = new string[] { };
            MaxResolveAttempts = 5;
        }
    }
}