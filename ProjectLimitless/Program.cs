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

using NLog;
using Limitless.Config;

namespace Limitless
{
    public class SetDemo
    {
        public string Two { get; set; }
        public MoreSet More { get; set; }
        public class MoreSet
        {
            public string Three { get; set; }
        }
    }

    class Program
    {
       
        static void Main(string[] args)
        {
            Logger log = LogManager.GetCurrentClassLogger();
            log.Info("Loading Project Limitless...");

            log.Debug("Loading configuration...");
            ConfigLoader configLoader = new ConfigLoader();
            LimitlessSettings settings = configLoader.Load();

            //Limitless limitless = new Limitless(settings, log);
            
            /*
            
            Nett TOML

            ///TODO: Test file merging
            var appSource =  ConfigSource.CreateFileSource("conf/Core.toml");
            var userSource = ConfigSource.CreateFileSource("conf/User.toml");
            var merged = ConfigSource.Merged(appSource, userSource); // order important here

            // merge both TOML files into one settings object
            Config<LimitlessSettings> settings = Nett.Coma.Config.Create(() => new LimitlessSettings(), merged);
            

            
            Console.WriteLine("DemoName String: {0}", settings.Get(s => s.DemoName));
            */
            /*Console.WriteLine("test: {0}", settings.Get(s => settings));
            Console.WriteLine("Owner.Name: {0}", settings.Get(s => s.Owner.Name));
            Console.WriteLine("Updates.Primary.Host: {0}", settings.Get(s => s.Updates.Primary.Host));
            Console.WriteLine("Updates.Secondary.Host: {0}", settings.Get(s => s.Updates.Secondary.Host));
            */


            log.Info("Project Limitless has shut down.");
        }
    }
}
