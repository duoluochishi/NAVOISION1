//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using FellowOakDicom.StructuredReport;
using NV.CT.DicomUtility.DicomCodeItems;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.DoseReportSR
{
    public static class DoseReportSRHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DicomCodeItem GetTargetRegion(BodyPart bodyPart)
        {
            switch(bodyPart)
            {
                case BodyPart.SPINE:
                    return TargetRegionCodeItem.SPINE_SCT;
                case BodyPart.SHOULDER:
                    return TargetRegionCodeItem.SHOULDER_SCT;
                case BodyPart.PELVIS:
                    return TargetRegionCodeItem.PELVIS_SCT;
                case BodyPart.NECK:
                    return TargetRegionCodeItem.NECK_SCT;
                case BodyPart.HEAD:
                    return TargetRegionCodeItem.HEAD_SCT;
                case BodyPart.LEG:
                    return TargetRegionCodeItem.LEG_SCT;
                case BodyPart.ARM:
                    return TargetRegionCodeItem.ARM_SCT;
                case BodyPart.ABDOMEN:
                    return TargetRegionCodeItem.ABDOMEN_SCT;
                case BodyPart.BREAST:
                    return TargetRegionCodeItem.BREAST_SCT;
                default:
                    return TargetRegionCodeItem.WHOLEBODY_SCT;
            }
        }




        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DicomCodeItem GetCTAcquisitionType(ScanOption scanOption)
        {
            switch (scanOption)
            {
                case ScanOption.Surview:
                case ScanOption.DualScout:
                    return AquisitionTypeCodeItem.ConstantAngle_DCM;
                case ScanOption.Axial:
                    return AquisitionTypeCodeItem.Sequenced_DCM;
                case ScanOption.Helical:
                    return AquisitionTypeCodeItem.Spiral_SCT;
                default:
                    return AquisitionTypeCodeItem.Free_DCM;
            }
        }

        public static (string,string,string) GetProcedureContext(bool isContrast)
        {
            if(isContrast)
            {
                return ("P5-00100", "SRT", "Diagnostic radiography with contrast media");
            }
            else
            {
                return ("P5-0808E", "SRT", "CT without contrast");
            }
        }

        public static (string,string,string) GetPhantomType(int phantomType)
        {
            switch(phantomType)
            {
                case 113681:
                    return ("113681", "DCM", "Phantom");
                case 113682:
                    return ("113682", "DCM", "ACR Accreditation Phantom - CT​");
                case 113683:
                    return ("113683", "DCM", "ACR Accreditation Phantom - MR​");
                case 113684:
                    return ("113684", "DCM", "ACR Accreditation Phantom - Mammography​");
                case 113685:
                    return ("113685", "DCM", "ACR Accreditation Phantom - Stereotactic Breast Biopsy​");
                case 113686:
                    return ("113686", "DCM", "ACR Accreditation Phantom - ECT​");
                case 113687:
                    return ("113687", "DCM", "ACR Accreditation Phantom - PET​");
                case 113688:
                    return ("113688", "DCM", "ACR Accreditation Phantom - ECT/PET");
                case 113689:
                    return ("113689", "DCM", "ACR Accreditation Phantom - PET Faceplate​");
                case 113690:
                    return ("113690", "DCM", "IEC Head Dosimetry Phantom");
                case 113691:
                    return ("113691", "DCM", "IEC Body Dosimetry Phantom​");
                case 113692:
                    return ("113692", "DCM", "NEMA XR21-2000 Phantom​");
                case 130541:
                    return ("130541", "DCM", "10 cm Dosimetry Phantom​");
                default:
                    return ("113681", "DCM", "Phantom");
            }
        }

        public static (string, string, string) GetPhantomTypeDescription(int phantomType)
        {
            switch (phantomType)
            {
                //case 113681:
                //    return ("113681", "DCM", "Phantom");
                //case 113682:
                //    return ("113682", "DCM", "ACR Accreditation Phantom - CT​");
                //case 113683:
                //    return ("113683", "DCM", "ACR Accreditation Phantom - MR​");
                //case 113684:
                //    return ("113684", "DCM", "ACR Accreditation Phantom - Mammography​");
                //case 113685:
                //    return ("113685", "DCM", "ACR Accreditation Phantom - Stereotactic Breast Biopsy​");
                //case 113686:
                //    return ("113686", "DCM", "ACR Accreditation Phantom - ECT​");
                //case 113687:
                //    return ("113687", "DCM", "ACR Accreditation Phantom - PET​");
                //case 113688:
                //    return ("113688", "DCM", "ACR Accreditation Phantom - ECT/PET");
                //case 113689:
                //    return ("113689", "DCM", "ACR Accreditation Phantom - PET Faceplate​");
                //case 113692:
                //    return ("113692", "DCM", "NEMA XR21-2000 Phantom​");
                case 130541:
                    return ("130541", "DCM", "10 cm Phantom​");
                case 113690:
                    return ("113690", "DCM", "IEC Head");
                case 113691:
                    return ("113691", "DCM", "IEC Body");
                default:
                    return ("113681", "DCM", "Phantom");
            }
        }

        public static DicomCodeItem GetYesNo(bool input)
        {
            if (input)
            {
                return YesNoOnlyCodeItem.Yes_SCT;
            }
            return YesNoOnlyCodeItem.No_SCT;
        }

        public static (string,string,string) GetPersonRole(int personRole)
        {
            return ("113850", "DCM", "Irradiation Authorizing");
        }
    }
}
