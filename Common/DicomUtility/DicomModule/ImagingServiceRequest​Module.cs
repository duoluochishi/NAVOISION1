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
    /// <summary>
    /// 来源Part04 K6.1.2.2
    /// 具体定义part03 C.4.12
    /// 在Part04中未列出的暂不考虑。
    public class ImagingServiceRequest​Module:IDicomDatasetUpdater
    {
        public string AccessionNumber { get; set; }

        //public SQ IssuerOfAccessionNumberSQ​ { get; set; }

        public string RequestingPhysician​ {get;set;}

        public string ReferringPhysicianName { get; set; }

        //other attributes of ImagingServiceRequest​Module are not implemented.

        public void Read(DicomDataset ds)
        {
            AccessionNumber = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AccessionNumber);
            //IssuerOfAccessionNumberSQ.Read(ds);
            RequestingPhysician​ = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.RequestingPhysician​);
            ReferringPhysicianName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ReferringPhysicianName);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.AccessionNumber, AccessionNumber);
            //IssuerOfAccessionNumberSQ.Update(ds);
            ds.AddOrUpdate(DicomTag.RequestingPhysician​, RequestingPhysician​);
            ds.AddOrUpdate(DicomTag.ReferringPhysicianName, ReferringPhysicianName);
            //others not implemented
        }




    }
}
