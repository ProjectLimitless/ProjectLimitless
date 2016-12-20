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
    /// Core API settings.
    /// </summary>
    public class CoreAPISettings
    {
        /// <summary>
        /// The host IP to bind to.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The port to bind on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CoreAPISettings()
        {
            Host = "0.0.0.0";
            Port = 8080;
        }
    }
}