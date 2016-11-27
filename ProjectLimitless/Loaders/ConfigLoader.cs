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

using Nett;
using Nett.Coma;

using Limitless.Config;

namespace Limitless.Loaders
{
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

            settings.Core = LimitlessSettings.Convert<CoreSettings>("Core", completeToml);
            settings.FullConfiguration = completeToml;

            return settings;
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
