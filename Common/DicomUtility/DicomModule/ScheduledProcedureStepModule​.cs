//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomSQ;

namespace NV.CT.DicomUtility.DicomModule
{


    /// <summary>
    /// 来源Part04 K6.1.2.2
    /// 具体定义part03 C.4.10
    /// 在Part04中未列出的暂不考虑。
    /// 
    public class ScheduledProcedureStepModule​:IDicomDatasetUpdater
    {
        public ScheduledProcedureStepSQ ScheduledProcedureStepSQ { get; set; }
        public ScheduledSpecimenSQ ScheduledSpecimenSQ { get; set; }
        public string BarcodeValue { get; set; }

        public ScheduledProcedureStepModule​()
        {
            ScheduledProcedureStepSQ = new ScheduledProcedureStepSQ ();
            ScheduledSpecimenSQ = new ScheduledSpecimenSQ ();
        }

        public void Read(DicomDataset ds)
        {
            ScheduledProcedureStepSQ.Read(ds);
            ScheduledSpecimenSQ.Read(ds);
            DicomContentHelper.GetDicomTag<string>(ds, DicomTag.BarcodeValue);
        }

        public void Update(DicomDataset ds)
        {
            ScheduledProcedureStepSQ.Update(ds);
            ScheduledSpecimenSQ.Update(ds);
            ds.AddOrUpdate(DicomTag.BarcodeValue, BarcodeValue);
        }
    }
}
