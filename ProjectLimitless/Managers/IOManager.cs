/** 
* This file is part of Project Limitless.
* Copyright � 2016 Donovan Solms.
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

using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Types;
using System.Dynamic;
using Limitless.Runtime.Enums;
using Limitless.Containers;

namespace Limitless.Managers
{
    /// <summary>
    /// Manages inputs and outputs from and to the Project Limitless API.
    /// </summary>
    public class IOManager
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;
        /// <summary>
        /// The loaded interaction engine.
        /// </summary>
        private IInteractionEngine _engine;
        /// <summary>
        /// The collection of available input providers arranged by Content-Type.
        /// </summary>
        private Dictionary<string, IInputProvider> _inputProviders;

        /// <summary>
        /// Standard constructor with logger.
        /// </summary>
        /// <param name="log">The </param>
        public IOManager(ILogger log)
        {
            _log = log;
            _inputProviders = new Dictionary<string, IInputProvider>();
        }

        /// <summary>
        /// TODO: Find a better way to reload modules in other modules
        /// See Issue #3 
        /// https://github.com/ProjectLimitless/ProjectLimitless/issues/3
        /// </summary>
        /// <param name="engine"></param>
        public void SetEngine(IInteractionEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Handle the input API call.
        /// </summary>
        /// <param name="parameters">The parameters for the API call</param>
        /// <param name="postData">The POSTed data</param>
        /// <param name="user">The authenticated user or client</param>
        /// <returns>The input response</returns>
        public APIResponse Handle(dynamic parameters, dynamic postData, dynamic user)
        {
            _log.Trace($"Handle input from client. Content-Type is '{parameters.contentType}'");
            var response = new APIResponse();

            // TODO: Define custom JSON? application/vnd.limitless.input+json?

            // Check the Content-Type
            // If it is application/json
            // if will be parsed into IOInput that may contain multiple
            // inputs.
            if (parameters.contentType == MimeType.Json)
            {
                _log.Debug("Received JSON, parse into usable type");
            }

            // TODO: Resolve multi-request parallel - ie. speech recognition + voice recognition id
            IOData processedData = ResolveInput(new IOData(parameters.contentType, postData));
            processedData = _engine.ProcessInput(processedData);
            
            response.Data = processedData.Data;
            var header = new
            {
                Header = "Content-Type",
                Value = processedData.Mime
            };
            response.Headers.Add(header);

            return response;
        }

        /// <summary>
        /// Registers an input provider for the MIME types as 
        /// returned by the provider.
        /// </summary>
        /// <param name="provider">The input provider to register</param>
        /// <exception cref="NotSupportedException">Thrown if a MIME type is already registered. No MIME types are loaded after the exception.</exception>
        public void RegisterProvider(IInputProvider provider)
        {
            List<string> mimes = provider.GetInputMimeTypes().ToList<string>();
            foreach (string mime in mimes)
            {
                if (_inputProviders.ContainsKey(mime))
                {
                    throw new NotSupportedException($"MIME Type '{mime}' already has an input provider registered '{_inputProviders[mime].GetType().Name}'");
                }
                else
                {
                    _inputProviders.Add(mime, provider);
                }
            }
        }

        /// <summary>
        /// Deregisters an input provider for the MIME types as 
        /// returned by the provider.
        /// </summary>
        /// <param name="provider">The input provider to deregister</param>
        public void DeregisterProvider(IInputProvider provider)
        {
            List<string> mimes = provider.GetInputMimeTypes().ToList<string>();
            foreach (string mime in mimes)
            {
                if (_inputProviders.ContainsKey(mime))
                {
                    _inputProviders.Remove(mime);
                }
            }
        }

        /// <summary>
        /// TODO: Fix this - better module replacement, see Limitless.cs SetLog calls
        /// </summary>
        /// <param name="log"></param>
        public void SetLog(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Recursively processes the input until a usable data type is extracted
        /// or LimitlessSettings
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resolveAttempts"></param>
        /// <returns>The recognised intent</returns>
        private IOData ResolveInput(IOData input, int resolveAttempts = 0)
        {
            if (resolveAttempts >= CoreContainer.Instance.Settings.Core.MaxResolveAttempts)
            {
                throw new NotSupportedException("Maximum attempts reached for extracting intent from input");
            }

            if (_inputProviders.ContainsKey(input.Mime) == false)
            {
                _log.Trace($"MIME type '{input.Mime}' has no supported input providers loaded - continue to InteractionEngine");
                return input;
            }

            // Try...Catch... I'm calling user code here
            try
            {
                var output = _inputProviders[input.Mime].Process(input);
                if (output == null)
                {
                    throw new NullReferenceException($"Input Provider '{_inputProviders[input.Mime].GetType().Name}' returned a null result for MIME type '{input.Mime}'");
                }

                resolveAttempts++;
                ResolveInput((IOData)output, resolveAttempts);
            }
            catch (Exception)
            {
                throw;
            }
            
            // In theory this will never be reached, it is only added here
            // to avoid compile issues of 'not all code paths return a value.
            // Why? 
            //  If the mime type is not supported, we return
            //  If the output is null, we return
            // TODO: Refactor this method
            return null;
        }
    }
}