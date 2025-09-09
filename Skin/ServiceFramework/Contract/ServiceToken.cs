namespace NV.CT.ServiceFramework.Contract;

public static class ServiceToken
{
    private static string CurrentServiceToken = string.Empty;
    public static bool Take(string appID)
    {
        if (CurrentServiceToken == string.Empty)
        {
            CurrentServiceToken = appID;
            return true;
        }

        if (CurrentServiceToken == appID)
        {
            return true;
        }
        return false;
    }

    public static bool Release(string appID)
    {
        if (CurrentServiceToken == appID)
        {
            CurrentServiceToken = string.Empty;
            return true;
        }
        return false;
    }

    public static bool Release()
    {
        CurrentServiceToken = string.Empty;
        return true;
    }

    public static string GetCurrentServiceToken()
    {
        return CurrentServiceToken;
    }
}
