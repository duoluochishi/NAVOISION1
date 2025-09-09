using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NV.CT.DmsTest.Config
{
    internal class Config
    {
        public static  readonly Config Instance = new();

        public DMSTestConfig LoadConfig()
        {
            DMSTestConfig config = new DMSTestConfig();
            using (StreamReader file = File.OpenText("D:\\Config\\ConfigMCS\\ConfigRoot\\DMSTest\\DMSTestConfig.json"))
            {
                string str = file.ReadToEnd();
                config = JsonSerializer.Deserialize<DMSTestConfig>(str)!;
                
            }
            return config;
        }
       
    }
}
