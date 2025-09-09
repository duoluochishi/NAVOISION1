using System.Diagnostics;
using System.IO;
using NV.CT.Service.Models;
using NV.MPS.Environment;

namespace NV.CT.Service.Common.Utils
{
    public static class DataAnalysisToolUtil
    {
        private static readonly string ToolPath = Path.Combine(RuntimeConfig.Console.MCSTools.Path, "DataAnalysis", "NV.CT.Tool.DataAnalysis.exe");

        public static GenericResponse OpenFolder(string path)
        {
            return Open($"Folder {path}");
        }

        public static GenericResponse OpenFiles(params string[] files)
        {
            return Open($"File {string.Join(' ', files)}");
        }

        private static GenericResponse Open(string argument)
        {
            if (!File.Exists(ToolPath))
            {
                return new GenericResponse(false, $"Data analysis tool does not exist: {ToolPath}");
            }

            var info = new ProcessStartInfo(ToolPath, argument)
            {
                WorkingDirectory = Path.GetDirectoryName(ToolPath),
            };
            using var _ = Process.Start(info);
            return new GenericResponse(true, string.Empty);
        }
    }
}