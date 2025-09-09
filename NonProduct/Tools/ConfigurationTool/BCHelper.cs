using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTool
{
    public static class BCHelper
    {
        public static void StartBeyondCompare(string baseConfigFolder, string newConfigFolder, string bcFullPath)
        {
            try
            {
                if (string.IsNullOrEmpty(baseConfigFolder) || string.IsNullOrEmpty(newConfigFolder))
                    return;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = bcFullPath, //@"C:\Program Files\Beyond Compare 5\BCompare.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = $"\"{baseConfigFolder}\" \"{newConfigFolder}\""
                    }
                };

                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
