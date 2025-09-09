//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using FellowOakDicom;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomModule;


namespace NV.CT.DicomUtility.DicomIOD
{
    /// <summary>
    /// XRayRadiationDoseSRIOD
    /// https://dicom.innolitics.com/ciods/x-ray-radiation-dose-sr
    /// 当前实现不包括ContentSequence中的实际内容，只有信息定义中相关Module的信息。
    /// SRDocumentContentModule中的ContentSequence在StructureReportGenerator实际填充。
    /// 具体接口见StructureReportGenerator.GenerateDoseReportSR
    /// </summary>
    public class XRayRadiationDoseSRIOD:IDicomDatasetUpdater
    {
        public PatientModule PatientModule { get; private set; }

        public GeneralStudyModule GeneralStudyModule { get; private set; }

        public SRDocumentSeriesModule SRDocumentSeriesModule { get; private set; }

        public GeneralEquipmentModule GeneralEquipmentModule { get; private set; }

        public EnhancedGeneralEquipmentModule EnhancedGeneralEquipmentModule { get; private set; }

        public SRDocumentGeneralModule SRDocumentGeneralModule { get; private set; }
        public SRDocumentContentModule SRDocumentContentModule { get; private set; }

        public SOPCommonModule SOPCommonModule { get; set; }

        public XRayRadiationDoseSRIOD()
        {
            PatientModule = new PatientModule();
            GeneralStudyModule = new GeneralStudyModule();
            SRDocumentSeriesModule = new SRDocumentSeriesModule();
            GeneralEquipmentModule = new GeneralEquipmentModule();
            EnhancedGeneralEquipmentModule = new EnhancedGeneralEquipmentModule();
            SRDocumentGeneralModule = new SRDocumentGeneralModule();
            SRDocumentContentModule = new SRDocumentContentModule();
            SOPCommonModule = new SOPCommonModule();

            Init();
        }

        /// <summary>
        /// 初始化DoseReport相关固定字段
        /// </summary>
        private void Init()
        {
            SRDocumentContentModule.ConceptNameCodeSequence.CodeValue = "113701";
            SRDocumentContentModule.ConceptNameCodeSequence.CodingSchemeDesignator = "DCM";
            SRDocumentContentModule.ConceptNameCodeSequence.CodeMeaning = "X-Ray Radiation Dose Report";

            SOPCommonModule.SOPClassUID = "1.2.840.10008.5.1.4.1.1.88.67";
            SOPCommonModule.InstanceNumber = 1;
        }

        public void Update(DicomDataset ds)
        {
            PatientModule.Update(ds);
            GeneralStudyModule.Update(ds);
            SRDocumentSeriesModule.Update(ds);
            GeneralEquipmentModule.Update(ds);
            EnhancedGeneralEquipmentModule.Update(ds);
            SRDocumentGeneralModule.Update(ds);
            SRDocumentContentModule.Update(ds);
            SOPCommonModule.Update(ds);
        }

        public void Read(DicomDataset ds)
        {
            PatientModule.Read(ds);
            GeneralStudyModule.Read(ds);
            SRDocumentSeriesModule.Read(ds);
            GeneralEquipmentModule.Read(ds);
            EnhancedGeneralEquipmentModule.Read(ds);
            SRDocumentGeneralModule.Read(ds);
            SRDocumentContentModule.Read(ds);
            SOPCommonModule.Read(ds);
        }
    }
}
