
using Nett.Coma;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLimitless
{
    class Program
    {
        static void Main(string[] args)
        {
            ///TODO: Test file merging
            var appSource =  ConfigSource.CreateFileSource("conf/Core.toml");
            var userSource = ConfigSource.CreateFileSource("conf/User.toml");
            var merged = ConfigSource.Merged(appSource, userSource); // order important here

            // merge both TOML files into one settings object
            Config<Limitless.Config.CoreSettings> settings = Config.Create(() => new Limitless.Config.CoreSettings(), merged);
            Console.WriteLine("test: {0}", settings.Get(s => s.Test));
            Console.WriteLine("Owner.Name: {0}", settings.Get(s => s.Owner.Name));
            Console.WriteLine("Updates.Primary.Host: {0}", settings.Get(s => s.Updates.Primary.Host));
            Console.WriteLine("Updates.Secondary.Host: {0}", settings.Get(s => s.Updates.Secondary.Host));

            Console.ReadLine();
        }
    }
}
