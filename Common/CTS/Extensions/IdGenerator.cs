namespace NV.CT.CTS.Extensions;

/// <summary>
/// ID生成器
/// </summary>
public static class IdGenerator
{
    private static int current = 0;
    private static int MaxValue = 999;
    private static long lastTimestamp = GetTimestamp(DateTime.Now);
    private static object objLock = new object();
    private static readonly DateTime orginal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// 获取下一个ID
    /// </summary>
    /// <param name="category">类别，仅支持0-9</param>
    /// <returns></returns>
    public static string Next(int category = 0)
    {
        lock (objLock)
        {
            var currentTimestamp = GetTimestamp(DateTime.Now);
            if (currentTimestamp == lastTimestamp)
            {
                current++;
                if (current >= MaxValue)
                {
                    NextTimestamp();
                    current = 0;
                }
            }
            else
            {
                lastTimestamp = currentTimestamp;
                current = 0;
            }
            return $"{DateTime.Now.ToString("yyMMddHHmmss")}{category}{current.ToString("D3")}";
        }
    }

    private static void NextTimestamp()
    {
        var currentTimestamp = GetTimestamp(DateTime.Now);
        while (lastTimestamp == currentTimestamp)
        {
            currentTimestamp = GetTimestamp(DateTime.Now);
        }
        lastTimestamp = currentTimestamp;
    }

    private static long GetTimestamp(DateTime dt)
    {
        var ts = dt.Subtract(orginal);
        return (long)ts.TotalSeconds;
    }

    /// <summary>
    /// 获取下一个自动ID
    /// </summary>
    /// <returns></returns>
    public static string NextRandomID()
    {
        lock (objLock)
        {
            var random = new Random(DateTime.Now.Second);
            return $"{DateTime.Now.ToString("yyMMddHHmmss")}{random.Next(0,9)}";
        }
    }
}
