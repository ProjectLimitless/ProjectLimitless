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

namespace Limitless.Builtin.DatabaseProviders
{
    /// <summary>
    /// Configuration for the MySQL database provider.
    /// </summary>
    public class DatabaseProviderConfig
    {
        /// <summary>
        /// Get and sets the connection string for the provider.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}