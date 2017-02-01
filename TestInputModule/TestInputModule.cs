using System;
using System.Collections.Generic;

using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace TestInputModule
{
    public class TestInputConfig
    {
        public string TestName { get; set; }
    }

    public class TestInputModule : IModule, IIOProcessor
    {
        ILogger _log;

        public TestInputModule(ILogger log)
        {
            _log = log;
        }

        public IOStage Stage { get; set; } = IOStage.PreProcessor;

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
                new SupportedIOCombination(new MimeLanguage("text/plain", "en-ZA"), new MimeLanguage("text/plain", "en-US")),
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

        public IOData Process(IOData input, MimeLanguage preferredOutput)
        {
            _log.Debug("Processing Input...");
            //return new IOData("application/vnd.limitless.intent+json", "asdsad");
            if (input.MimeLanguage.Language == "en-ZA")
                return new IOData(new MimeLanguage("text/plain", "en-US"), input.Data + ". ZA, US");
            if (input.MimeLanguage.Language == "en-GB")
                return new IOData(new MimeLanguage("text/plain", "en-US"), input.Data + ". GB, US");

            return new IOData(new MimeLanguage("text/plain", "en"), input.Data + " EN");
        }
    }
}
