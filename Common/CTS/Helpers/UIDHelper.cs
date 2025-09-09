namespace NV.CT.CTS.Helpers;

public static class UIDHelper
{
    private static int current = 0;
    private static int MaxValue = 9999;
    private static long lastTimestamp = GetTimestamp(DateTime.Now);
    private static object objLock = new object();
    private static readonly DateTime orginal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static string CreateStudyInstanceUID() => $"1.2.840.1.59.0.8559.{Next(10)}.141";

    public static string CreateFORUID() => $"1.2.840.1.59.0.8549.{Next(10)}.141";

    public static string CreateSeriesInstanceUID() => $"1.2.840.1.59.0.8569.{Next(40)}.141";

    public static string CreateReconInstanceUID() => $"1.2.840.1.59.0.8579.{Next(30)}.141";

    public static string CreateSOPClassUID() => "1.2.840.10008.5.1.4.1.1.2";
    public static string CreateSOPInstanceUID() => $"1.2.840.1.59.0.8589.{NextSOPInstanceUID(20)}.141";

    public static string CreateIrradiationEventUID() => $"1.2.156.14702.1.1005.64.1.{DateTime.Now:yyyyMMddHHmmssffff}";

    public static string CreateStudyID() => NextWithMaxLength16(); //按照DICOM标准，StudyID SH 最大长度为16chars

    /// <summary>
    /// 获取下一个ID
    /// </summary>
    /// <param name="category">类别，仅支持0-9</param>
    /// <returns></returns>
    private static string Next(int category = 0)
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
            return $"{DateTime.Now.ToString("yyMMddHHmmss")}{category.ToString("D2")}{current.ToString("D2")}";
        }
    }
    private static string NextSOPInstanceUID(int category = 0)
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
            return $"{DateTime.Now.ToString("yyMMddHHmmss")}{category.ToString("D2")}{current.ToString("D4")}";
        }
    }

    /// <summary>
    /// 获取下一个最大长度为16的ID
    /// </summary>
    /// <returns></returns>
    private static string NextWithMaxLength16()
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
            return $"NV{DateTime.Now.ToString("yyMMddHHmmss")}{current.ToString("D2")}";
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
}
