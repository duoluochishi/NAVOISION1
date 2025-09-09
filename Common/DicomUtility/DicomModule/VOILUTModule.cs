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

namespace NV.CT.DicomUtility.DicomModule
{
    public class VOILUTModule : IDicomDatasetUpdater
    {
        public double[] WindowWidth { get; set; }

        public double[] WindowCenter { get; set; }

        public void Update(DicomDataset ds)
        {
            //当前先只考虑写第一个吧~
            if(WindowWidth is not null && WindowCenter is not null)
            {
                ds.AddOrUpdate(DicomTag.WindowWidth, WindowWidth[0]);
                ds.AddOrUpdate(DicomTag.WindowCenter, WindowCenter[0]);
            }
        }
        public void Read(DicomDataset ds)
        {
            WindowWidth = DicomContentHelper.GetDicomTags<double>(ds, DicomTag.WindowWidth);
            WindowCenter = DicomContentHelper.GetDicomTags<double>(ds, DicomTag.WindowCenter);
        }
    }
}
