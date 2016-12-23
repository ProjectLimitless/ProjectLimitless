/**
* This is a development module - Will be removed later.
*/
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Attributes;

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

        [APIRoute(Path = "/demo/ping/{name}", Method = HttpMethod.Get, RequiresAuthentication = true)]
        public dynamic PersonalPong(dynamic parameters, dynamic user)
        {
            return $"Pong {parameters.name} for user {user.UserName}";
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
