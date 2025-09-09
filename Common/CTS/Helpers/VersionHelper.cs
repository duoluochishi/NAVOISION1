using System.Diagnostics;

namespace NV.CT.CTS.Helpers;

public static class VersionHelper
{
    public static string GetSoftwareVersion()
    {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? string.Empty;
    }
}
