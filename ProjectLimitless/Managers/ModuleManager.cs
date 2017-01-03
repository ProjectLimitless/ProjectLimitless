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

using Limitless.Runtime.Interfaces;

namespace Limitless.Managers
{
    /// <summary>
    /// Manages modules for use by the Core.
    /// </summary>
    public class ModuleManager
    {
        /// <summary>
        /// Gets the collection of loaded modules per interface type.
        /// </summary>
        public Dictionary<Type, List<IModule>> Modules { get; private set; }
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;
        /// <summary>
        /// The original parsed configurations for all the modules.
        /// </summary>
        private TomlTable _moduleConfigurations;
        /// <summary>
        /// Path the to modules directory.
        /// </summary>
        private string _modulesPath;
        /// <summary>
        /// A list containing interfaces that may only have
        /// one implementation module loaded.
        /// </summary>
        private List<Type> _singularModules;
            
        /// <summary>
        /// Handles loading, unloading and verifying.
        /// </summary>
        /// <param name="moduleConfigurations">The configuration of modules</param>
        /// <param name="log">The logger to use</param>
        public ModuleManager(TomlTable moduleConfigurations, ILogger log)
        {
            Modules = new Dictionary<Type, List<IModule>>();

            _log = log;
            _moduleConfigurations = moduleConfigurations;
            // Hard-coding the path name to make life easier for Unattended upgrades.
            _modulesPath = "modules";
            _singularModules = new List<Type>{
                typeof(ILogger)
            };
        }

        /// <summary>
        /// Load the specified module.
        /// </summary>
        /// <param name="moduleName">The name of the module to load</param>
        /// <exception cref="DllNotFoundException">Thrown when the module could not be found</exception>
        /// <exception cref="NotSupportedException">Thrown when no exported type implements IModule</exception>
        /// <returns>The loaded module</returns>
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
            IModule module = Construct(moduleType);
            Configure(moduleName, module);
            bool loaded = AddModule(module);
            if (loaded)
            {
                _log.Info($"Module '{moduleName}' has been loaded and configured");
                if (module is ILogger)
                {
                    _log = module as ILogger;
                }
            }
            else
            {
                _log.Error($"The module '{moduleName}' was not registered by the ModuleLoader. Please check logs.");
            }
            return module;
        }

        /// <summary>
        /// Load the specified builtin module.
        /// </summary>
        /// <param name="moduleName">The name of the module to load</param>
        /// <param name="builtinType">The builtin type of the module</param>
        /// <exception cref="DllNotFoundException">Thrown when the module could not be found</exception>
        /// <exception cref="NotSupportedException">Thrown when no exported type implements IModule</exception>
        /// <returns>The loaded module</returns>
        public IModule LoadBuiltin(string moduleName, Type builtinType)
        {
            if (typeof(IModule).IsAssignableFrom(builtinType) == false)
            {
                throw new NotSupportedException($"Module '{moduleName}' does not implement IModule.");
            }
            
            IModule module = Construct(builtinType);
            Configure(moduleName, module);
            bool loaded = AddModule(module);
            if (loaded)
            {
                _log.Info($"Module '{moduleName}' has been loaded and configured");
                if (module is ILogger)
                {
                    _log = module as ILogger;
                }
            }
            else
            {
                _log.Error($"The module '{moduleName}' was not registered by the ModuleLoader. Please check logs.");
            }
            return module;
        }

        /// <summary>
        /// Construct moduleType using the best matching constructor and 
        /// injecting as many modules as possible.
        /// </summary>
        /// <param name="moduleType">The Type to construct</param>
        /// <returns>The constructed module</returns>
        private IModule Construct(Type moduleType)
        {
            ConstructorInfo selectedConstructor = null;
            ConstructorInfo[] allConstructors = moduleType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (ConstructorInfo constructor in allConstructors)
            {                
                // Check if we have enough loaded modules to satisfy this constructor
                bool canSatisfy = true;
                ParameterInfo[] parameters = constructor.GetParameters();
                List<string> parametersList = new List<string>();
                foreach (ParameterInfo parameter in parameters)
                {
                    if (Modules.ContainsKey(parameter.ParameterType) == false)
                    {
                        _log.Trace($" Constructor parameter {parameter.Name}({parameter.ParameterType.Name}) can not be satisfied");
                        canSatisfy = false;
                        break;
                    }
                }
                // Set the best constructor
                if (canSatisfy)
                {
                    if (selectedConstructor == null)
                    {
                        selectedConstructor = constructor;
                    }
                    else if (parameters.Count() > selectedConstructor.GetParameters().Count())
                    {
                        selectedConstructor = constructor;
                    }
                }
            }

            // Execute the best matching constructor for the module
            List<object> constructorParameters = new List<object>();
            foreach (ParameterInfo parameter in selectedConstructor.GetParameters())
            {
                if (Modules.ContainsKey(parameter.ParameterType))
                {
                    if (Modules[parameter.ParameterType].Count >= 1)
                    {
                        // Only the first loaded module will be injected
                        constructorParameters.Add(Modules[parameter.ParameterType][0]);
                    }
                }
            }

            dynamic instance = Activator.CreateInstance(moduleType, constructorParameters.ToArray());
            IModule module = (IModule)instance;
            return module;
        }

        /// <summary>
        /// Configure the given module.
        /// </summary>
        /// <param name="moduleName">The name of module used to extract configuration</param>
        /// <param name="module">The IModule to configure</param>
        private void Configure(string moduleName, IModule module)
        {
            Type configurationType = module.GetConfigurationType();

            // Execute the configure method for IModule
            // I extract the Get<T> generic function from the TomlTable using reflection. 
            MethodInfo getMethod = typeof(TomlTable).GetMethods()
                .Where(x => x.Name == "Get")
                .First(x => x.IsGenericMethod);
            // Then I make it generic again
            MethodInfo generic = getMethod.MakeGenericMethod(configurationType);
            // Dots (.) have special meaning in TOML files, so remove them from the config lookup
            moduleName = moduleName.Replace(".", "");
            // Invoke it. The Get<T> method requires the configuration key as param
            dynamic dynamicConfig = generic.Invoke(_moduleConfigurations, new object[] { moduleName });

            // Configure the module with the settings
            module.Configure(dynamicConfig);
        }

        /// <summary>
        /// Limitless.Runtime can specify many interfaces, AddModule
        /// picks the best matching one to use to mark the module as
        /// for injection into other modules.
        /// </summary>
        /// <param name="module">The module to add</param>
        /// <returns>true if the module was loaded, false otherwise</returns>
        private bool AddModule(IModule module)
        {
            var interfaces = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(
                            t => t.IsInterface && 
                            t.Namespace == "Limitless.Runtime.Interfaces" &&
                            // Exclude the base module interface, we're not interested in it
                            t.Name != "IModule");
            if (interfaces == null)
            {
                return false;
            }
            bool added = false;
            foreach (Type type in interfaces.ToArray())
            {       
                // A module can implement multiple interfaces
                if (type.IsAssignableFrom(module.GetType()))
                {
                    if (Modules.ContainsKey(type) == false)
                    {
                        Modules.Add(type, new List<IModule>());
                    }
                    if (_singularModules.Contains(type))
                    {
                        if (Modules[type].Count == 1)
                        {
                            _log.Warning($"Module type '{type.ToString()}' already has a module loaded. '{module.ToString()}' will not be loaded");
                            continue;
                        }
                    }
                    Modules[type].Add(module);
                    added = true;
                }
            }
            return added;
        }
    }
}
