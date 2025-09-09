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

namespace NV.CT.DicomUtility.Transfer.DicomDataModifier
{
    /// <summary>
    /// export匿名化支持类
    /// </summary>
    public class DicomAnonymouseHandler : IDicomDataModificationHandler
    {
        private DicomAnonymizer _dicomAnonymizer;
        public DicomAnonymouseHandler()
        {
            _dicomAnonymizer = new DicomAnonymizer();
        }
        public void ModifyDicomData(DicomDataset dataset)
        {
            _dicomAnonymizer.AnonymizeInPlace(dataset);
        }

        public void ModifyDicomFile(DicomFile dcmFile)
        {
            _dicomAnonymizer.AnonymizeInPlace(dcmFile);
        }
    }
}
