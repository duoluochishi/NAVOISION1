using System;

namespace NV.CT.LogManagement.Models
{
    public class LogFileProfileModel
    {
        public string FileName { get; set; }

        public string FileFullPath { get; set; }

        public DateTime LastWriteTime { get; set; }

    }
}
