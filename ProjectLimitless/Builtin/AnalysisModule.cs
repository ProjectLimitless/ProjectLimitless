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
using System.Dynamic;
using System.Reflection;
using System.Collections.Generic;

using Limitless.Containers;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Attributes;

namespace Limitless.Builtin
{
    internal class AnalysisModule : IModule
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Standard constructor with log.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public AnalysisModule(ILogger log)
        {
            _log = log;
        }

        // TODO: Fix this!
        public void SetLog(ILogger log)
        {
            _log = log;
        }
        
        //TODO: Load as proper builtin modulel
        public void Configure(dynamic settings)
        {
            throw new NotImplementedException();
        }

        public Type GetConfigurationType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetTitle"/>
        /// </summary>
        public string GetTitle()
        {
            return "Project Limitless Analysis Module";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            return "Donovan Solms";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetVersion"/>
        /// </summary>
        public string GetVersion()
        {
            return "0.0.0.1";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetDescription"/>
        /// </summary>
        public string GetDescription()
        {
            return "Provides recording and analysis of Project Limitless actions";
        }

        /// <summary>
        /// Record the API call for later analysis.
        /// </summary>
        /// <param name="output">The output to record</param>
        /// <param name="parameters">The input parameters of the API call</param>
        /// <returns>The forwarded output</returns>
        public dynamic Record(dynamic output, object[] parameters)
        {
            _log.Debug($"Parameter count {parameters.Length}");
            _log.Debug($"Output: {Convert.ToString(output)}");
            return output;
        }
    }
}
