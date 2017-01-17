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

using Limitless.Runtime.Enums;
using Limitless.Runtime.Interfaces;

namespace Limitless.Builtin
{
    /// <summary>
    /// A simple Console logger to be used while bootstrapping Limitless.
    /// </summary>
    public class BootstrapLogger : ILogger
    {
        /// <summary>
        /// The required log level.
        /// </summary>
        private LogLevel _logLevel;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public BootstrapLogger()
        {
            UpdateLevel(LogLevel.Info);
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.UpdateLevel(string)"/>
        /// </summary>
        public void UpdateLevel(LogLevel level)
        {
            _logLevel = level;
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Trace(string, object[])"/>
        /// </summary>
        public void Trace(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace)
            {
                Console.WriteLine($"[Bootstrap] Trace | {format}", args);
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Debug(string, object[])"/>
        /// </summary>
        public void Debug(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace ||
                _logLevel == LogLevel.Debug)
            {
                Console.WriteLine($"[Bootstrap] Debug | {format}", args);
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Info(string, object[])"/>
        /// </summary>
        public void Info(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace ||
                _logLevel == LogLevel.Debug ||
                _logLevel == LogLevel.Info)
            {
                Console.WriteLine($"[Bootstrap] Info | {format}", args);
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Warning(string, object[])"/>
        /// </summary>
        public void Warning(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace ||
                _logLevel == LogLevel.Debug ||
                _logLevel == LogLevel.Info ||
                _logLevel == LogLevel.Warning)
            {
                Console.WriteLine($"[Bootstrap] Warning | {format}", args);
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Error(string, object[])"/>
        /// </summary>
        public void Error(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace ||
                _logLevel == LogLevel.Debug ||
                _logLevel == LogLevel.Info ||
                _logLevel == LogLevel.Warning ||
                _logLevel == LogLevel.Error)
            {
                Console.WriteLine($"[Bootstrap] Error | {format}", args);
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.ILogger.Critical(string, object[])"/>
        /// </summary>
        public void Critical(string format, params object[] args)
        {
            if (_logLevel == LogLevel.Trace ||
                _logLevel == LogLevel.Debug ||
                _logLevel == LogLevel.Info ||
                _logLevel == LogLevel.Warning ||
                _logLevel == LogLevel.Error ||
                _logLevel == LogLevel.Critical)
            {
                Console.WriteLine($"[Bootstrap] Critical | {format}", args);
            }
        }
    }
}
