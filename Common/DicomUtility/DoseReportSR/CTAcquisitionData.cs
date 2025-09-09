//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;
using System.Collections.Generic;

namespace NV.CT.DicomUtility.DoseReportSR
{
    /// <summary>
    /// 单次CT曝光请求数据
    /// EV (113819, DCM, "CT Acquisition")
    /// https://dicom.nema.org/medical/dicom/current/output/chtml/part16/sect_TID_10013.html
    /// </summary>
    public class CTAcquisitionData
    {
        /// <summary>
        /// EV(125203, DCM, "Acquisition Protocol")
        /// </summary>
        public string AquisitionProtocol
        {
            get; set;
        }
        /// <summary>
        /// EV (123014, DCM, "Target Region")   与协议中对应的是BodyPart
        /// </summary>
        public BodyPart BodyPart
        {
            get; set;
        }
        /// <summary>
        /// EV (113820, DCM, "CT Acquisition Type")
        /// </summary>
        public ScanOption AcquisitionType
        {
            get; set;
        }

        /// <summary>
        /// EV (113769, DCM, "Irradiation Event UID")
        /// </summary>
        public string IrradiationEventUID
        {
            get;set;
        }

        /// <summary>
        /// EV (408730004, SCT, "Procedure Context")
        /// </summary>
        public bool IsContrast
        {
            get;set;
        }



        public string Comment
        {
            get;set;
        }

        /// <summary>
        /// EV (113824, DCM, "Exposure Time")
        /// </summary>
        public double ExposureTime
        {
            get;set;
        }

        /// <summary>
        /// EV (113825, DCM, "Scanning Length")
        /// </summary>
        public double ScanningLength
        {
            get;set;
        }

        /// <summary>
        /// EV (113826, DCM, "Nominal Single Collimation Width")
        /// </summary>
        public double NominalSingleCollimationWidth
        {
            get;set;
        }

        /// <summary>
        /// EV (113827, DCM, "Nominal Total Collimation Width")
        /// </summary>
        public double TotalSingleCollimationWidth
        {
            get;set;
        }

        /// <summary>
        /// EV (113828, DCM, "Pitch Factor")
        /// </summary>
        public double PitchFactor
        {
            get;set;
        }

        /// <summary>
        /// EV (113823, DCM, "Number of X-Ray Sources")
        /// </summary>
        public int NumberOfXRaySources
        {
            get;set;
        }

        /// <summary>
        /// EV (113831, DCM, "CT X-Ray Source Parameters")
        /// </summary>

        public List<XRaySourceParam> XRaySourceParams
        {
            get;
        }

        /// <summary>
        /// EV (113829, DCM, "CT Dose")
        /// </summary>
        public CTDoseInfo CTDoseInfo { get; set; }

        /// <summary>
        /// EV (113876, DCM, "Device Role in Procedure")
        /// </summary>

        public CTDeviceRoleParticipant CTDeviceRoleParticipant { get; set; }

        public CTAcquisitionData()
        {
            XRaySourceParams = new List<XRaySourceParam>();
        }
    }
}
