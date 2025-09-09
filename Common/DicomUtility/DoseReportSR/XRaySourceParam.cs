//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class XRaySourceParam
    {
        /// <summary>
        /// EV (113832, DCM, "Identification of the X-Ray Source")
        /// </summary>
        public string IdentificationXRaySource
        {
            get;set;
        }
        /// <summary>
        /// EV(113733, DCM, "KVP")
        /// </summary>
        public double KVP
        {
            get;set;
        }
        /// <summary>
        /// EV (113833, DCM, "Maximum X-Ray Tube Current")
        /// </summary>
        public double MaxTubeCurrent
        {
            get;set;
        }
        /// <summary>
        /// EV (113734, DCM, "X-Ray Tube Current")
        /// </summary>
        public double TubeCurrent
        {
            get;set;
        }
        /// <summary>
        /// EV (113834, DCM, "Exposure Time per Rotation")
        /// </summary>
        public double ExposureTimePerRotate
        { 
            get; set; 
        }

    }

}
