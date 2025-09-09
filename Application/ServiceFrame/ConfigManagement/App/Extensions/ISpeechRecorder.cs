//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.ConfigManagement.Extensions;

public interface ISpeechRecorder
{
    /// <summary>
    /// The SetFileName.
    /// </summary>
    /// <param name="fileName">The fileName<see cref="string"/>.</param>
    void SetFileName(string fileName);

    /// <summary>
    /// The StartRec.
    /// </summary>
    void StartRec();

    /// <summary>
    /// The StopRec.
    /// </summary>
    void StopRec();
}