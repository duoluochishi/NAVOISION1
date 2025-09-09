namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public static class FileUtils
    {
        public static void EnsureDirectoryPath(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
