using NV.CT.CTS.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.EntitySql;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NV.CT.NP.Tools.DataTransfer.Utils
{
    /// <summary>
    /// Dicom:
    /// base path: PatientId/StudyInstanceUID/
    /// full path: TartgePath/PatientId/StudyInstanceUID/SeriesInstaceUID/文件名    
    /// RawData:
    /// base path: PatientId/StudyInstanceUID/
    /// full path: TartgePath/PatientId/RawData/StudyInstanceUID/ScanID/文件名
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// RoboCopy
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="logPath"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> CopyAsync(string source, string destination, string logPath = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(logPath))
                    logPath = Path.Combine(destination, "robocopy_log.txt");

                var logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "robocopy",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Arguments = $"\"{source}\" \"{destination}\" /E /COPY:DAT /R:3 /W:5 /V /LOG+:\"{logPath}\""
                    }
                };

                process.Start();

                using (cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                    }
                    catch
                    {
                        // Ignore exeption, process might have already exited.
                    }
                }))
                {
                    await process.WaitForExitAsync(cancellationToken);
                }

                // 检查 robocopy 的退出代码
                // 0: 没有复制任何文件
                // 1: 成功复制了所有文件
                // 2: 发生了额外错误（如访问被拒绝）
                // 3: 部分文件被复制
                // 8: 有文件复制失败
                if (process.ExitCode > 7)
                {
                    return (false, $"Robocopy failed with exit code: {process.ExitCode}");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The path in the t_series and t_rawdata</param>
        /// <param name="studyInstanceUID"></param>
        /// <returns></returns>
        public static string GetStudyInstanceUIDDirectory(string path, string studyInstanceUID, string filter = "")
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(studyInstanceUID))
                return string.Empty;

            try
            {
                var directoryPath = File.Exists(path) ? Path.GetDirectoryName(path) : path;

                if (string.IsNullOrEmpty(directoryPath))
                    return string.Empty;

                var current = new DirectoryInfo(directoryPath);

                while (current != null)
                {
                    if (string.Equals(current.Name, studyInstanceUID, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(filter) || current.Parent?.Name == filter)
                            return current.FullName;

                        return string.Empty;
                    }
                    current = current.Parent;
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Calculate total file size and multiply by a safty factor of 1.2
        /// </summary>
        /// <param name="directories"></param>
        /// <returns></returns>
        public static long CalculateTotalFileSize(HashSet<string> directories)
        {
            long totalSize = 0;
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.Exists)
                    directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ForEach(f => totalSize += f.Length);
            }
            return (long)(totalSize * 1.2);
        }
    }
}
