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
using System.IO;
using System.Linq;

using Nett;
using Nett.Coma;
using System.Reflection;

namespace Limitless.Config
{
    public class TestModule
    {
        public string Name { get; set; }
        public APISet API { get; set; }
        public class APISet
        {
            public string Host { get; set; }
        }

    }

    public class TM
    {
        public static Type GetConfigType()
        {
            return typeof(TestModule);
        }
    }

    /// <summary>
    /// ConfigLoader implements the loading logic for the Core, User
    /// and module configurations.
    /// </summary>
    internal class ConfigLoader
    {
        /// <summary>
        /// Gets the path containing the Core and User configurations.
        /// </summary>
        public string ConfigurationPath { get; } = Path.GetFullPath("conf");
        /// <summary>
        /// Gets the path containing the module configurations.
        /// </summary>
        public string ModuleConfigurationPath { get; } = Path.GetFullPath(Path.Combine("conf", "modules"));

        /// <summary>
        /// Loads the configurations from ConfigurationPath and ModuleConfigurationPath.
        /// </summary>
        /// <returns>The merged configuration</returns>
        public LimitlessSettings Load()
        {
            LimitlessSettings settings = new LimitlessSettings();
            
            string coreConfigString = "";
            string coreFilePath = Path.Combine(ConfigurationPath, "Core.toml");
            if (File.Exists(coreFilePath) == false)
            {
                throw new FileNotFoundException($"The Core.toml configuration file could not be found at '{coreFilePath}'.");
            }
            coreConfigString = File.ReadAllText(coreFilePath);

            string userConfigString = "";
            string userFilePath = Path.Combine(ConfigurationPath, "User.toml");
            if (File.Exists(userFilePath) == false)
            {
                throw new FileNotFoundException($"The User.toml configuration file could not be found at '{userFilePath}'.");
            }
            userConfigString= File.ReadAllText(userFilePath);

            string moduleConfigsString = "";
            string[] moduleConfigFiles = Directory.GetFiles(ModuleConfigurationPath, "*.toml");
            if (moduleConfigFiles.Length == 0)
            {
                throw new NotSupportedException($"No module configurations could be found at '{ModuleConfigurationPath}'.");
            }
            foreach (string configPath in moduleConfigFiles)
            {
                moduleConfigsString += File.ReadAllText(configPath);
            }

            string combinedConfigsString = coreConfigString + moduleConfigsString;

            TomlTable combinedToml = Toml.ReadString(combinedConfigsString);
            TomlTable userToml = Toml.ReadString(userConfigString);
            
            TomlTable completeToml = Merge(combinedToml, userToml);

            foreach (string key in completeToml.Keys)
            {

            }

            TestModule m = ConfigLoader.Convert<TestModule>("TestModule", completeToml);
            Console.WriteLine(m.Name);


            return settings;
        }

        /// <summary>
        /// Converts the section with given key to type T extracted from data.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="key">The key in the config</param>
        /// <param name="data">The configuration data</param>
        /// <returns>The section with key as type T</returns>
        public static T Convert<T>(string key, TomlTable data)
        {
            // I extract the Get<T> generic function from the TomlTable using reflection. 
            MethodInfo getMethod = typeof(TomlTable).GetMethods()
                .Where(x => x.Name == "Get")
                .First(x => x.IsGenericMethod);
            // Then I make it generic again
            MethodInfo generic = getMethod.MakeGenericMethod(typeof(T));
            // Invoke it. The Get<T> method requires the configuration key as param
            dynamic dynamicConfig = generic.Invoke(data, new object[] { key });
            return (T)dynamicConfig;
        }

        /// <summary>
        /// Merge two TomlTables together. Values from overrides will replace values in original where available.
        /// </summary>
        /// <param name="inputA">The </param>
        /// <param name="inputB"></param>
        /// <returns></returns>
        private TomlTable Merge(TomlTable original, TomlTable overrides)
        {
            original.OverwriteWithValuesForLoadFrom(overrides);
            return original;
        }
    }
}
