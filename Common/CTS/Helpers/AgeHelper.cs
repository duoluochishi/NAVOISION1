//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/24 10:28:30     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Helpers;

public static class AgeHelper
{
    public static DateTime GetBirthday(AgeType ageType, int age)
    {
        switch (ageType)
        {
            case AgeType.Year:
                return DateTime.Now.AddYears(-age);
            case AgeType.Month:
                if (age % 12 == 0 && age != 0)
                {
                    int years = (age / 12);
                    return DateTime.Now.AddYears(-years);
                }
                else
                {
                    return DateTime.Now.AddMonths(-age);
                }
            case AgeType.Week:
                return DateTime.Now.AddDays((-age) * 7);
            case AgeType.Day:
                return DateTime.Now.AddDays(-age);
            default:
                return default;
        }
    }

    public static (int, AgeType) CalculateAgeByBirthday(DateTime birthday)
    {
        if (birthday <= DateTime.MinValue || birthday >= DateTime.MaxValue)
        {
            return (0, AgeType.Year);
        }

        int age;
        AgeType ageType;
        TimeSpan span = DateTime.Now.Subtract(birthday);
        int diff = span.Days;
        if (diff >= 365)
        {
            age = diff / 365;
            ageType = AgeType.Year;
        }
        else if (diff >= 30)
        {
            age = diff / 30;
            ageType = AgeType.Month;
        }
        else if (diff >= 7)
        {
            age = diff / 7;
            ageType = AgeType.Week;
        }
        else
        {
            age = diff;
            ageType = AgeType.Day;
        }

        return (age, ageType);
    }

}
