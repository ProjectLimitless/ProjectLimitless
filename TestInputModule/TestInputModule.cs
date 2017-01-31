using Limitless.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limitless.Runtime.Enums;
using System.Dynamic;
using Limitless.Runtime.Types;

namespace TestInputModule
{
    public class TestInputConfig
    {
        public string TestName { get; set; }
    }

    public class TestInputModule : IModule, IIOProvider
    {
        ILogger _log;

        public TestInputModule(ILogger log)
        {
            _log = log;
        }

        public IODirection Direction { get; set; } = IODirection.In;

        public void Configure(dynamic settings)
        {
            // Nothing to do
        }

        public string GetAuthor()
        {
            return "Testing";
        }

        public Type GetConfigurationType()
        {
            return typeof(TestInputConfig);
        }

        public string GetDescription()
        {
            return "Testing input provider";
        }

        public IEnumerable<SupportedIOCombination> GetSupportedIOCombinations()
        {
            return new List<SupportedIOCombination>
            {
                new SupportedIOCombination(new MimeLanguage("text/plain", "en"), new MimeLanguage("text/plain", "en-ZA")),
                new SupportedIOCombination(new MimeLanguage("text/plain", "en-GB"), new MimeLanguage("text/plain", "en-US")),
            };
        }
        
        public string GetTitle()
        {
            return "TestInputModule";
        }

        public string GetVersion()
        {
            return "0.0.0.1";
        }

        public IOData Process(IOData input)
        {
            _log.Debug("Processing Input...");
            //return new IOData("application/vnd.limitless.intent+json", "asdsad");
            if (input.MimeLanguage.Language == "en")
                return new IOData(new MimeLanguage("text/plain", "en-ZA"), input.Data + " ZA");
            if (input.MimeLanguage.Language == "en-GB")
                return new IOData(new MimeLanguage("text/plain", "en-US"), input.Data + " US");

            return new IOData(new MimeLanguage("text/plain", "en"), input.Data + " EN");
        }
    }
}
