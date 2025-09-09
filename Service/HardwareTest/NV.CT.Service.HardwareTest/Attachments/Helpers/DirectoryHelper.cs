using NV.CT.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class DirectoryHelper
    {
        public static void EnsureDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void EnsureDirectories(IEnumerable<string> directories)
        {
            foreach (var directory in directories)
            {
                EnsureDirectory(directory);
            }
        }

        public static GenericResponse CopyDirectory(string sourceDirectory, string targetDirectory) 
        {
            EnsureDirectory(sourceDirectory);
            EnsureDirectory(targetDirectory);

            try
            {
                string[] files = Directory.GetFiles(sourceDirectory);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string targetFilePath = Path.Combine(targetDirectory, fileName);
                    File.Copy(file, targetFilePath);
                }

                string[] folders = Directory.GetDirectories(sourceDirectory);
                foreach (var folder in folders)
                {
                    string folderName = Path.GetFileName(folder);
                    string targetFolderPath = Path.Combine(targetDirectory, folderName);
                    CopyDirectory(folder, targetFolderPath);
                }

                return new(true, "[CopyDirectory] finished.");
            }
            catch (Exception ex) 
            {
                return new(false, $"[CopyDirectory] failed, [Stack]: {ex.ToString()}");
            }
        }

    }
}
