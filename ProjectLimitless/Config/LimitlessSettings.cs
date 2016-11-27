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

using System.Linq;
using System.Reflection;

using Nett;

namespace Limitless.Config
{
    /// <summary>
    /// Settings passed into a new Limitless instance.
    /// </summary>
    public class LimitlessSettings
    {
        /// <summary>
        /// Collection of the core configuration settings.
        /// </summary>
        public CoreSettings Core { get; set; }
        /// <summary>
        /// Gets the full configuration as parsed.
        /// </summary>
        public TomlTable FullConfiguration { get; set; }

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
    }
}