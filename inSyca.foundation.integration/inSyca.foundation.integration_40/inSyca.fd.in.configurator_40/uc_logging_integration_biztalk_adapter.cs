﻿using System.Xml.Linq;
using inSyca.foundation.framework.application;
using inSyca.foundation.framework.application.windowsforms;

namespace inSyca.foundation.integration.configurator
{
    public partial class uc_logging_integration_biztalk_adapter : uc_logging
    {
        internal uc_logging_integration_biztalk_adapter(configXml _configFile)
            : base(_configFile)
        {
            InitializeComponent();
        }
    }
}
