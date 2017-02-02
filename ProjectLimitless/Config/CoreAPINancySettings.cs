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
    /// Core API settings for Nancy.
    /// </summary>
    public class CoreAPINancySettings
    {
        /// <summary>
        /// Sets and gets if the diagnostics dashboard should be enabled.
        /// </summary>
        public bool DashboardEnabled { get; set; }
        /// <summary>
        /// The password for the dashboard if enabled is set to true.
        /// </summary>
        public string DashboardPassword { get; set; }
        /// <summary>
        /// The URL path to serve the dashboard on.
        /// </summary>
        public string DashboardPath { get; set; }
        /// <summary>
        /// Sets and gets if request tracing should be enabled.
        /// </summary>
        public bool EnableRequestTracing { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="CoreAPINancySettings"/>
        /// with defaults.
        /// </summary>
        public CoreAPINancySettings()
        {
            DashboardEnabled = false;
            DashboardPassword = "";
            DashboardPath = "_nancy";
            EnableRequestTracing = false;
        }
    }
}