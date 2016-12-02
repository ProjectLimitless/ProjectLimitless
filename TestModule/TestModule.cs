/**
* This is a development module - Will be removed later.
*/
using Limitless.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limitless.Runtime.Types;
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

    public class TestModule : IModule, IAPIModule
    {
        public void Configure(dynamic rawSettings)
        {
            // Safe to cast it here, the loader specifically loads the configuration
            // of this TestSettings type. The 'dynamic' type is to simpify the IModule interface.
            // However, there is not need to cast it, you can use rawSettings directly is you wish.
            TestSettings settings = (TestSettings)rawSettings;

            Console.WriteLine("This is TestModule");
            Console.WriteLine($"Name: {settings.Name}");
            Console.WriteLine($"Name (dynamic): {rawSettings.Name}");
            Console.WriteLine($"API Host: {settings.API.Host}");
            Console.WriteLine($"API Host (dynamic): {rawSettings.API.Host}");
        }

        public Type GetConfigurationType()
        {
            return typeof(TestSettings);
        }

        public dynamic PersonalPong(dynamic input)
        {
            return $"Pong {input.name}";
        }

        [APIRoute(Path = "/ass")]
        public dynamic Ass(dynamic input)
        {
            return "Yes my ass!";
        }

        public List<APIRouteHandler> GetRoutes()
        {
            List<APIRouteHandler> routes = new List<APIRouteHandler>();

            APIRouteHandler handler = new APIRouteHandler();
            handler.Route = "/ping";
            handler.Handler = (dynamic input) =>
            {
                return "Pong!";
            };
            routes.Add(handler);

            handler = new APIRouteHandler();
            handler.Route = "/ping/{name}";
            handler.Handler = PersonalPong;
            routes.Add(handler);
            
            return routes;
        }
    }
}
