using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTool.Model
{
    
    public class SettingsModel
    {
        public string ConfigurationAssemblyPath { get; set; }
        public string BaseConfigFolder { get; set; }
        public string NewConfigFolder { get; set; }
        public string BeyondCompareFullPath { get; set; }
    }
}
