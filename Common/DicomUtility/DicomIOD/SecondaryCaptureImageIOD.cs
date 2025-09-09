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


using DoseReport.DicomModule;
using FellowOakDicom;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomModule;

namespace NV.CT.DicomUtility.DicomIOD
{
    public class SecondaryCaptureImageIOD : IDicomDatasetUpdater
    {
        public PatientModule PatientModule { get; set; }

        public GeneralStudyModule GeneralStudyModule { get; set; }
        public GeneralSeriesModule GeneralSeriesModule { get; set; }

        public GeneralEquipmentModule GeneralEquipmentModule { get; set; }

        public ImagePixelModule ImagePixelModule { get; set; }

        public GeneralImageModule GeneralImageModule { get; set; }

        public SOPCommonModule SOPCommonModule { get; set; }

        public CTImageModule CTImageModule { get; set; }

        public ImagePlaneModule ImagePlaneModule { get; set; }  

        public VOILUTModule VOILUTModule { get; set; }  



        public SecondaryCaptureImageIOD()
        {
            PatientModule = new PatientModule();
            GeneralStudyModule = new GeneralStudyModule();
            GeneralSeriesModule = new GeneralSeriesModule();
            GeneralEquipmentModule = new GeneralEquipmentModule();
            ImagePixelModule = new ImagePixelModule();
            GeneralImageModule = new GeneralImageModule();
            SOPCommonModule = new SOPCommonModule();
            CTImageModule = new CTImageModule();
            ImagePlaneModule = new ImagePlaneModule();
            VOILUTModule = new VOILUTModule();
        }

        public void Update(DicomDataset ds)
        {
            PatientModule.Update(ds);
            GeneralStudyModule.Update(ds);
            GeneralSeriesModule.Update(ds);
            GeneralEquipmentModule.Update(ds);
            ImagePixelModule.Update(ds);
            GeneralImageModule.Update(ds);
            SOPCommonModule.Update(ds);
            CTImageModule.Update(ds);
            ImagePlaneModule.Update(ds);
            VOILUTModule.Update(ds);
        }
        public void Read(DicomDataset ds)
        {
            PatientModule.Read(ds);
            GeneralStudyModule.Read(ds);
            GeneralSeriesModule.Read(ds);
            GeneralEquipmentModule.Read(ds);
            ImagePixelModule.Read(ds);
            GeneralImageModule.Read(ds);
            SOPCommonModule.Read(ds);
            CTImageModule.Read(ds);
            ImagePlaneModule.Read(ds);
            VOILUTModule.Read(ds);
        }
    }
}
