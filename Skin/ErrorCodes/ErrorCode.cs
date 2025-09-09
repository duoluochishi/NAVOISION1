//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/12/02 16:00:53     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.ErrorCodes;

/// <summary>
/// 错误码
/// </summary>
public class ErrorCode
{
    public string Key { get; }
    public string Code { get; set; }
    public ErrorLevel Level { get; set; }
    public string Description { get; set; }
    public string Reason { get; set; }
    public string Solution { get; set; }
    public string ExceptionHandling { get; set; }

    public ErrorCode(string key) : this(key, "", ErrorLevel.None, "", "", "", "")
    {
    }

    public ErrorCode(string key, string code, ErrorLevel level, string description, string reason, string solution, string exceptionHandling)
    {
        Key = key;
        Code = code;
        Level = level;
        Description = description;
        Reason = reason;
        Solution = solution;
        ExceptionHandling = exceptionHandling;
    }
}
