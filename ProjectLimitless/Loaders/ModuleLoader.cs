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
using System.Reflection;
using System.Collections.Generic;

using Nett;
using NLog;

using Limitless.Runtime;


namespace Limitless.Loaders
{
    /// <summary>
    /// Loads modules for use by the Core.
    /// </summary>
    public class ModuleLoader
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private Logger _log;
        /// <summary>
        /// The original parsed configurations for all the modules.
        /// </summary>
        private TomlTable _moduleConfigurations;

        private string _modulesPath;
            
        /// <summary>
        /// Handles loading, verifying and constructing modules.
        /// </summary>
        /// <param name="moduleConfigurations">The configuration of modules</param>
        /// <param name="log">The logger to use</param>
        public ModuleLoader(TomlTable moduleConfigurations, Logger log)
        {
            _log = log;
            _moduleConfigurations = moduleConfigurations;
            // Hard-coding the path name to make life easier for Unattended upgrades.
            _modulesPath = "modules";
        }

        /// <summary>
        /// Load the specified module.
        /// </summary>
        /// <param name="moduleName">The name of the module to load</param>
        /// <returns>The loaded and constructed module</returns>
        /// <exception cref="DllNotFoundException">Thrown when the module could not be found</exception>
        /// <exception cref="NotSupportedException">Thrown when no exported type implements IModule</exception>
        public IModule Load(string moduleName)
        {
            string moduleFilename = moduleName + ".dll";
            _log.Trace($"Checking if module {moduleFilename} is valid and loadable");

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _modulesPath, moduleFilename);
            _log.Trace(" Loading {0}", dllPath);
            if (File.Exists(dllPath) == false)
            {
                throw new DllNotFoundException($"Module '{moduleName}' not found at '{dllPath}'");
            }

            // Only check modules that implement the IModule interface
            var dll = Assembly.LoadFrom(dllPath);
            List<Type> availableModules = dll.GetExportedTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t))
                .ToList();

            if (availableModules.Count > 1)
            {
                throw new NotSupportedException($"Module '{moduleName}' implements more than one IModule. Only one is allowed.");
            }
            // Only one module is allowed in a DLL
            if (availableModules.Count != 1)
            {
                throw new NotSupportedException($"Module '{moduleName}' does not implement IModule.");
            }

            Type moduleType = availableModules.First();
            ConstructorInfo[] constructors = moduleType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                List<string> parametersList = new List<string>();
                foreach (ParameterInfo parameter in parameters)
                {
                    parametersList.Add($"{parameter.Name}({parameter.ParameterType.Name})");
                }
                _log.Trace($" Module '{moduleName}' constructor found with parameters: {string.Join(",", parametersList)}");
            }
            
            // TODO: Attempt to inject as many 

            dynamic instance = Activator.CreateInstance(moduleType);
            IModule module = (IModule)instance;
            Type configurationType = module.GetConfigurationType();

            // I extract the Get<T> generic function from the TomlTable using reflection. 
            MethodInfo getMethod = typeof(TomlTable).GetMethods()
                .Where(x => x.Name == "Get")
                .First(x => x.IsGenericMethod);
            // Then I make it generic again
            MethodInfo generic = getMethod.MakeGenericMethod(configurationType);
            // Invoke it. The Get<T> method requires the configuration key as param
            dynamic dynamicConfig = generic.Invoke(_moduleConfigurations, new object[] { moduleName});

            // Configure the module with the settings
            module.Configure(dynamicConfig);
            _log.Info($"Module '{moduleName}' has been loaded and configured");
            
            return module;
        }
    }
}
