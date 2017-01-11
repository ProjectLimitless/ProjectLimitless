﻿/** 
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

using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Types;
using System.Dynamic;
using Limitless.Runtime.Enums;

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
            else if (_inputProviders.ContainsKey(parameters.contentType))
            {
                _log.Debug($"Received content of type '{parameters.contentType}'. Provider available.");
            }
            else
            {
                _log.Warning($"Content type '{parameters.contentType}' has no input provider available.");
            }
            
            


            response.Data = "thisisbase64data";

            var header = new
            {
                Header = "Content-Type",
                Value = "text/base64"
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
                if (_inputProviders.Keys.Contains(mime))
                {
                    throw new NotSupportedException($"MIME Type '{mime}' already has an input provider registered '{_inputProviders[mime].GetType().Name}'");
                }
                else
                {
                    _inputProviders.Add(mime, provider);
                }
            }
        }
    }
}
