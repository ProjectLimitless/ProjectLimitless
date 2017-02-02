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
using System.Linq;
using System.Collections.Generic;

using Limitless.Builtin;
using Limitless.Containers;
using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace Limitless.Managers
{
    /// <summary>
    /// Manages inputs and outputs from and to the Project Limitless API.
    /// 
    /// //TODO: Rename to IORouter? InputRouter?
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
        /// The collection of available input providers.
        /// </summary>
        private readonly List<IIOProcessor> _inputProviders;
        /// <summary>
        /// The collection of available output providers.
        /// </summary>
        private readonly List<IIOProcessor> _outputProviders;
        
        /// <summary>
        /// Creates a new instance of <see cref="IOManager"/>
        /// and sets the <see cref="ILogger"/>.
        /// </summary>
        /// <param name="log">The </param>
        public IOManager(ILogger log)
        {
            _log = log;
            _inputProviders = new List<IIOProcessor>();
            _outputProviders = new List<IIOProcessor>();
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
        /// <param name="request">The request object for the API call</param>
        /// <returns>The input response</returns>
        public APIResponse Handle(APIRequest request)
        {
            _log.Trace($"Handle input from client. Content-Type is '{request.Headers.ContentType}'");
            var response = new APIResponse();


            // Check the Content-Type
            // If it is application/json
            // TODO: if will be parsed into IOData that may contain multiple
            // TODO: Define custom JSON? application/vnd.limitless.input+json?
            // inputs.
            if (request.Headers.ContentType == MimeType.Json)
            {
                _log.Debug("Received JSON, parse into usable type");
                throw new NotImplementedException("JSON received. Needs to be parsed into usable type");
            }

            // Find out what Mime/Language input combinations are supported by the interaction engine
            // This will determine what the input pipeline should attempt to produce
            // TODO: Support multiple IO combinations for more advanced engines
            SupportedIOCombination engineCombinations = _engine.GetSupportedIOCombinations().First();

            // TODO: Resolve multi-request parallel - ie. speech recognition + voice recognition id
            // Attempt to process input to reach the engineCombinations
            var ioData = new IOData(new MimeLanguage(request.Headers.ContentType, request.Headers.RequestLanguage), request.Data);
            ioData = Resolve(ioData, engineCombinations.SupportedInput, _inputProviders);

            // Let the engine do its thing
            ioData = _engine.ProcessInput(ioData);

            // TODO Find solution to multiple accepted languages
            // What is the client expecting in return
            Tuple<string, decimal> preferredMime = request.Headers.Accept.OrderByDescending(x => x.Item2).First();
            // Multiple languages could be present, this selects the language with the highest weighted value
            // More on languages and weighted values: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
            // The tuple contains <language,weight>
            Tuple<string, decimal> preferredLanguage = request.Headers.AcceptLanguage.OrderByDescending(x => x.Item2).First();
            ioData = Resolve(ioData, new MimeLanguage(preferredMime.Item1, preferredLanguage.Item1), _outputProviders);
            
            // Set the headers for the data.
            // TODO: In future this will be part of the response
            // not just a list.
            response.Data = ioData.Data;
            var header = new
            {
                Header = "Content-Type",
                Value = ioData.MimeLanguage.Mime
            };
            response.Headers.Add(header);
            header = new
            {
                Header = "Language",
                Value = ioData.MimeLanguage.Language
            };
            response.Headers.Add(header);

            return response;
        }

        /// <summary>
        /// Registers an input provider for the MIME types as 
        /// returned by the provider.
        /// </summary>
        /// <param name="processor">The processor to register</param>
        public void RegisterProvider(IIOProcessor processor)
        {
            if (processor.Stage == IOStage.PreProcessor)
            {
                _inputProviders.Add(processor);
            }
            else
            {
                _outputProviders.Add(processor);
            }
        }

        /// <summary>
        /// Deregisters an input provider for the MIME types as 
        /// returned by the provider.
        /// </summary>
        /// <param name="processor">The processor to deregister</param>
        public void DeregisterProvider(IIOProcessor processor)
        {
            if (processor.Stage == IOStage.PreProcessor)
            {
                _inputProviders.Remove(processor);
            }
            else
            {
                _outputProviders.Remove(processor);
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
        /// or LimitlessSettings MaxResolveAttempts is reached
        /// </summary>
        /// <param name="input">The input data</param>
        /// <param name="preferredOutput">The preferred output Mime and Language</param>
        /// <param name="providers">The providers available to process input</param>
        /// <param name="resolveAttempts">The maximum attempts to extract usable data from input</param>
        /// <returns>The processed data from the pipeline</returns>
        private IOData Resolve(IOData input, MimeLanguage preferredOutput, List<IIOProcessor> providers, int resolveAttempts = 0)
        {
            /* Resolving the input provider to use happens in the following order
             * 1. Use the first provider that matches both input and preferredOutput
             * 2. Use the first provider that matches input and preferredOutput.Language[0,2]
             * 3. Use the first provider that matches input.Language[0,2] and preferredOutput.Language[0,2]
             * 4. Use the first provider that matched input and output Mimes
             * 5. Fail
             */

            // TODO: Add support for wildcard mime and language */*
            // Get a copy of the list so that we don't change the instance list
            providers = new List<IIOProcessor>(providers);
            if (resolveAttempts >= CoreContainer.Instance.Settings.Core.MaxResolveAttempts)
            {
                throw new NotSupportedException("Maximum attempts reached for providing data in the preferred format");
            }

            //Use the first provider that matches both input and preferredOutput
            var availableProviders = providers.Where(
                                        x => x.GetSupportedIOCombinations().Count
                                        (
                                            y => y.SupportedInput.Mime == input.MimeLanguage.Mime && y.SupportedInput.Language == input.MimeLanguage.Language &&
                                                y.SupportedOutput.Mime == preferredOutput.Mime && y.SupportedOutput.Language == preferredOutput.Language
                                        ) > 0);
            if (availableProviders.Count<IIOProcessor>() == 0)
            {
                availableProviders = providers.Where(
                                            x => x.GetSupportedIOCombinations().Count
                                            (
                                                y => y.SupportedInput.Mime == input.MimeLanguage.Mime && y.SupportedInput.Language == input.MimeLanguage.Language &&
                                                    y.SupportedOutput.Mime == preferredOutput.Mime && y.SupportedOutput.Language == preferredOutput.Language.Substring(0,2)
                                            ) > 0);
            }
            if (availableProviders.Count<IIOProcessor>() == 0)
            {
                availableProviders = providers.Where(
                                            x => x.GetSupportedIOCombinations().Count
                                            (
                                                y => y.SupportedInput.Mime == input.MimeLanguage.Mime && y.SupportedInput.Language == input.MimeLanguage.Language.Substring(0, 2) &&
                                                    y.SupportedOutput.Mime == preferredOutput.Mime && y.SupportedOutput.Language == preferredOutput.Language.Substring(0, 2)
                                            ) > 0);
            }
            if (availableProviders.Count<IIOProcessor>() == 0)
            {
                availableProviders = providers.Where(
                                            x => x.GetSupportedIOCombinations().Count
                                            (
                                                y => y.SupportedInput.Mime == input.MimeLanguage.Mime && y.SupportedOutput.Mime == preferredOutput.Mime
                                            ) > 0);
            }
            // Nothing matched
            if (availableProviders.Count<IIOProcessor>() == 0)
            {
                _log.Trace($"No provider available for input types '{input.MimeLanguage}' providing output types '{preferredOutput}'. Returning current");
                return input;
            }

            _log.Trace($"Found {availableProviders.Count<IIOProcessor>()} provider(s) for input {input.MimeLanguage} and output {preferredOutput}");

            var provider = availableProviders.First();

            // After process is done the output becomes the
            // input for the next provider
            input = provider.Process(input, preferredOutput);
            if (input == null)
            {
                throw new NullReferenceException($"Provider '{provider.GetType().Name}' returned a null result for input '{input.MimeLanguage}'");
            }

            resolveAttempts++;
            if (providers.Remove(provider))
            {
                Resolve(input, preferredOutput, providers, resolveAttempts);
            }
            else
            {
                throw new InvalidOperationException($"The provider '{provider.GetType().Name}' could not be removed from the available list");
            }
            
            // In theory this will never be reached
            // Why? 
            //  If the mime type is not supported, we return
            //  If the output is null, we return
            return input;
        }
        
        /// <summary>
        /// Creates the required routes for the IOManager.
        /// </summary>
        /// <param name="handler">A handler to wrap around the call. Takes the output and API input parameters as input.</param>
        /// <returns>The list of API routes</returns>
        internal List<APIRoute> GetRequiredRoutes(Func<dynamic, object[], dynamic> handler = null)
        {
            // TODO: Rethink inputs
            // Might be audio, text, video, image, gesture, etc
            // Will be from a client app, probably not a user directly
            // Audio might output text but can output intent as well
            // Intent needs to be executed and response needs to be
            // negotiated based on the input
            // maybe a ll-accept header / ll-content-type?

            // Inputs can be sent as raw mime byte data or
            // as part of JSON as base64 encoded fields. 
            // Here I sent up the processing of the input
            // API route that handles the data sent by the client
            // application
            var routes = new List<APIRoute>();

            var route = new APIRoute
            {
                Path = "/input",
                Description = "Process the input",
                Method = HttpMethod.Post,
                Handler = Handle,
                RequiresAuthentication = true
            };
            routes.Add(route);

            return routes;
        }
    }
}