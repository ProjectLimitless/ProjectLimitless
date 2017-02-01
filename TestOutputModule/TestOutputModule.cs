using System;
using System.Collections.Generic;

using Limitless.Runtime.Enums;
using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace TestOutputModule
{
    public class TestOutputConfig
    {
        public string TestName { get; set; }
    }

    public class TestOutputModule : IModule, IIOProcessor
    {
        ILogger _log;

        public TestOutputModule(ILogger log)
        {
            _log = log;
        }

        public IOStage Stage { get; set; } = IOStage.PostProcessor;

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
            return typeof(TestOutputConfig);
        }

        public string GetDescription()
        {
            return "Testing output provider";
        }

        public IEnumerable<SupportedIOCombination> GetSupportedIOCombinations()
        {
            return new List<SupportedIOCombination>
            {
                new SupportedIOCombination(new MimeLanguage("text/plain", "en-US"), new MimeLanguage("text/plain", "en-ZA")),
                new SupportedIOCombination(new MimeLanguage("text/plain", "en-US"), new MimeLanguage("text/plain", "en-GB")),
            };
        }

        public string GetTitle()
        {
            return "TestOutputModule";
        }

        public string GetVersion()
        {
            return "0.0.0.1";
        }

        public IOData Process(IOData input, MimeLanguage preferredOutput)
        {
            _log.Debug("Processing Output...");
            //return new IOData("application/vnd.limitless.intent+json", "asdsad");
            if (preferredOutput.Language == "en-ZA")
                return new IOData(new MimeLanguage("text/plain", "en-ZA"), input.Data + " to ZA");
            if (preferredOutput.Language == "en-GB")
                return new IOData(new MimeLanguage("text/plain", "en-GB"), input.Data + " to GB");

            return new IOData(new MimeLanguage("text/plain", "en"), input.Data + " EN");
        }
    }
}
