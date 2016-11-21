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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limitless.Config
{
    /// <summary>
    /// CoreSettings holds the structure of the configuration TOML files.
    /// </summary>
    public class CoreSettings
    {
        ///TODO: Split into proper files and classes
        public string Test { get; set; }
        public OwnerSettings Owner { get; set; } = new OwnerSettings();
        public UpdatesSettings Updates { get; set; } = new UpdatesSettings();

        public class OwnerSettings
        {
            public string Name { get; set; }
        }

        public class UpdatesSettings
        {
            public UpdateSetting Primary { get; set; } = new UpdateSetting();
            public UpdateSetting Secondary { get; set; } = new UpdateSetting();

            public class UpdateSetting
            {
                public string Host { get; set; }
            }
        }
    }
}