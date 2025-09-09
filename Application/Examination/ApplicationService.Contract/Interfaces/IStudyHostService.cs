//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.Examination.ApplicationService.Contract.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IStudyHostService
{
    string StudyId { get; set; }

    StudyModel Instance { get; }

    event EventHandler<EventArgs<StudyModel>> StudyChanged;

    bool UpdateProtocol(StudyModel model, ProtocolModel protocol);

    bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus);
}