namespace NV.CT.CTS.Helpers;

public static class DirectoryHelper
{
    public static bool CheckAndCreate(string path)
    {
        if (Directory.Exists(path)) return true;

        Directory.CreateDirectory(path);
        return Directory.Exists(path);
    }

    public static bool DeleteDirectory(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return true;

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
            }
        }

        var directories = Directory.GetDirectories(path);

        foreach(var directory in directories)
        {
            DeleteDirectory(directory);

            try
            {
                Directory.Delete(directory);
            }
            catch
            {
            }
        }

        try
        {
            Directory.Delete(path);
        }
        catch
        {
        }

        return true;
    }
}
