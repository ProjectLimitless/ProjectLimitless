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

using Limitless.Builtin;
using Limitless.Loaders;

namespace Limitless
{
    class Program
    {  
        static void Main(string[] args)
        {
            var log = new BootstrapLogger();
            log.Warning("Bootstrap logger in use. This logger does not support hiding logging levels - everything is displayed");
            log.Info("Loading Project Limitless...");

            log.Debug("Loading configuration...");
            var configLoader = new ConfigLoader();
            var settings = configLoader.Load();

            var limitless = new Limitless(settings, log);
            limitless.Run();
            
            log.Info("Project Limitless has shut down.");
        }
    }
}
