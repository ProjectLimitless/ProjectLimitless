using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Types;

namespace TestInteractionEngine
{
    public class TestInteractionEngineConfig
    {

    }

    public class TestInteractionEngine : IModule, IInteractionEngine
    {
        private ILogger _log;

        public TestInteractionEngine(ILogger log)
        {
            _log = log;
            _log.Info("Test Interaction Engine created");
        }

        public void Configure(dynamic settings)
        {
            // Nothing to do
        }

        public void DeregisterSkill()
        {
            throw new NotImplementedException();
        }

        public string GetAuthor()
        {
            return "Limitless Testing";
        }

        public Type GetConfigurationType()
        {
            return typeof(TestInteractionEngineConfig);
        }

        public string GetDescription()
        {
            return "Testing interaction engine";
        }

        public string GetTitle()
        {
            return "Test Interaction Engine";
        }

        public string GetVersion()
        {
            return "0.0.0.1";
        }

        public List<dynamic> ListSkills()
        {
            var skills = new List<dynamic>();
            skills.Add(new
            {
                Name = "Manage Google Schedule",
                Intent = new
                {
                    Action = "Book",
                    Target = "Meeting"
                }
            });
            return skills;
        }

        public IOData ProcessInput(IOData ioData)
        {
            _log.Trace($"Received IOData input of MIME type '{ioData.Mime}'");

            return new IOData("text/plain", "This is the output!");
        }

        public void RegisterSkill()
        {
            throw new NotImplementedException();
        }
    }
}
