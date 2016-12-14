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
        /// Gets the collection of loaded modules.
        /// </summary>
        public Dictionary<Type, IModule> Modules { get; private set; }

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
        /// Handles loading, unloading and verifying.
        /// </summary>
        /// <param name="moduleConfigurations">The configuration of modules</param>
        /// <param name="log">The logger to use</param>
        public ModuleManager(TomlTable moduleConfigurations, ILogger log)
        {
            Modules = new Dictionary<Type, IModule>();

            _log = log;
            _moduleConfigurations = moduleConfigurations;
            // Hard-coding the path name to make life easier for Unattended upgrades.
            _modulesPath = "modules";
        }

        /// <summary>
        /// Load the specified module.
        /// </summary>
        /// <param name="moduleName">The name of the module to load</param>
        /// <exception cref="DllNotFoundException">Thrown when the module could not be found</exception>
        /// <exception cref="NotSupportedException">Thrown when no exported type implements IModule</exception>
        public void Load(string moduleName)
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
            Type addedType = AddModule(module);
            if (addedType != null)
            {
                if (addedType == typeof(ILogger))
                {
                    _log = (ILogger)module;
                }
            }
            
            _log.Info($"Module '{moduleName}' has been loaded and configured");
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
                constructorParameters.Add(Modules[parameter.ParameterType]);
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
        /// <returns>The interface type added, null if the interface could not be determined</returns>
        private Type AddModule(IModule module)
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
                return null;
            }
            Type addedType = null;
            foreach (Type type in interfaces.ToArray())
            {       
                // A module can implement multiple interfaces
                if (type.IsAssignableFrom(module.GetType()))
                {
                    Modules.Add(type, module);
                    addedType = type;
                }
            }
            return addedType;
        }
    }
}
