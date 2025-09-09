using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class ScreenShotPath
    {
       public static string ScreenShotDir= Path.Combine(Directory.GetParent(MPS.Environment.RuntimeConfig.Console.Backup.Path).FullName, @"WebRoot\");
    }
}
