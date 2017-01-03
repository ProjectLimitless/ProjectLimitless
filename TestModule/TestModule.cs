/**
* This is a development module - Will be removed later.
*/
using System;
using System.Reflection;

using Limitless.Runtime.Enums;
using Limitless.Runtime.Attributes;
using Limitless.Runtime.Interfaces;

namespace TestModule
{
    public class TestSettings
    {
        public string Name { get; set; }
        public APISettings API { get; set; }

        public class APISettings
        {
            public string Host { get; set; }
        }
    }

    /// <summary>
    /// Testing module
    /// </summary>
    public class TestModule : IModule, IUIModule
    {
        private string _instanceValue;
        private ILogger _log;

        public TestModule()
        {
            throw new NotSupportedException("TestModule requires an ILogger");
        }

        public TestModule(ILogger log)
        {
            _log = log;
            _log.Info("Constructor with log");
        }

        public void Configure(dynamic rawSettings)
        {
            // Safe to cast it here, the loader specifically loads the configuration
            // of this TestSettings type. The 'dynamic' type is to simpify the IModule interface.
            // However, there is not need to cast it, you can use rawSettings directly is you wish.
            TestSettings settings = (TestSettings)rawSettings;
            _instanceValue = "This is an instance value";
            _log.Trace("This is TestModule");
            _log.Debug($"Name: {settings.Name}");
            _log.Debug($"Name (dynamic): {rawSettings.Name}");
            _log.Debug($"API Host: {settings.API.Host}");
            _log.Debug($"API Host (dynamic): {rawSettings.API.Host}");
            _log.Debug("Checking if format works: {0}!", "Yes it does");
        }

        public Type GetConfigurationType()
        {
            return typeof(TestSettings);
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetTitle"/>
        /// </summary>
        public string GetTitle()
        {
            var assembly = typeof(TestModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attribute != null)
            {
                return attribute.Title;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            var assembly = typeof(TestModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (attribute != null)
            {
                return attribute.Company;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetVersion"/>
        /// </summary>
        public string GetVersion()
        {
            var assembly = typeof(TestModule).Assembly;
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetDescription"/>
        /// </summary>
        public string GetDescription()
        {
            var assembly = typeof(TestModule).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
            return "Unknown";
        }

        [APIRoute(Path = "/demo/ping/{name}", Method = HttpMethod.Get, RequiresAuthentication = true)]
        public dynamic PersonalPong(dynamic parameters, dynamic user)
        {
            // With Expando type
            dynamic obj = new System.Dynamic.ExpandoObject();
            obj.Action = "Pong";
            obj.Name = parameters.name;
            obj.User = user.UserName;
            return obj;
        }

        [APIRoute(Path = "/test/post", Method = HttpMethod.Post, RequiredFields = new string[] { "name" })]
        public dynamic PersonalPost(dynamic parameters, dynamic postData)
        {
            // With anonymous types
            return new
            {
                Action = "PostPong",
                Name = (string)postData.name
            };
        }

        [APIRoute(Path = "/demo/{version}", Method = HttpMethod.Get, Description = "Sample Route containing a version parameter")]
        public dynamic Ass(dynamic parameters)
        {
            return "Yes my demo! " + parameters.version + " (" + _instanceValue + ")";
        }

        public string GetContentPath()
        {
            return "ui-demo";
        }
    }
}
