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
using System.Collections.Generic;

using Nancy;
using Nancy.Extensions;
using Nancy.ViewEngines;
using Nancy.ErrorHandling;
using Nancy.Responses.Negotiation;

namespace Limitless.Loaders
{
    /// <summary>
    /// Implements the loading of custom error pages for Nancy.
    /// Mostly taken from https://blog.tommyparnell.com/custom-error-pages-in-nancy/
    /// and the default handler implemented in Nancy.
    /// </summary>
    public class CustomErrorsLoader : IStatusCodeHandler
    {
        /// <summary>
        /// The list of error codes we process.
        /// </summary>
        public static Dictionary<int, string> Checks { get { return _checks; } }
        /// <summary>
        /// The renderer.
        /// </summary>
        private IViewRenderer _viewRenderer;
        /// <summary>
        /// The list of error codes we process.
        /// </summary>
        private static Dictionary<int, string> _checks = new Dictionary<int, string>();
        /// <summary>
        /// Response negotiator for the error codes.
        /// </summary>
        private IResponseNegotiator _responseNegotiator;

        /// <summary>
        /// Constructor with IViewRenderer injected.
        /// </summary>
        /// <param name="viewRenderer">The view renderer to use</param>
        public CustomErrorsLoader(IViewRenderer viewRenderer, IResponseNegotiator responseNegotiator)
        {
            _viewRenderer = viewRenderer;
            _responseNegotiator = responseNegotiator;
        }

        /// <summary>
        /// Check if we handle the given status code.
        /// </summary>
        /// <param name="statusCode">The status code to check</param>
        /// <param name="context">The current context</param>
        /// <returns>true if we handle, false otherwise</returns>
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return _checks.ContainsKey((int)statusCode);
        }

        /// <summary>
        /// Add a status code as being checked.
        /// </summary>
        /// <param name="code">The HTTP status code</param>
        /// <param name="message">The HTTP status message</param>
        public static void AddCode(int code, string message)
        {
            if (_checks.ContainsKey(code))
            {
                _checks[code] = message;
            }
            else
            {
                _checks.Add(code, message);
            }
        }

        /// <summary>
        /// Remove a status code from being checked.
        /// </summary>
        /// <param name="code">The HTTP status code</param>
        public static void RemoveCode(int code)
        {
            _checks.Remove(code);
        }

        /// <summary>
        /// Disable from checking any codes.
        /// </summary>
        public static void Disable()
        {
            _checks = new Dictionary<int, string>();
        }

        /// <summary>
        /// Handle the status code.
        /// </summary>
        /// <param name="statusCode">The status code to handle</param>
        /// <param name="context">The current context</param>
        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (context.Response != null && context.Response.Contents != null && !ReferenceEquals(context.Response.Contents, Response.NoBody))
            {
                return;
            }
            if (!_checks.ContainsKey((int)statusCode))
            {
                return;
            }

            Exception ex;
            dynamic result = new ExpandoObject();

            if (context.TryGetException(out ex))
            {
                result.Type = context.GetException().GetType();
                result.Message = _checks[(int)statusCode];
                result.StackTrace = context.GetExceptionDetails();
                result.Target = $"{context.GetException().TargetSite.Module}.{context.GetException().TargetSite.Name}";
            }
            else
            {
                result.Type = statusCode;
                result.Message = _checks[(int)statusCode];
                result.StackTrace = "";
                result.Target = "";
            }

            try
            {
                context.Response = _responseNegotiator.NegotiateResponse(result, context);
                context.Response.StatusCode = statusCode;
                return;
            }
            catch (Exception)
            {
                // Move on to HTML view
            }
            // Wrap the exception in an HTML <pre> tag
            result.StackTrace = string.Concat("<pre>", context.GetExceptionDetails().Replace("<", "&gt;").Replace(">", "&lt;"), "</pre>");
            result.StatusCode = (int)statusCode;
            var response = _viewRenderer.RenderView(context, "views/errors/" + (int)statusCode + ".html", result);
            response.StatusCode = statusCode;
            context.Response = response;
        }
    }
}
