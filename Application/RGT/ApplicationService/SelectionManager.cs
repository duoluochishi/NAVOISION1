//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.RGT.ApplicationService.Contract.Interfaces;

namespace NV.CT.RGT.ApplicationService.Impl;

public class SelectionManager: ISelectionManager
{
    public (StudyModel studyModel, PatientModel patientModel) CurrentSelection { get; private set; }

    public event EventHandler<EventArgs<(StudyModel, PatientModel)>>? SelectionChanged;

    public void SelectScan(StudyModel studyModel, PatientModel patientModel)
    {
        CurrentSelection = (studyModel, patientModel);

        SelectionChanged?.Invoke(this, new EventArgs<(StudyModel, PatientModel)>((studyModel, patientModel)));
    }

}