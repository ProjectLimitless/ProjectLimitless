/** 
* This file is part of Project Limitless.
* Copyright © 2017 Donovan Solms.
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
    /// Core API settings for CORS headers.
    /// </summary>
    public class CoreAPICORSSettings
    {
        /// <summary>
        /// The allowed origin(s) for API calls.
        /// Defaults to anywhere (*).
        /// </summary>
        public string AllowedOrigin { get; set; }

        /// <summary>
        /// Creates a new <see cref="CoreAPICORSSettings"/>
        /// with the <see cref="AllowedOrigin"/> set to '*'.
        /// </summary>
        public CoreAPICORSSettings()
        {
            AllowedOrigin = "*";
        }
    }
}