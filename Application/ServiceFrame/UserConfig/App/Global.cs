using System;

namespace NV.CT.UserConfig;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private Global()
    {
    }
    public static Global Instance => _instance.Value;
}